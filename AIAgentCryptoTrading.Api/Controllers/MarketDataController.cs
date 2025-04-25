using AIAgentCryptoTrading.Core.Interfaces;
using AIAgentCryptoTrading.Core.Models;
// using AIAgentCryptoTrading.DataCollector.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using AIAgentCryptoTrading.DataCollector;

namespace AIAgentCryptoTrading.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MarketDataController : ControllerBase
    {
        private readonly IDataCollector _dataCollector;
        private readonly CoinGeckoDataCollector _geckoCollector;
        
        public MarketDataController(IDataCollector dataCollector, CoinGeckoDataCollector geckoCollector)
        {
            _dataCollector = dataCollector;
            _geckoCollector = geckoCollector;
        }
        
        [HttpGet("{symbol}")]
        public async Task<ActionResult<List<CandleData>>> GetMarketData(
            string symbol, 
            [FromQuery] string timeframe = "1d", 
            [FromQuery] int limit = 100,
            [FromQuery] string source = "binance")
        {
            // try
            // {
            //     if (source.ToLower() == "coingecko")
            //     {
            //         // Convert symbol to CoinGecko format
            //         string coinId = TranslateSymbolToCoinId(symbol);
            //         int days = CalculateDays(timeframe, limit);
                    
            //         // Use the provider to get data
            //         var priceData = await _geckoProvider.GetHistoricalMarketDataAsync(coinId, "usd", days);
                    
            //         // Convert to list of CandleData
            //         var candles = priceData
            //             .Select(kvp => new CandleData
            //             {
            //                 Timestamp = kvp.Key,
            //                 Open = (decimal)kvp.Value.Open,
            //                 High = (decimal)kvp.Value.High,
            //                 Low = (decimal)kvp.Value.Low,
            //                 Close = (decimal)kvp.Value.Close,
            //                 Volume = (decimal)kvp.Value.Volume
            //             })
            //             .OrderBy(c => c.Timestamp)
            //             .Take(limit)
            //             .ToList();
                    
            //         return Ok(candles);
            //     }
            //     else
            //     {
            //         // Use the standard data collector for Binance
            //         var data = await _dataCollector.GetHistoricalDataAsync(symbol, timeframe, limit);
            //         return Ok(data);
            //     }
            // }
            // catch (Exception ex)
            // {
            //     return BadRequest($"Error getting market data: {ex.Message}");
            // }
            try
            {
                List<CandleData> data;
                
                // Select data source
                if (source.ToLower() == "coingecko")
                {
                    data = await _geckoCollector.GetHistoricalDataAsync(symbol, timeframe, limit);
                }
                else
                {
                    data = await _dataCollector.GetHistoricalDataAsync(symbol, timeframe, limit);
                }
                
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error getting market data: {ex.Message}");
            }
            }
        
        [HttpGet("top")]
        public async Task<ActionResult<List<string>>> GetTopCryptos([FromQuery] int limit = 10)
        {
            try
            {
                var topCryptos = await _geckoCollector.GetTopCryptos(limit);
                return Ok(topCryptos);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error getting top cryptos: {ex.Message}");
            }
        }
        
        // Helper methods for CoinGecko data
        private string TranslateSymbolToCoinId(string symbol)
        {
            // Simple mapping for common symbols
            Dictionary<string, string> symbolToId = new Dictionary<string, string>
            {
                { "BTCUSDT", "bitcoin" },
                { "ETHUSDT", "ethereum" },
                { "BNBUSDT", "binancecoin" },
                { "SOLUSDT", "solana" },
                { "ADAUSDT", "cardano" },
                { "XRPUSDT", "ripple" },
                { "DOGEUSDT", "dogecoin" },
                { "DOTUSDT", "polkadot" },
                { "AVAXUSDT", "avalanche-2" },
                { "MATICUSDT", "matic-network" }
            };
            
            if (symbolToId.ContainsKey(symbol))
                return symbolToId[symbol];
                
            // Default fallback
            return symbol.Replace("USDT", "").ToLower();
        }
        
        private int CalculateDays(string timeframe, int limit)
        {
            // Convert timeframe to days
            switch (timeframe)
            {
                case "1m": return (int)Math.Ceiling(limit / 1440.0); // 1 minute
                case "5m": return (int)Math.Ceiling(limit / 288.0);  // 5 minutes
                case "15m": return (int)Math.Ceiling(limit / 96.0);  // 15 minutes
                case "30m": return (int)Math.Ceiling(limit / 48.0);  // 30 minutes
                case "1h": return (int)Math.Ceiling(limit / 24.0);   // 1 hour
                case "2h": return (int)Math.Ceiling(limit / 12.0);   // 2 hours
                case "4h": return (int)Math.Ceiling(limit / 6.0);    // 4 hours
                case "6h": return (int)Math.Ceiling(limit / 4.0);    // 6 hours
                case "8h": return (int)Math.Ceiling(limit / 3.0);    // 8 hours
                case "12h": return (int)Math.Ceiling(limit / 2.0);   // 12 hours
                case "1d": return limit;                             // 1 day
                case "3d": return limit * 3;                         // 3 days
                case "1w": return limit * 7;                         // 1 week
                case "1M": return limit * 30;                        // 1 month
                default: return Math.Max(limit, 90);                 // Default to 90 days or limit
            }
        }
    }
}