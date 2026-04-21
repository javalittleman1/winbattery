# WinBattery 电池管家

一款专注于 Windows 笔记本电池监控的轻量桌面工具，数据全部来自系统原生 WMI 接口与 `powercfg` 命令，无广告、无联网、无后台。

## 功能特性

- **电池总览**：健康度、损耗率、当前电量、剩余续航、循环次数
- **详细参数**：电压、温度、化学类型、制造商、序列号等全部 WMI 字段
- **充放电曲线**：基于真实电量历史数据绘制的 3 小时趋势图
- **耗电排行**：使用真实 WMI 进程 CPU 使用率作为功耗代理指标排序
- **悬浮球**：置顶小窗实时显示电量/健康度/状态，支持鼠标拖动
- **中英双语**：全界面 100% 文本覆盖，实时切换无需重启
- **主题系统**：浅色 / 深色 / 跟随 Windows 系统，自动同步切换
- **工具箱**：一键生成 Windows 官方电池报告、导出数据、开机自启

## 运行环境

- Windows 10 1909+ / Windows 11
- .NET 9 运行时（如未安装，单文件版已自带）
- 仅支持内置电池的笔记本电脑

## 快速启动

```bash
dotnet run
```

## 发布单文件 exe

```bash
dotnet publish -r win-x64 -c Release --self-contained true \
  /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
```

产物位于 `bin/Release/net9.0-windows/win-x64/publish/WinBattery.exe`。

## 项目结构

```
WinBattery/
├── Core/
│   ├── BatteryInfo.cs        # 电池数据模型
│   ├── BatteryService.cs     # WMI + powercfg 数据读取
│   ├── ConfigService.cs      # 本地 JSON 配置持久化
│   ├── HistoryService.cs     # 电量历史记录本地存储
│   ├── I18nService.cs        # 中英双语字典
│   └── ThemeService.cs       # 主题管理与系统主题监听
├── Pages/
│   ├── OverviewPage.cs       # 总览
│   ├── DetailsPage.cs        # 详细参数
│   ├── ChartPage.cs          # 充放电曲线
│   ├── UsagePage.cs          # 耗电排行
│   └── SettingsPage.cs       # 历史与设置
├── Controls/
│   └── CardPanel.cs          # 圆角卡片控件
├── FloatingForm.cs           # 悬浮窗
├── MainForm.cs               # 主窗体
└── Program.cs                # 入口
```

## 技术栈

- .NET 9 WinForms
- System.Management (WMI)
- GDI+ 自定义绘图（曲线图、圆角卡片）
- Registry 系统主题监听

## 许可证

MIT
