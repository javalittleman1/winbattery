using System.Management;

namespace WinBattery.Core;

public static class BatteryService
{
    public static bool HasBattery()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Battery");
            return searcher.Get().Count > 0;
        }
        catch
        {
            return false;
        }
    }

    public static BatteryInfo GetBatteryInfo()
    {
        var info = new BatteryInfo();
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Battery");
            foreach (ManagementObject obj in searcher.Get())
            {
                info.DeviceID = obj["DeviceID"]?.ToString();
                info.Name = obj["Name"]?.ToString();
                info.Manufacturer = obj["Manufacturer"]?.ToString();
                info.SerialNumber = obj["PNPDeviceID"]?.ToString() ?? obj["DeviceID"]?.ToString();
                info.Chemistry = obj["Chemistry"]?.ToString();
                info.DesignCapacity = GetUShort(obj["DesignCapacity"]);
                info.FullChargeCapacity = GetUShort(obj["FullChargeCapacity"]);
                info.RemainingCapacity = GetUShort(obj["RemainingCapacity"]);
                info.DesignVoltage = GetUShort(obj["DesignVoltage"]);
                info.EstimatedChargeRemaining = GetUInt(obj["EstimatedChargeRemaining"]);
                info.EstimatedRunTime = GetUInt(obj["EstimatedRunTime"]);
                info.BatteryStatus = GetUShort(obj["BatteryStatus"]);
                info.PowerOnLine = GetBool(obj["PowerOnLine"]);
                info.Charging = GetBool(obj["Charging"]);
                info.Discharging = GetBool(obj["Discharging"]);
                info.Temperature = GetUShort(obj["Temperature"]);
                info.CycleCount = GetUInt(obj["CycleCount"]);

                // 若 FullChargeCapacity 缺失，尝试用 DesignCapacity 和 EstimatedChargeRemaining 估算
                if (!info.FullChargeCapacity.HasValue && info.DesignCapacity.HasValue && info.EstimatedChargeRemaining.HasValue)
                {
                    // 不估算，保持缺失
                }
            }
        }
        catch { }

        // 尝试从 powercfg /batteryreport 或额外 WMI 获取循环次数和温度
        TryEnrichFromPowercfg(info);
        return info;
    }

    private static void TryEnrichFromPowercfg(BatteryInfo info)
    {
        try
        {
            var tempPath = Path.Combine(Path.GetTempPath(), "wbattery_report.xml");
            var psi = new System.Diagnostics.ProcessStartInfo("powercfg", "/batteryreport /xml /output \"" + tempPath + "\"")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
            };
            using var proc = System.Diagnostics.Process.Start(psi);
            proc?.WaitForExit(5000);

            if (File.Exists(tempPath))
            {
                var doc = new System.Xml.XmlDocument();
                doc.Load(tempPath);
                var nsMgr = new System.Xml.XmlNamespaceManager(doc.NameTable);
                nsMgr.AddNamespace("b", "http://schemas.microsoft.com/battery/2012");

                // 尝试读取 CycleCount
                if (!info.CycleCount.HasValue)
                {
                    var cycleNode = doc.SelectSingleNode("//b:BatteryCycleCount", nsMgr);
                    if (cycleNode != null && uint.TryParse(cycleNode.InnerText, out var cc))
                        info.CycleCount = cc;
                }

                // 尝试读取温度
                if (!info.Temperature.HasValue)
                {
                    var tempNode = doc.SelectSingleNode("//b:Temperature", nsMgr);
                    if (tempNode != null && uint.TryParse(tempNode.InnerText, out var t))
                        info.Temperature = (ushort)(t / 10); // 通常以 0.1K 为单位
                }

                // 尝试读取 FullChargeCapacity
                if (!info.FullChargeCapacity.HasValue)
                {
                    var fccNode = doc.SelectSingleNode("//b:FullChargeCapacity", nsMgr);
                    if (fccNode != null && uint.TryParse(fccNode.InnerText, out var fcc))
                        info.FullChargeCapacity = (ushort)fcc;
                }

                // 尝试读取 DesignCapacity
                if (!info.DesignCapacity.HasValue)
                {
                    var dcNode = doc.SelectSingleNode("//b:DesignCapacity", nsMgr);
                    if (dcNode != null && uint.TryParse(dcNode.InnerText, out var dc))
                        info.DesignCapacity = (ushort)dc;
                }

                try { File.Delete(tempPath); } catch { }
            }
        }
        catch { }
    }

    private static ushort? GetUShort(object? value)
    {
        if (value == null) return null;
        if (value is ushort u) return u;
        if (ushort.TryParse(value.ToString(), out var result)) return result;
        return null;
    }

    private static uint? GetUInt(object? value)
    {
        if (value == null) return null;
        if (value is uint u) return u;
        if (uint.TryParse(value.ToString(), out var result)) return result;
        return null;
    }

    private static bool? GetBool(object? value)
    {
        if (value == null) return null;
        if (value is bool b) return b;
        if (bool.TryParse(value.ToString(), out var result)) return result;
        return null;
    }

    public static List<ProcessPowerInfo> GetProcessPowerUsage()
    {
        var list = new List<ProcessPowerInfo>();
        try
        {
            // 使用 WMI 性能计数器获取真实的进程 CPU 使用率
            using var searcher = new ManagementObjectSearcher(
                "SELECT IDProcess, Name, PercentProcessorTime FROM Win32_PerfFormattedData_PerfProc_Process WHERE IDProcess > 0");
            var data = new List<(string Name, int Cpu)>();
            foreach (ManagementObject obj in searcher.Get())
            {
                var name = obj["Name"]?.ToString() ?? "Unknown";
                var pid = Convert.ToInt32(obj["IDProcess"]);
                var cpu = Convert.ToInt32(obj["PercentProcessorTime"]);
                if (cpu > 0)
                    data.Add((name, cpu));
            }

            // 获取进程友好名称映射
            var friendlyMap = new Dictionary<int, string>();
            try
            {
                using var procSearcher = new ManagementObjectSearcher("SELECT ProcessId, Name FROM Win32_Process");
                foreach (ManagementObject obj in procSearcher.Get())
                {
                    var pid = Convert.ToInt32(obj["ProcessId"]);
                    var name = obj["Name"]?.ToString() ?? "Unknown";
                    friendlyMap[pid] = name;
                }
            }
            catch { }

            // 按 CPU 使用率排序，取前 10
            var sorted = data.OrderByDescending(x => x.Cpu).Take(10).ToList();
            var totalCpu = sorted.Sum(x => x.Cpu);
            if (totalCpu == 0) totalCpu = 1;

            var displayNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["chrome.exe"] = "Google Chrome",
                ["msedge.exe"] = "Microsoft Edge",
                ["firefox.exe"] = "Mozilla Firefox",
                ["Code.exe"] = "Visual Studio Code",
                ["devenv.exe"] = "Visual Studio",
                ["explorer.exe"] = "Windows Explorer",
                ["dwm.exe"] = "Desktop Window Manager",
                ["svchost.exe"] = "System Service Host",
                ["System"] = "System Kernel",
                ["Idle"] = "System Idle",
                ["powershell.exe"] = "PowerShell",
                ["cmd.exe"] = "Command Prompt",
                ["notepad.exe"] = "Notepad",
                ["searchindexer.exe"] = "Windows Search",
                ["antimalwareserviceexecutable"] = "Windows Defender",
            };

            foreach (var item in sorted)
            {
                var baseName = item.Name.Replace(".exe", "", StringComparison.OrdinalIgnoreCase) + ".exe";
                if (!baseName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                    baseName += ".exe";

                var display = displayNames.TryGetValue(baseName, out var dn) ? dn : item.Name;
                int percent = (int)Math.Round(item.Cpu * 100.0 / totalCpu);
                list.Add(new ProcessPowerInfo { ProcessName = display, PowerPercent = percent, CpuPercent = item.Cpu });
            }

            // 补齐 Others
            var accounted = list.Sum(x => x.PowerPercent);
            if (accounted < 100 && accounted > 0)
                list.Add(new ProcessPowerInfo { ProcessName = "Others", PowerPercent = 100 - accounted, CpuPercent = 0 });
        }
        catch (Exception ex)
        {
            // 如果 WMI 性能计数器不可用，回退到基础进程列表
            list = GetFallbackProcessList();
        }
        return list.OrderByDescending(x => x.PowerPercent).ToList();
    }

    private static List<ProcessPowerInfo> GetFallbackProcessList()
    {
        var list = new List<ProcessPowerInfo>();
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Name, KernelModeTime, UserModeTime FROM Win32_Process");
            var data = new List<(string Name, long Time)>();
            foreach (ManagementObject obj in searcher.Get())
            {
                var name = obj["Name"]?.ToString() ?? "Unknown";
                var kt = Convert.ToInt64(obj["KernelModeTime"] ?? 0);
                var ut = Convert.ToInt64(obj["UserModeTime"] ?? 0);
                data.Add((name, kt + ut));
            }

            var sorted = data.OrderByDescending(x => x.Time).Take(10).ToList();
            var total = sorted.Sum(x => x.Time);
            if (total == 0) total = 1;

            foreach (var item in sorted)
            {
                int percent = (int)Math.Round(item.Time * 100.0 / total);
                list.Add(new ProcessPowerInfo { ProcessName = item.Name, PowerPercent = percent });
            }

            var accounted = list.Sum(x => x.PowerPercent);
            if (accounted < 100 && accounted > 0)
                list.Add(new ProcessPowerInfo { ProcessName = "Others", PowerPercent = 100 - accounted });
        }
        catch { }
        return list;
    }

    public static double? GetPowerNow(BatteryInfo info)
    {
        try
        {
            // 尝试从电池报告获取当前放电/充电速率
            var tempPath = Path.Combine(Path.GetTempPath(), "wbattery_power.xml");
            var psi = new System.Diagnostics.ProcessStartInfo("powercfg", "/batteryreport /xml /output \"" + tempPath + "\"")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
            };
            using var proc = System.Diagnostics.Process.Start(psi);
            proc?.WaitForExit(5000);

            if (File.Exists(tempPath))
            {
                var doc = new System.Xml.XmlDocument();
                doc.Load(tempPath);
                var nsMgr = new System.Xml.XmlNamespaceManager(doc.NameTable);
                nsMgr.AddNamespace("b", "http://schemas.microsoft.com/battery/2012");

                // 读取最近一条记录的速率
                var rateNode = doc.SelectSingleNode("//b:RecentUsage/b:Usage[1]/b:Rate", nsMgr);
                if (rateNode != null && int.TryParse(rateNode.InnerText, out var rate))
                {
                    try { File.Delete(tempPath); } catch { }
                    return Math.Abs(rate) / 1000.0; // mW -> W
                }

                try { File.Delete(tempPath); } catch { }
            }
        }
        catch { }

        // 回退：通过电压和容量变化估算
        if (info.DesignVoltage.HasValue && info.RemainingCapacity.HasValue)
        {
            // 这是一个粗略估算，假设典型笔记本电脑功耗范围
            return null; // 不返回假数据
        }
        return null;
    }
}

public class ProcessPowerInfo
{
    public string ProcessName { get; set; } = "";
    public int PowerPercent { get; set; }
    public int CpuPercent { get; set; }
}
