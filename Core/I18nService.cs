namespace WinBattery.Core;

public static class I18nService
{
    public static string Lang { get; private set; } = "zh";

    public static event Action? LanguageChanged;

    private static readonly Dictionary<string, Dictionary<string, string>> Dict = new()
    {
        ["zh"] = new Dictionary<string, string>
        {
            ["appName"] = "WinBattery 电池管家",
            ["menu_overview"] = "总览",
            ["menu_details"] = "详细参数",
            ["menu_chart"] = "充放电曲线",
            ["menu_usage"] = "耗电排行",
            ["menu_settings"] = "历史与设置",
            ["battery_health"] = "电池健康度",
            ["cycle_count"] = "充电循环次数",
            ["battery_wear"] = "电池损耗",
            ["full_capacity"] = "当前满充容量",
            ["design_capacity"] = "设计容量",
            ["battery_status"] = "电池状态",
            ["status_charging"] = "正在充电 / 已接通电源",
            ["status_discharging"] = "正在放电 / 使用电池供电",
            ["status_plugged"] = "已接通电源 / 待机中",
            ["status_unknown"] = "状态未知",
            ["health_tip"] = "健康状态建议",
            ["tip_excellent"] = "电池状态优秀，请继续保持良好使用习惯。",
            ["tip_good"] = "电池状态良好，正常使用即可。建议保持20%~80%充电区间延长寿命。",
            ["tip_fair"] = "电池出现一定损耗，建议避免长时间满充或完全放电。",
            ["tip_poor"] = "电池损耗较严重，建议考虑更换电池以获得更好续航。",
            ["hardware_info"] = "电池硬件信息",
            ["manufacturer"] = "制造商",
            ["battery_model"] = "电池型号",
            ["serial"] = "序列号",
            ["chemistry"] = "化学类型",
            ["voltage"] = "设计电压",
            ["temperature"] = "电池温度",
            ["power_now"] = "当前功耗",
            ["charge_curve"] = "充放电曲线（最近3小时）",
            ["chart_placeholder"] = "图表区域：电量/功率变化曲线",
            ["last_charge"] = "最近一次充电记录",
            ["charge_log"] = "2025-04-20 10:00 - 12:30 | 20% → 100%",
            ["power_usage"] = "耗电应用排行",
            ["history"] = "健康趋势记录",
            ["settings"] = "软件设置",
            ["auto_start"] = "开机自启动",
            ["float_window"] = "悬浮窗",
            ["refresh_rate"] = "刷新频率",
            ["version"] = "版本号",
            ["language"] = "语言设置",
            ["theme"] = "主题设置",
            ["theme_system"] = "跟随系统",
            ["theme_light"] = "浅色",
            ["theme_dark"] = "深色",
            ["current_charge"] = "当前电量",
            ["remaining_time"] = "剩余时间",
            ["health_excellent"] = "优秀",
            ["health_good"] = "良好",
            ["health_fair"] = "一般",
            ["health_poor"] = "较差",
            ["no_battery"] = "未检测到电池",
            ["no_battery_tip"] = "当前设备未检测到内置电池，可能为台式机。",
            ["unknown"] = "未知",
            ["minutes"] = "分钟",
            ["hours"] = "小时",
            ["on"] = "开启",
            ["off"] = "关闭",
            ["save"] = "保存",
            ["generate_report"] = "生成电池报告",
            ["report_generated"] = "电池报告已生成",
            ["data_export"] = "导出数据",
            ["second"] = "秒",
            ["refresh_interval"] = "刷新间隔",
        },
        ["en"] = new Dictionary<string, string>
        {
            ["appName"] = "WinBattery Manager",
            ["menu_overview"] = "Overview",
            ["menu_details"] = "Details",
            ["menu_chart"] = "Charge Curve",
            ["menu_usage"] = "Power Usage",
            ["menu_settings"] = "History & Settings",
            ["battery_health"] = "Battery Health",
            ["cycle_count"] = "Cycle Count",
            ["battery_wear"] = "Battery Wear",
            ["full_capacity"] = "Full Charge Capacity",
            ["design_capacity"] = "Design Capacity",
            ["battery_status"] = "Battery Status",
            ["status_charging"] = "Charging / Power Connected",
            ["status_discharging"] = "Discharging / On Battery",
            ["status_plugged"] = "Plugged In / Standby",
            ["status_unknown"] = "Unknown Status",
            ["health_tip"] = "Health Suggestion",
            ["tip_excellent"] = "Battery is in excellent condition. Keep up the good habits.",
            ["tip_good"] = "Battery is healthy. Keep 20%-80% charge for longer life.",
            ["tip_fair"] = "Some wear detected. Avoid keeping at 100% or 0% for long.",
            ["tip_poor"] = "Battery degraded significantly. Consider replacing it for better endurance.",
            ["hardware_info"] = "Battery Hardware Info",
            ["manufacturer"] = "Manufacturer",
            ["battery_model"] = "Model",
            ["serial"] = "Serial Number",
            ["chemistry"] = "Chemistry",
            ["voltage"] = "Design Voltage",
            ["temperature"] = "Temperature",
            ["power_now"] = "Current Power",
            ["charge_curve"] = "Charge/Discharge Curve (3h)",
            ["chart_placeholder"] = "Chart: Battery & Power Trend",
            ["last_charge"] = "Last Charge Record",
            ["charge_log"] = "2025-04-20 10:00 - 12:30 | 20% → 100%",
            ["power_usage"] = "Power Usage Ranking",
            ["history"] = "Health History",
            ["settings"] = "Settings",
            ["auto_start"] = "Auto Start",
            ["float_window"] = "Floating Window",
            ["refresh_rate"] = "Refresh Rate",
            ["version"] = "Version",
            ["language"] = "Language",
            ["theme"] = "Theme",
            ["theme_system"] = "System",
            ["theme_light"] = "Light",
            ["theme_dark"] = "Dark",
            ["current_charge"] = "Current Charge",
            ["remaining_time"] = "Remaining Time",
            ["health_excellent"] = "Excellent",
            ["health_good"] = "Good",
            ["health_fair"] = "Fair",
            ["health_poor"] = "Poor",
            ["no_battery"] = "No Battery Detected",
            ["no_battery_tip"] = "No internal battery detected. This may be a desktop PC.",
            ["unknown"] = "Unknown",
            ["minutes"] = "min",
            ["hours"] = "hours",
            ["on"] = "On",
            ["off"] = "Off",
            ["save"] = "Save",
            ["generate_report"] = "Generate Report",
            ["report_generated"] = "Battery report generated",
            ["data_export"] = "Export Data",
            ["second"] = "s",
            ["refresh_interval"] = "Refresh Interval",
        }
    };

    public static void SetLanguage(string lang)
    {
        if (Dict.ContainsKey(lang))
        {
            Lang = lang;
            LanguageChanged?.Invoke();
        }
    }

    public static string T(string key)
    {
        if (Dict.TryGetValue(Lang, out var d) && d.TryGetValue(key, out var val))
            return val;
        if (Dict["en"].TryGetValue(key, out var enVal))
            return enVal;
        return key;
    }

    public static string DetectSystemLanguage()
    {
        var ci = System.Globalization.CultureInfo.CurrentUICulture;
        return ci.Name.StartsWith("zh", StringComparison.OrdinalIgnoreCase) ? "zh" : "en";
    }
}
