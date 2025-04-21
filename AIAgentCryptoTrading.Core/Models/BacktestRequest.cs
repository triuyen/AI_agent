using System;

namespace AIAgentCryptoTrading.Api.Models
{
    public class BacktestRequest
    {
        public string Symbol { get; set; }
        public string Strategy { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}