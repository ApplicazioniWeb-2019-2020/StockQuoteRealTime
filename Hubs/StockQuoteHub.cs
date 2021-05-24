using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using StockQuoteRealTime.Models;
using StockQuoteRealTime.Services;
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
        readonly object sync = new object();
        readonly Dictionary<string, Timer> timers = new Dictionary<string, Timer>();

        readonly IServiceScopeFactory _scopeFactory;
        readonly IStockInfoService _stockInfoService;

        public StockQuoteHub(IServiceScopeFactory scopeFactor, IStockInfoService stockInfoService)
        {
            _scopeFactory = scopeFactor;
            _stockInfoService = stockInfoService;
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
                        var stockInfo = await _stockInfoService.GetStockInfo(symbol);

                        var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<StockQuoteHub>>();
                        await hubContext.Clients.Group(symbol).SendAsync("ReceiveQuote", stockInfo);
                    }
                };

                timer.Start();

                timers.Add(symbol, timer);
            }
        }
    }
}
