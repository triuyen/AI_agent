
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AIAgentCryptoTrading.Core.Models;
using AIAgentCryptoTrading.DataCollector.Services;
using AIAgentCryptoTrading.StrategyEngine.Strategies;

namespace AIAgentCryptoTrading.StrategyEngine.Services
{
    public class MeanReversionStrategyService
    {
        private readonly CoinGeckoDataProvider _dataProvider;
        
        public MeanReversionStrategyService()
        {
            _dataProvider = new CoinGeckoDataProvider();
        }
        
        public async Task<Dictionary<DateTime, PriceData>> ExecuteStrategyAsync(
            string coinId, 
            string currency, 
            int days,
            int rsiPeriod = 14,
            double rsiOversold = 30,
            double rsiOverbought = 70,
            int bbPeriod = 20,
            double bbStd = 2,
            bool exitMiddle = true)
        {
            // Get price data
            var priceData = await _dataProvider.GetOHLCDataAsync(coinId, currency, days.ToString());
            
            // Fall back to market data if OHLC unavailable
            if (priceData.Count == 0)
            {
                priceData = await _dataProvider.GetHistoricalMarketDataAsync(coinId, currency, days);
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