using AIAgentCryptoTrading.Core.Models;
using System;
using System.Threading.Tasks;

namespace AIAgentCryptoTrading.Core.Interfaces
{
    public interface IBacktester
    {
        Task<BacktestResult> RunBacktestAsync(string symbol, string strategy, DateTime startDate, DateTime endDate);
    }
}