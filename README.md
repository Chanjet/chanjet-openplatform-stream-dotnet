# 畅捷通 Stream Gateway .NET SDK

[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-6.0%20%7C%208.0%20%7C%20Standard%202.0-blue)](https://dotnet.microsoft.com/)

> ⚠️ **免责声明 (Disclaimer)**：当前 .NET SDK 核心代码由 AI 生成，由于执行环境限制，**未经本地编译与运行测试**。代码结构与逻辑仅供参考，在用于生产环境之前，请务必进行自行编译与严格验证。

本 SDK 是专为畅捷通开放平台 ISV 打造的轻量级通讯框架。屏蔽了底层 WebSocket 的连接管理细节，内置强大的消息自动解密、POJO 自动分发机制。

## 🚀 核心特性

- **自动驾驶级连接管理**：内置心跳保活、断线指数退避重连。
- **智能消息分发器 (MessageDispatcher)**：支持根据 `msgType` 自动路由到不同的业务回调委托中。
- **透明安全保障**：自动执行 AES-128-ECB 解密，开发者仅需处理明文实体。
- **语义化监听**：提供 `OnAppTicket`、`OnAppNotice` 等强类型回调方法，开箱即用。

## 📦 快速开始

### 1. 引入依赖
SDK 支持 `netstandard2.0`, `net6.0`, `net8.0`。
通过将 `Chanjet.Connector.Sdk` 项目引用到您的解决方案中即可。

### 2. 编写业务处理器

```csharp
using Chanjet.Connector.Sdk;

// 1. 创建分发器
var dispatcher = new MessageDispatcher();

// 2. 订阅系统消息
dispatcher.OnAppTicket(msg => {
    Console.WriteLine($"收到最新票据: {msg.BizContent?.AppTicket}");
    return true; // 返回 true 自动回复成功 ACK
});

// 3. 订阅好系列业务消息 (如：销货单)
dispatcher.OnAppNotice("GoodsIssue", (msg, content) => {
    Console.WriteLine($"收到销货单事件: {content.GetRawText()}");
    return true;
});

// 4. 初始化并启动客户端
var client = new GatewayClient.Builder()
    .AppKey("your_app_key")
    .AppSecret("your_app_secret_32_chars")
    // .GatewayUrl("https://stream-open.chanapp.chanjet.com") // 可选，默认指向生产环境
    .Build();

client.UseDispatcher(dispatcher);

// 启动客户端并等待 (支持 CancellationToken)
await client.StartAsync();
```

## 📋 内置模型参考

SDK 现已内置以下标准模型：

| 消息类型 (msgType) | 语义化监听方法 | 模型类 |
| :--- | :--- | :--- |
| `APP_TICKET` | `OnAppTicket` | `AppTicketMessage` |
| `TEMP_AUTH_CODE` | `OnEntAuthCode` | `EntAuthCodeMessage` |
| `PAY_ORDER_SUCCESS`| `OnOrderStatus` | `OrderStatusMessage` |
| `APP_CANCEL_...` | `OnEntUnauth`等 | `EntUnauthMessage` |
| `APP_NOTICE` | `OnAppNotice` | `AppNoticeMessage` |

## 📖 示例项目
更多详尽的使用场景，请参考源码树中的：
👉 [**Chanjet.Connector.Demo**](Chanjet.Connector.Demo/Program.cs)

## ⚖️ 许可
MIT License
