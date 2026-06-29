# 畅捷通 Stream Gateway .NET SDK - 快速入门演示 (Demo)

本目录包含一个可直接运行的跨平台控制台应用程序，用于演示如何使用 `Chanjet.Connector.Sdk` 与畅捷通开放平台的流式网关建立 WebSocket 连接并接收业务消息。

## 🛠️ 环境准备

1. **.NET SDK**: 请确保您的系统中安装了 [**.NET 6.0 / 8.0 SDK**](https://dotnet.microsoft.com/download) 或更高版本。
   - 验证安装：在终端执行 `dotnet --version`，如果输出版本号即代表环境就绪。

## ⚙️ 凭证配置

Demo 程序通过读取**环境变量**来获取启动凭证。您可以在启动前通过命令行设置，或者在 IDE 中配置运行环境变量。

必须配置的环境变量：
- `APP_KEY`: 您在畅捷通开放平台创建的应用 Key。
- `APP_SECRET`: 您在畅捷通开放平台获取的应用 Secret（通常为32位字符）。

可选配置：
- `GATEWAY_URL`: 测试环境网关地址。若不配置，SDK 将默认连接到**生产环境**。

## 🚀 启动方式

### 方式 1：通过命令行运行 (推荐)

打开终端，进入本 `Chanjet.Connector.Demo` 目录，然后执行以下命令：

**在 macOS / Linux 下:**
```bash
export APP_KEY="your_app_key"
export APP_SECRET="your_app_secret_32_chars"
# export GATEWAY_URL="wss://open.chanjet.com/gateway" # 可选

dotnet run
```

**在 Windows (PowerShell) 下:**
```powershell
$env:APP_KEY="your_app_key"
$env:APP_SECRET="your_app_secret_32_chars"

dotnet run
```

### 方式 2：通过 Visual Studio / Rider 运行

1. 打开根目录下的解决方案文件 `Chanjet.Connector.sln`。
2. 将 `Chanjet.Connector.Demo` 设置为**启动项目 (Startup Project)**。
3. 打开该项目的“属性” (Properties) -> “调试” (Debug) 面板。
4. 在“环境变量” (Environment Variables) 中添加 `APP_KEY` 和 `APP_SECRET`。
5. 按下 `F5` 启动调试。

## 💡 预期输出

当您看到如下类似的日志时，代表客户端已经成功连上畅捷通网关并开始待命接收消息了：

```text
Starting .NET SDK Demo...
[GatewayClient] Connecting to wss://stream-open.chanapp.chanjet.com ...
[GatewayClient] Connected successfully!
```

按下 `Ctrl + C` 即可优雅退出程序。
