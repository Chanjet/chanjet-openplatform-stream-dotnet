# 畅捷通 Stream Gateway .NET SDK - 快速入门演示 (Demo)

本目录包含一个可直接运行的跨平台控制台应用程序，用于演示如何使用 `Chanjet.Connector.Sdk` 与畅捷通开放平台的流式网关建立 WebSocket 连接并接收业务消息。

## 🛠️ 环境准备

1. **.NET SDK**: 请确保您的系统中安装了 [**.NET 6.0 / 8.0 SDK**](https://dotnet.microsoft.com/download) 或更高版本。
   - 验证安装：在终端执行 `dotnet --version`，如果输出版本号即代表环境就绪。

## ⚙️ 凭证配置

Demo 程序引入了 `DotNetEnv`，支持直接从项目目录中的 `.env` 文件加载环境变量。

1. 在当前 `Chanjet.Connector.Demo` 目录下，复制 `.env.example` 并重命名为 `.env`。
2. 编辑 `.env` 文件，填入您的真实凭证：

```env
APP_KEY=your_app_key
APP_SECRET=your_app_secret_32_chars
# GATEWAY_URL=wss://open.chanjet.com/gateway # 可选
```

## 🚀 启动方式

### 方式 1：通过命令行运行 (推荐)

打开终端，进入本 `Chanjet.Connector.Demo` 目录，确保 `.env` 文件已配置好，然后执行：

```bash
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
