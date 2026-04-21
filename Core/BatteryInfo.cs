namespace WinBattery.Core;

public class BatteryInfo
{
    // 基础信息
    public string? DeviceID { get; set; }
    public string? Name { get; set; }
    public string? Manufacturer { get; set; }
    public string? SerialNumber { get; set; }
    public string? Chemistry { get; set; }
    public ushort? DesignCapacity { get; set; }
    public ushort? FullChargeCapacity { get; set; }
    public ushort? RemainingCapacity { get; set; }
    public ushort? DesignVoltage { get; set; }
    public uint? EstimatedChargeRemaining { get; set; }
    public uint? EstimatedRunTime { get; set; }
    public ushort? BatteryStatus { get; set; }
    public bool? PowerOnLine { get; set; }
    public bool? Charging { get; set; }
    public bool? Discharging { get; set; }
    public ushort? Temperature { get; set; }
    public uint? CycleCount { get; set; }

    // 计算属性
    public double? HealthPercent
    {
        get
        {
            if (FullChargeCapacity.HasValue && DesignCapacity.HasValue && DesignCapacity.Value > 0)
                return Math.Round((double)FullChargeCapacity.Value / DesignCapacity.Value * 100.0, 1);
            return null;
        }
    }

    public double? WearPercent
    {
        get
        {
            var health = HealthPercent;
            return health.HasValue ? Math.Round(100.0 - health.Value, 1) : null;
        }
    }

    public string GetStatusText()
    {
        if (Charging == true) return "Charging";
        if (Discharging == true) return "Discharging";
        if (PowerOnLine == true) return "PluggedIn";
        return "Unknown";
    }

    public string GetChemistryText()
    {
        return Chemistry switch
        {
            "Li-I" or "Li-ion" or "LION" => "Li-ion",
            "NiMH" or "Ni-MH" => "NiMH",
            "NiCd" or "Ni-Cd" => "NiCd",
            "LiPo" or "Li-Po" => "Li-Po",
            _ => Chemistry ?? "Unknown"
        };
    }
}
