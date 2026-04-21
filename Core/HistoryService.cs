using System.Text.Json;

namespace WinBattery.Core;

public class HistoryRecord
{
    public DateTime Time { get; set; }
    public int ChargePercent { get; set; }
    public double? HealthPercent { get; set; }
}

public static class HistoryService
{
    private static readonly string HistoryPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "WinBattery", "history.json");

    public static List<HistoryRecord> Load()
    {
        try
        {
            if (File.Exists(HistoryPath))
            {
                var json = File.ReadAllText(HistoryPath);
                var list = JsonSerializer.Deserialize<List<HistoryRecord>>(json);
                if (list != null) return list;
            }
        }
        catch { }
        return new List<HistoryRecord>();
    }

    public static void Save(List<HistoryRecord> records)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(HistoryPath)!);
            var json = JsonSerializer.Serialize(records, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(HistoryPath, json);
        }
        catch { }
    }

    public static void AppendCurrent(BatteryInfo info)
    {
        if (info?.EstimatedChargeRemaining == null) return;
        var records = Load();
        records.Add(new HistoryRecord
        {
            Time = DateTime.Now,
            ChargePercent = (int)info.EstimatedChargeRemaining.Value,
            HealthPercent = info.HealthPercent
        });
        // 只保留最近 7 天的数据（约 20160 条，每 30 秒一条）
        var cutoff = DateTime.Now.AddDays(-7);
        records.RemoveAll(r => r.Time < cutoff);
        Save(records);
    }

    public static List<int> GetRecentChargeHistory(int hours)
    {
        var records = Load();
        var cutoff = DateTime.Now.AddHours(-hours);
        return records
            .Where(r => r.Time >= cutoff)
            .OrderBy(r => r.Time)
            .Select(r => r.ChargePercent)
            .ToList();
    }

    public static List<(string Date, string Health)> GetHealthHistory()
    {
        var records = Load();
        // 按天去重，取每天最后一次的健康度
        return records
            .GroupBy(r => r.Time.ToString("yyyy-MM-dd"))
            .Select(g => (g.Key, g.LastOrDefault(r => r.HealthPercent.HasValue)?.HealthPercent))
            .Where(x => x.Item2.HasValue)
            .Select(x => (x.Key, $"{x.Item2.Value:F0}%"))
            .ToList();
    }
}
