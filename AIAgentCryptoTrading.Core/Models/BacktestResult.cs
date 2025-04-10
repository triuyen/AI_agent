namespace AIAgentCryptoTrading.Core.Models
{
    public class BacktestResult
    {
        public string StrategyName { get; set; }
        public string Symbol { get; set; }
        public decimal TotalReturn { get; set; }
        public decimal WinRate { get; set; }
        public int TotalTrades { get; set; }
    }
}