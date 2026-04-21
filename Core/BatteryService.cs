using System.Management;

namespace WinBattery.Core;

public static class BatteryService
{
    // powercfg 缓存
    private static BatteryInfo? _cachedInfo;
    private static DateTime _lastCacheTime = DateTime.MinValue;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(60);

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
                // WMI 返回的是 uint16，但值可能很大，用 GetUInt 避免溢出截断
                info.DesignCapacity = GetUInt(obj["DesignCapacity"]);
                info.FullChargeCapacity = GetUInt(obj["FullChargeCapacity"]);
                info.RemainingCapacity = GetUInt(obj["RemainingCapacity"]);
                info.DesignVoltage = GetUInt(obj["DesignVoltage"]);
                info.EstimatedChargeRemaining = GetUInt(obj["EstimatedChargeRemaining"]);
                info.EstimatedRunTime = GetUInt(obj["EstimatedRunTime"]);
                info.BatteryStatus = GetUShort(obj["BatteryStatus"]);
                info.PowerOnLine = GetBool(obj["PowerOnLine"]);
                info.Charging = GetBool(obj["Charging"]);
                info.Discharging = GetBool(obj["Discharging"]);
                var rawTemp = GetUInt(obj["Temperature"]);
                // WMI Temperature 通常是 0.1°C 单位，转换为摄氏度
                if (rawTemp.HasValue && rawTemp.Value > 100)
                    info.Temperature = rawTemp.Value / 10;
                else
                    info.Temperature = rawTemp;
                info.CycleCount = GetUInt(obj["CycleCount"]);
            }
        }
        catch { }

        // 使用缓存的 powercfg 数据补充（避免每 5 秒都执行 powercfg）
        EnrichFromCacheOrPowercfg(info);
        return info;
    }

    private static void EnrichFromCacheOrPowercfg(BatteryInfo info)
    {
        // 如果缓存未过期，直接使用缓存补充
        if (_cachedInfo != null && DateTime.Now - _lastCacheTime < CacheDuration)
        {
            if (!info.CycleCount.HasValue) info.CycleCount = _cachedInfo.CycleCount;
            if (!info.Temperature.HasValue) info.Temperature = _cachedInfo.Temperature;
            if (!info.FullChargeCapacity.HasValue) info.FullChargeCapacity = _cachedInfo.FullChargeCapacity;
            if (!info.DesignCapacity.HasValue) info.DesignCapacity = _cachedInfo.DesignCapacity;
            return;
        }

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

                // 更新缓存对象
                _cachedInfo ??= new BatteryInfo();

                // 尝试读取 CycleCount
                var cycleNode = doc.SelectSingleNode("//b:BatteryCycleCount", nsMgr);
                if (cycleNode != null && uint.TryParse(cycleNode.InnerText, out var cc))
                {
                    _cachedInfo.CycleCount = cc;
                    if (!info.CycleCount.HasValue) info.CycleCount = cc;
                }

                // 尝试读取温度
                var tempNode = doc.SelectSingleNode("//b:Temperature", nsMgr);
                if (tempNode != null && uint.TryParse(tempNode.InnerText, out var t))
                {
                    // 电池报告温度通常以 0.1K 为单位，转换为摄氏度
                    var celsius = (t / 10.0) - 273.15;
                    _cachedInfo.Temperature = (uint)Math.Round(celsius);
                    if (!info.Temperature.HasValue) info.Temperature = _cachedInfo.Temperature;
                }

                // 尝试读取 FullChargeCapacity
                var fccNode = doc.SelectSingleNode("//b:FullChargeCapacity", nsMgr);
                if (fccNode != null && uint.TryParse(fccNode.InnerText, out var fcc))
                {
                    _cachedInfo.FullChargeCapacity = fcc;
                    if (!info.FullChargeCapacity.HasValue) info.FullChargeCapacity = fcc;
                }

                // 尝试读取 DesignCapacity
                var dcNode = doc.SelectSingleNode("//b:DesignCapacity", nsMgr);
                if (dcNode != null && uint.TryParse(dcNode.InnerText, out var dc))
                {
                    _cachedInfo.DesignCapacity = dc;
                    if (!info.DesignCapacity.HasValue) info.DesignCapacity = dc;
                }

                _lastCacheTime = DateTime.Now;
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
        if (value is int i) return (uint)i;
        if (value is ushort us) return us;
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

    // 功耗缓存
    private static double? _cachedPower;
    private static DateTime _lastPowerCacheTime = DateTime.MinValue;

    public static double? GetPowerNow(BatteryInfo info)
    {
        // 使用缓存避免频繁调用 powercfg
        if (_cachedPower.HasValue && DateTime.Now - _lastPowerCacheTime < CacheDuration)
            return _cachedPower;

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
                    _cachedPower = Math.Abs(rate) / 1000.0; // mW -> W
                    _lastPowerCacheTime = DateTime.Now;
                    try { File.Delete(tempPath); } catch { }
                    return _cachedPower;
                }

                try { File.Delete(tempPath); } catch { }
            }
        }
        catch { }

        return null; // 不返回假数据
    }
}

public class ProcessPowerInfo
{
    public string ProcessName { get; set; } = "";
    public int PowerPercent { get; set; }
    public int CpuPercent { get; set; }
}
