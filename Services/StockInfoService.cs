using StockQuoteRealTime.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace StockQuoteRealTime.Services
{
    public class StockInfoService : IStockInfoService
    {
        const string UrlTemplate = "https://api.nasdaq.com/api/quote/{0}/info?assetclass=stocks";

        static readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        readonly IHttpClientFactory _httpClientFactory;

        public StockInfoService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<StockInfo> GetStockInfo(string symbol)
        {
            var url = string.Format(UrlTemplate, symbol?.Trim());
            var client = _httpClientFactory.CreateClient("quotes");
            var jsonString = await client.GetStringAsync(url);

            return JsonSerializer.Deserialize<StockInfo>(jsonString, options);
        }
    }
}
