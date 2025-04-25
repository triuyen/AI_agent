using AIAgentCryptoTrading.Core.Interfaces;
using AIAgentCryptoTrading.Core.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace AIAgentCryptoTrading.DataCollector
{
    public class CoinGeckoDataCollector : IDataCollector
    {
        private readonly HttpClient _httpClient;
        
        public CoinGeckoDataCollector()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://api.coingecko.com/api/v3/")
            };
        }
        
        // This method implements the interface
        public async Task<List<CandleData>> GetHistoricalDataAsync(string symbol, string timeframe, int limit)
        {
            // Convert symbol to CoinGecko ID (e.g., BTCUSDT -> bitcoin)
            string coinId = TranslateSymbolToCoinId(symbol);
            
            // Calculate days based on limit and timeframe
            int days = CalculateDays(timeframe, limit);
            
            // Use your teammate's comprehensive method
            var cryptoData = await GetCryptoDataDailyWithDerivedOHLC(coinId, "usd", days);
            
            // Convert to your CandleData format
            var candleList = new List<CandleData>();
            
            foreach (var data in cryptoData)
            {
                // Only add entries that have full OHLC data
                if (data.ContainsKey("open") && data.ContainsKey("high") && 
                    data.ContainsKey("low") && data.ContainsKey("close"))
                {
                    var candle = new CandleData
                    {
                        Timestamp = (DateTime)data["timestamp"],
                        Open = Convert.ToDecimal(data["open"]),
                        High = Convert.ToDecimal(data["high"]),
                        Low = Convert.ToDecimal(data["low"]),
                        Close = Convert.ToDecimal(data["close"]),
                        Volume = data.ContainsKey("volume") ? Convert.ToDecimal(data["volume"]) : 0
                    };
                    
                    candleList.Add(candle);
                }
            }
            
            // Sort by timestamp (oldest first), then take only the requested limit
            return candleList.OrderBy(c => c.Timestamp).Take(limit).ToList();
        }
        
        // Get top cryptocurrencies by market cap - from teammate's code
        public async Task<List<string>> GetTopCryptos(int limit = 10)
        {
            try
            {
                var response = await _httpClient.GetAsync($"coins/markets?vs_currency=usd&order=market_cap_desc&per_page={limit}&page=1");
                response.EnsureSuccessStatusCode();
                
                var coinsJson = await response.Content.ReadAsStringAsync();
                var coinsList = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(coinsJson);
                
                // Convert to trading symbol format (e.g., bitcoin -> BTCUSDT)
                var symbols = new List<string>();
                foreach (var coin in coinsList)
                {
                    var symbol = coin["symbol"].ToString().ToUpper() + "USDT";
                    symbols.Add(symbol);
                }
                
                return symbols;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching top cryptos: {ex.Message}");
                // Return some defaults if API fails
                return new List<string> { "BTCUSDT", "ETHUSDT", "BNBUSDT", "SOLUSDT", "ADAUSDT" };
            }
        }
        
        // Helper method to translate from trading symbol to CoinGecko ID
        private string TranslateSymbolToCoinId(string symbol)
        {
            // Remove USDT suffix and convert to lowercase
            var baseSymbol = symbol.Replace("USDT", "").ToLower();
            
            // Map common symbols that don't directly match CoinGecko IDs
            Dictionary<string, string> symbolMap = new Dictionary<string, string>
            {
                { "btc", "bitcoin" },
                { "eth", "ethereum" },
                { "bnb", "binancecoin" },
                { "sol", "solana" },
                { "ada", "cardano" },
                { "xrp", "ripple" },
                { "doge", "dogecoin" },
                { "dot", "polkadot" },
                { "avax", "avalanche-2" },
                { "matic", "matic-network" }
            };
            
            if (symbolMap.ContainsKey(baseSymbol))
                return symbolMap[baseSymbol];
            
            return baseSymbol;
        }
        
        // Helper method to calculate days based on timeframe and limit
        private int CalculateDays(string timeframe, int limit)
        {
            // Convert timeframe to days
            switch (timeframe)
            {
                case "1m": return Math.Max(1, (int)Math.Ceiling(limit / 1440.0));
                case "5m": return Math.Max(1, (int)Math.Ceiling(limit / 288.0));
                case "15m": return Math.Max(1, (int)Math.Ceiling(limit / 96.0));
                case "1h": return Math.Max(1, (int)Math.Ceiling(limit / 24.0));
                case "4h": return Math.Max(1, (int)Math.Ceiling(limit / 6.0));
                case "1d": return limit;
                default: return Math.Max(limit, 90);
            }
        }
        
        // This is your teammate's core method, slightly modified
        private async Task<List<Dictionary<string, object>>> GetCryptoDataDailyWithDerivedOHLC(string cryptoId, string vsCurrency = "usd", int days = 365)
        {
            try
            {
                // Get market chart data which includes daily prices
                var response = await _httpClient.GetAsync($"coins/{cryptoId}/market_chart?vs_currency={vsCurrency}&days={days}&interval=daily");
                response.EnsureSuccessStatusCode();
                
                var marketChartJson = await response.Content.ReadAsStringAsync();
                var marketChart = JsonConvert.DeserializeObject<Dictionary<string, List<List<object>>>>(marketChartJson);

                // Extract data
                var pricesData = marketChart["prices"];
                var volumes = marketChart["total_volumes"];
                var marketCaps = marketChart["market_caps"];

                if (pricesData.Count == 0)
                {
                    return new List<Dictionary<string, object>>();
                }

                // Process price data
                var timeSeriesList = new List<Dictionary<string, object>>();
                foreach (var pricePoint in pricesData)
                {
                    var timestamp = DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(pricePoint[0])).DateTime;
                    var price = Convert.ToDouble(pricePoint[1], CultureInfo.InvariantCulture);
                    
                    timeSeriesList.Add(new Dictionary<string, object>
                    {
                        { "timestamp", timestamp },
                        { "price", price },
                        { "crypto_id", cryptoId }
                    });
                }

                // Merge volume data
                for (int i = 0; i < Math.Min(volumes.Count, timeSeriesList.Count); i++)
                {
                    var volumeTimestamp = DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(volumes[i][0])).DateTime;
                    var volume = Convert.ToDouble(volumes[i][1], CultureInfo.InvariantCulture);
                    
                    var entry = timeSeriesList.FirstOrDefault(e => ((DateTime)e["timestamp"]).Date == volumeTimestamp.Date);
                    if (entry != null)
                    {
                        entry["volume"] = volume;
                    }
                }

                // Try to get OHLC data from the API first
                try
                {
                    var ohlcResponse = await _httpClient.GetAsync($"coins/{cryptoId}/ohlc?vs_currency={vsCurrency}&days={Math.Min(days, 90)}");
                    if (ohlcResponse.IsSuccessStatusCode)
                    {
                        var ohlcJson = await ohlcResponse.Content.ReadAsStringAsync();
                        var ohlcData = JsonConvert.DeserializeObject<List<List<object>>>(ohlcJson);

                        if (ohlcData != null && ohlcData.Count > 0)
                        {
                            // Analyze time differences to check if data is daily
                            var ohlcTimestamps = ohlcData.Select(item => 
                                DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(item[0])).DateTime).ToList();
                            
                            bool isDaily = IsDataDaily(ohlcTimestamps);
                            
                            if (isDaily)
                            {
                                // Merge OHLC data with time series
                                foreach (var ohlcPoint in ohlcData)
                                {
                                    var ohlcTimestamp = DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(ohlcPoint[0])).DateTime;
                                    var open = Convert.ToDouble(ohlcPoint[1], CultureInfo.InvariantCulture);
                                    var high = Convert.ToDouble(ohlcPoint[2], CultureInfo.InvariantCulture);
                                    var low = Convert.ToDouble(ohlcPoint[3], CultureInfo.InvariantCulture);
                                    var close = Convert.ToDouble(ohlcPoint[4], CultureInfo.InvariantCulture);
                                    
                                    var entry = timeSeriesList.FirstOrDefault(e => ((DateTime)e["timestamp"]).Date == ohlcTimestamp.Date);
                                    if (entry != null)
                                    {
                                        entry["open"] = open;
                                        entry["high"] = high;
                                        entry["low"] = low;
                                        entry["close"] = close;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error getting OHLC: {ex.Message}. Will derive from price data.");
                }

                // If we don't have OHLC data yet, try to derive from hourly data
                if (!timeSeriesList.Any(ts => ts.ContainsKey("open")))
                {
                    try
                    {
                        // Get hourly price data for more accurate OHLC derivation
                        var hourlyResponse = await _httpClient.GetAsync($"coins/{cryptoId}/market_chart?vs_currency={vsCurrency}&days={Math.Min(days, 90)}&interval=hourly");
                        hourlyResponse.EnsureSuccessStatusCode();
                        
                        var hourlyChartJson = await hourlyResponse.Content.ReadAsStringAsync();
                        var hourlyChart = JsonConvert.DeserializeObject<Dictionary<string, List<List<object>>>>(hourlyChartJson);

                        var hourlyPrices = hourlyChart["prices"];
                        if (hourlyPrices != null && hourlyPrices.Count > 0)
                        {
                            // Process hourly data
                            var hourlyData = new List<Dictionary<string, object>>();
                            foreach (var item in hourlyPrices)
                            {
                                var timestamp = DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(item[0])).DateTime;
                                var price = Convert.ToDouble(item[1], CultureInfo.InvariantCulture);
                                
                                hourlyData.Add(new Dictionary<string, object>
                                {
                                    { "timestamp", timestamp },
                                    { "price", price },
                                    { "date", timestamp.Date }
                                });
                            }
                            
                            // Group by day and calculate OHLC
                            var dailyOhlc = hourlyData
                                .GroupBy(h => ((DateTime)h["date"]).Date)
                                .Select(g => new Dictionary<string, object>
                                {
                                    { "date", g.Key },
                                    { "open", g.First()["price"] },
                                    { "high", g.Max(h => (double)h["price"]) },
                                    { "low", g.Min(h => (double)h["price"]) },
                                    { "close", g.Last()["price"] }
                                })
                                .ToList();
                                
                            // Merge with time series
                            foreach (var ts in timeSeriesList)
                            {
                                var tsDate = ((DateTime)ts["timestamp"]).Date;
                                var ohlc = dailyOhlc.FirstOrDefault(d => (DateTime)d["date"] == tsDate);
                                
                                if (ohlc != null)
                                {
                                    ts["open"] = ohlc["open"];
                                    ts["high"] = ohlc["high"];
                                    ts["low"] = ohlc["low"];
                                    ts["close"] = ohlc["close"];
                                }
                                else
                                {
                                    // For days without hourly data, use daily price
                                    var price = (double)ts["price"];
                                    ts["open"] = price;
                                    ts["high"] = price;
                                    ts["low"] = price;
                                    ts["close"] = price;
                                }
                            }
                        }
                        else
                        {
                            // Fall back to daily price if no hourly data
                            SetOhlcFromDailyPrice(timeSeriesList);
                        }
                    }
                    catch (Exception)
                    {
                        // Fall back to daily price if hourly data fetch fails
                        SetOhlcFromDailyPrice(timeSeriesList);
                    }
                }

                // If we still don't have OHLC data, use daily price
                if (!timeSeriesList.Any(ts => ts.ContainsKey("open")))
                {
                    SetOhlcFromDailyPrice(timeSeriesList);
                }

                // Verify and clean data
                VerifyOhlcData(timeSeriesList);
                
                return timeSeriesList;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetCryptoDataDailyWithDerivedOHLC: {ex.Message}");
                return new List<Dictionary<string, object>>();
            }
        }
        
        // Helper method to set OHLC data from daily price
        private void SetOhlcFromDailyPrice(List<Dictionary<string, object>> timeSeriesList)
        {
            foreach (var ts in timeSeriesList)
            {
                if (!ts.ContainsKey("open"))
                {
                    var price = (double)ts["price"];
                    ts["open"] = price;
                    ts["high"] = price;
                    ts["low"] = price;
                    ts["close"] = price;
                }
            }
        }
        
        // Helper to check if data is daily
        private bool IsDataDaily(List<DateTime> timestamps)
        {
            if (timestamps.Count < 2)
                return false;
                
            var timeDiffs = new List<TimeSpan>();
            for (int i = 1; i < timestamps.Count; i++)
            {
                timeDiffs.Add(timestamps[i] - timestamps[i-1]);
            }
            
            var avgHours = timeDiffs.Average(td => td.TotalHours);
            return 20 <= avgHours && avgHours <= 28; // Approximately daily
        }
        
        // Helper to verify OHLC data consistency
        private void VerifyOhlcData(List<Dictionary<string, object>> data)
        {
            // Fix any inconsistencies in OHLC data
            foreach (var item in data)
            {
                if (item.ContainsKey("high") && item.ContainsKey("low") && 
                    item.ContainsKey("open") && item.ContainsKey("close"))
                {
                    double high = (double)item["high"];
                    double low = (double)item["low"];
                    double open = (double)item["open"];
                    double close = (double)item["close"];
                    
                    // Ensure high is the highest value
                    high = Math.Max(high, Math.Max(open, close));
                    
                    // Ensure low is the lowest value
                    low = Math.Min(low, Math.Min(open, close));
                    
                    item["high"] = high;
                    item["low"] = low;
                }
            }
        }
    }
}