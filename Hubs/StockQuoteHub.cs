using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using StockQuoteRealTime.Models;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;

namespace StockQuoteRealTime.Hubs
{
    public class StockQuoteHub : Hub
    {
        static readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        readonly object sync = new object();
        readonly Dictionary<string, Timer> timers = new Dictionary<string, Timer>();

        readonly IServiceScopeFactory _scopeFactory;

        public StockQuoteHub(IServiceScopeFactory scopeFactor)
        {
            _scopeFactory = scopeFactor;
        }

        public async Task SubscribeSymbol(string symbol)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, symbol);
            SendStockQuote(_scopeFactory, symbol);
        }

        void SendStockQuote(IServiceScopeFactory scopeFactor, string symbol)
        {
            if (timers.TryGetValue(symbol, out _)) return;

            lock (sync)
            {
                var scopeFactory = _scopeFactory;

                // Aggiunge un timer server-side che ogni secondo rileva la quotazione del simbolo indicato.
                var timer = new Timer();
                timer.Interval = 1000;
                timer.Elapsed += async (sender, e) =>
                {
                    // https://github.com/dotnet/AspNetCore.Docs/issues/8537
                    using (var scope = scopeFactory.CreateScope())
                    {
                        var stockInfo = await GetStockInfo(symbol);

                        var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<StockQuoteHub>>();
                        await hubContext.Clients.Group(symbol).SendAsync("ReceiveQuote", stockInfo);
                    }
                };

                timer.Start();

                timers.Add(symbol, timer);
            }
        }

        async Task<StockInfo> GetStockInfo(string symbol)
        {
            var url = $"https://api.nasdaq.com/api/quote/{symbol}/info?assetclass=stocks";

            using var client = new WebClient();
            client.Headers[HttpRequestHeader.Accept] = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";
            var jsonString = await client.DownloadStringTaskAsync(url);

            return JsonSerializer.Deserialize<StockInfo>(jsonString, options);
        }
    }
}
