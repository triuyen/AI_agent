using System;

namespace AIAgentCryptoTrading.Core.Models
{
    public class Trade
    {
        public DateTime EntryDate { get; set; }
        public DateTime? ExitDate { get; set; }
        public decimal EntryPrice { get; set; }
        public decimal? ExitPrice { get; set; }
        public string Direction { get; set; } // "Long" or "Short"
        public decimal? ProfitLoss { get; set; }
    }
}