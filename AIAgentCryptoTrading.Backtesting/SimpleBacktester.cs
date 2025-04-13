using AIAgentCryptoTrading.Core.Interfaces;
using AIAgentCryptoTrading.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AIAgentCryptoTrading.Backtesting
{
    public class SimpleBacktester : IBacktester
    {
        private readonly IDataCollector _dataCollector;
        
        public SimpleBacktester(IDataCollector dataCollector)
        {
            _dataCollector = dataCollector;
        }
        
        public async Task<BacktestResult> RunBacktestAsync(string symbol, string strategy, DateTime startDate, DateTime endDate)
        {
            // Calculate how many days we need to fetch
            int days = (int)(endDate - startDate).TotalDays;
            int limit = days + 1; // +1 to include both start and end date
            
            // Fetch historical data
            var candles = await _dataCollector.GetHistoricalDataAsync(symbol, "1d", limit);
            
            // Filter data by date range
            candles = candles
                .Where(c => c.Timestamp >= startDate && c.Timestamp <= endDate)
                .OrderBy(c => c.Timestamp)
                .ToList();
                
            if (!candles.Any())
            {
                throw new Exception("No data available for the specified date range");
            }
            
            // Apply strategy
            var trades = new List<Trade>();
            decimal balance = 1000m; // Starting with $1000
            decimal originalBalance = balance;
            bool inPosition = false;
            decimal entryPrice = 0m;
            
            switch (strategy.ToLower())
            {
                case "sma":
                    trades = ApplySMAStrategy(candles, ref balance, ref inPosition, ref entryPrice);
                    break;
                    
                case "macd":
                    trades = ApplyMACDStrategy(candles, ref balance, ref inPosition, ref entryPrice);
                    break;
                    
                default:
                    throw new ArgumentException($"Strategy '{strategy}' is not implemented");
            }
            
            // Calculate performance metrics
            decimal totalReturn = (balance - originalBalance) / originalBalance * 100;
            int winningTrades = trades.Count(t => t.ProfitLoss > 0);
            decimal winRate = trades.Any() ? (decimal)winningTrades / trades.Count * 100 : 0;
            
            return new BacktestResult
            {
                StrategyName = strategy,
                Symbol = symbol,
                TotalReturn = totalReturn,
                WinRate = winRate,
                TotalTrades = trades.Count
            };
        }
        
        private List<Trade> ApplySMAStrategy(List<CandleData> candles, ref decimal balance, ref bool inPosition, ref decimal entryPrice)
        {
            // Simple Moving Average crossover strategy (SMA 10 crosses above SMA 30)
            var trades = new List<Trade>();
            
            // Calculate SMAs
            List<decimal> prices = candles.Select(c => c.Close).ToList();
            List<decimal> sma10 = CalculateSMA(prices, 10);
            List<decimal> sma30 = CalculateSMA(prices, 30);
            
            // Need at least 30 candles to have both SMAs
            for (int i = 30; i < candles.Count; i++)
            {
                // Buy signal: SMA10 crosses above SMA30
                bool buyCrossover = sma10[i-1] <= sma30[i-1] && sma10[i] > sma30[i];
                
                // Sell signal: SMA10 crosses below SMA30
                bool sellCrossover = sma10[i-1] >= sma30[i-1] && sma10[i] < sma30[i];
                
                if (!inPosition && buyCrossover)
                {
                    // Buy
                    entryPrice = candles[i].Close;
                    decimal shares = balance / entryPrice;
                    balance = 0; // All in!
                    inPosition = true;
                    
                    trades.Add(new Trade
                    {
                        EntryDate = candles[i].Timestamp,
                        EntryPrice = entryPrice,
                        Direction = "Long"
                    });
                }
                else if (inPosition && sellCrossover)
                {
                    // Sell
                    decimal exitPrice = candles[i].Close;
                    decimal shares = balance / entryPrice;
                    balance = shares * exitPrice;
                    inPosition = false;
                    
                    // Update the last trade
                    var lastTrade = trades.Last();
                    lastTrade.ExitDate = candles[i].Timestamp;
                    lastTrade.ExitPrice = exitPrice;
                    lastTrade.ProfitLoss = (exitPrice - entryPrice) / entryPrice * 100;
                }
            }
            
            // If still in position at the end, close it
            if (inPosition)
            {
                decimal exitPrice = candles.Last().Close;
                decimal shares = balance / entryPrice;
                balance = shares * exitPrice;
                
                // Update the last trade
                var lastTrade = trades.Last();
                lastTrade.ExitDate = candles.Last().Timestamp;
                lastTrade.ExitPrice = exitPrice;
                lastTrade.ProfitLoss = (exitPrice - entryPrice) / entryPrice * 100;
            }
            
            return trades;
        }
        
        private List<Trade> ApplyMACDStrategy(List<CandleData> candles, ref decimal balance, ref bool inPosition, ref decimal entryPrice)
        {
            // MACD strategy implementation would go here
            // For now, just return an empty list
            return new List<Trade>();
        }
        
        private List<decimal> CalculateSMA(List<decimal> prices, int period)
        {
            var result = new List<decimal>();
            
            // Not enough data
            if (prices.Count < period)
            {
                return result;
            }
            
            // First values are null until we have enough periods
            for (int i = 0; i < period - 1; i++)
            {
                result.Add(0);
            }
            
            // Calculate SMA for each point
            for (int i = period - 1; i < prices.Count; i++)
            {
                decimal sum = 0;
                for (int j = 0; j < period; j++)
                {
                    sum += prices[i - j];
                }
                result.Add(sum / period);
            }
            
            return result;
        }
    }
    
}