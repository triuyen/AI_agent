using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using AIAgentCryptoTrading.Core.Models;
using AIAgentCryptoTrading.DataCollector;
using AIAgentCryptoTrading.StrategyEngine.Strategies;

namespace AIAgentCryptoTrading.StrategyEngine.Services
{
    public class MeanReversionStrategyService
    {
        private readonly CoinGeckoDataCollector _dataProvider;
        
        public MeanReversionStrategyService()
        {
            _dataProvider = new CoinGeckoDataCollector();
        }
        
        public async Task<Dictionary<DateTime, PriceData>> ExecuteStrategyAsync(
            string symbol, 
            string timeframe = "1d",
            int limit = 365,
            int rsiPeriod = 14,
            double rsiOversold = 30,
            double rsiOverbought = 70,
            int bbPeriod = 20,
            double bbStd = 2,
            bool exitMiddle = true)
        {
            // Get price data using the available method
            var candleData = await _dataProvider.GetHistoricalDataAsync(symbol, timeframe, limit);
            
            // Convert CandleData to the format expected by your strategy
            var priceData = new Dictionary<DateTime, PriceData>();
            
            foreach (var candle in candleData)
            {
                priceData[candle.Timestamp] = new PriceData
                {
                    Open = (double)candle.Open,
                    High = (double)candle.High,
                    Low = (double)candle.Low,
                    Close = (double)candle.Close,
                    Volume = (double)candle.Volume
                };
            }
            
            // Check if we have data
            if (priceData.Count == 0)
            {
                return new Dictionary<DateTime, PriceData>();
            }
            
            // Apply strategy
            var result = MeanReversionStrategy.ExecuteStrategy(
                priceData,
                rsiPeriod,
                rsiOversold,
                rsiOverbought,
                bbPeriod,
                bbStd,
                exitMiddle
            );
            
            return result;
        }
    }
}