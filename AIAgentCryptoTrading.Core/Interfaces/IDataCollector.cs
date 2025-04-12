using AIAgentCryptoTrading.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIAgentCryptoTrading.Core.Interfaces
{
    public interface IDataCollector
    {
        Task<List<CandleData>> GetHistoricalDataAsync(string symbol, string timeframe, int limit);
    }
}