namespace StockQuoteRealTime.Models
{
    public class StockData
    {
        public string Symbol { get; set; }

        public string CompanyName { get; set; }

        public string Exchange { get; set; }

        public StockPrimaryData PrimaryData { get; set; }
    }
}
