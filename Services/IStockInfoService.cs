using StockQuoteRealTime.Models;
using System.Threading.Tasks;

namespace StockQuoteRealTime.Services
{
    public interface IStockInfoService
    {
        Task<StockInfo> GetStockInfo(string symbol);
    }
}