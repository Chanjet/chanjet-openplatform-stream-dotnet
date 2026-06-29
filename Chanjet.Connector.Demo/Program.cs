using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Chanjet.Connector.Sdk;
using Chanjet.Connector.Sdk.Protocol;

namespace Chanjet.Connector.Demo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting .NET SDK Demo...");

            // Load .env file
            DotNetEnv.Env.TraversePath().Load();

            // In a real application, you would read these from a config file (e.g. appsettings.json) or environment variables.
            string appKey = Environment.GetEnvironmentVariable("APP_KEY") ?? "your_app_key";
            string appSecret = Environment.GetEnvironmentVariable("APP_SECRET") ?? "your_app_secret";
            string gatewayUrl = Environment.GetEnvironmentVariable("GATEWAY_URL") ?? "";

            var dispatcher = new MessageDispatcher();

            // 1. App Ticket
            dispatcher.OnAppTicket(msg =>
            {
                Console.WriteLine($"[System] Received APP_TICKET: {msg.BizContent?.AppTicket}");
                return true;
            });

            // 2. Temp Auth Code
            dispatcher.OnEntAuthCode(msg =>
            {
                Console.WriteLine($"[System] Received TEMP_AUTH_CODE: {msg.BizContent?.TempAuthCode} for State: {msg.BizContent?.State}");
                return true;
            });

            // 3. App Notice (Specific BoName and TransType)
            dispatcher.OnAppNotice("Goods", "01", (msg, content) =>
            {
                Console.WriteLine($"[Business] Goods created or updated: {content.GetRawText()}");
                return true;
            });

            // 4. App Notice (Any transaction for a BoName)
            dispatcher.OnAppNotice("GoodsIssue", (msg, content) =>
            {
                Console.WriteLine($"[Business] GoodsIssue event: {content.GetRawText()}");
                return true;
            });

            // 5. Order Status
            dispatcher.OnOrderStatus(msg =>
            {
                Console.WriteLine($"[Business] Order paid successfully: {msg.BizContent?.OrderNo}");
                return true;
            });

            // 6. Generic/Custom handler
            dispatcher.Register<BaseMessage>("CUSTOM_EVENT", msg =>
            {
                Console.WriteLine($"[Custom] Received event: {msg.MsgType}");
                return true;
            });

            var client = new GatewayClient.Builder()
                .AppKey(appKey)
                .AppSecret(appSecret)
                .GatewayUrl(gatewayUrl)
                .Build();

            client.UseDispatcher(dispatcher);

            var cts = new CancellationTokenSource();
            
            Console.CancelKeyPress += (s, e) =>
            {
                Console.WriteLine("Shutting down...");
                cts.Cancel();
                e.Cancel = true;
            };

            await client.StartAsync(cts.Token);
            Console.WriteLine("Exiting demo.");
        }
    }
}
