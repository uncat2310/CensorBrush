# CensorBrush 打码笔

一个单文件、免安装的 Windows 图片打码小工具：打开图片，拿鼠标像笔一样把马赛克涂上去，涂错了当场擦掉，存为新图。全程在原图分辨率上处理，不走截图流程，不掉画质。

全部程序就是一个 `CensorBrush.exe`（约 183 KB），拷到任何 Windows 10 / 11 电脑上双击就能用。**编译好的 exe 就放在本仓库根目录**，下载它即可，无需自己构建。

## 使用方法

| 操作 | 做法 |
| --- | --- |
| 打开图片 | 点左上角文件夹图标，或把图片直接拖进窗口，或双击空白画布 |
| 涂抹打码 | 按住**左键**拖动 |
| 擦除还原 | 按住**右键**拖动，或切到「还原」模式用左键擦 |
| 马赛克粗细 | 顶部滑杆调节块大小，右侧胶囊是当前数值 |
| 缩放 | Ctrl + 滚轮，或状态栏右下角的 − / + 与百分比（点百分比 = 适合窗口） |
| 撤销 | Ctrl + Z（最多 20 步，且总内存不超过 512 MB） |
| 保存 | Ctrl + S 快速保存为「原文件名_censor.png」；「另存为」可选择 PNG 或 JPEG |

## 特性

- **紧贴笔触的马赛克**：像素块只生成在涂抹经过的地方，不会糊住整片区域，颗粒大小可调
- **原分辨率处理**：打开原图、原尺寸输出，不经过截图，不压缩画质
- **界面清爽**：Windows 11 / 记事本式极简图标命令栏，跟随系统深色 / 浅色模式
- **高 DPI 清晰**：内置 PerMonitorV2 清单，高分屏缩放下文字和图标不模糊
- **流畅**：涂抹时只刷新笔刷附近的区域，大图拖动不卡顿
- **安静**：不联网、无遥测、不写注册表（仅读取一次系统主题设置以跟随深浅色），不收集任何数据

## 系统要求

- Windows 10 或 Windows 11
- 使用系统自带的 .NET Framework 4.8，无需安装任何运行时或依赖

## 关于 SmartScreen 提示

本软件未购买代码签名证书。第一次运行时，Windows 可能弹出蓝色的 SmartScreen 提示（「Windows 已保护你的电脑」），这是未签名程序的正常待遇：点「更多信息」→「仍要运行」即可，只会提示这一次。

## 从源码构建

源码是单个 C# 文件（WinForms / .NET Framework）。在本仓库目录下运行：

```powershell
powershell -ExecutionPolicy Bypass -File build.ps1
```

或手动执行一条命令：

```powershell
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /nologo /target:winexe /optimize+ /win32manifest:CensorBrush.manifest /reference:System.dll /reference:System.Drawing.dll /reference:System.Windows.Forms.dll /out:CensorBrush.exe CensorBrush.cs
```

说明：

- `CensorBrush.manifest` 提供高 DPI 感知，构建时请保留
- `CensorBrush.ico`（应用图标）已随仓库分发，`build.ps1` 会自动嵌入，构建出的 exe 与仓库里的成品完全一致

## 文件一览

| 文件 | 说明 |
| --- | --- |
| `CensorBrush.exe` | 编译好的成品，下载双击即用 |
| `CensorBrush.cs` | 全部源码（单文件，约 1500 行） |
| `CensorBrush.ico` | 应用图标（构建时嵌入 exe） |
| `CensorBrush.manifest` | 应用清单（PerMonitorV2 高 DPI） |
| `build.ps1` | 一键构建脚本 |
| `LICENSE` | MIT 许可证 |

## License

[MIT](LICENSE)
