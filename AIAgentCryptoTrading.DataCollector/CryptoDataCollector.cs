using AIAgentCryptoTrading.Core.Interfaces;
using AIAgentCryptoTrading.Core.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AIAgentCryptoTrading.DataCollector
{
    public class CryptoDataCollector : IDataCollector
    {
        private readonly HttpClient _httpClient;
        
        public CryptoDataCollector()
        {
            _httpClient = new HttpClient();
        }
        
        public async Task<List<CandleData>> GetHistoricalDataAsync(string symbol, string timeframe, int limit)
        {
            // Fetch data from exchange (using Binance API as an example)
            string binanceEndpoint = $"https://api.binance.com/api/v3/klines?symbol={symbol.Replace("/", "")}&interval={timeframe}&limit={limit}";
            
            try
            {
                var response = await _httpClient.GetStringAsync(binanceEndpoint);
                var jsonArray = JsonConvert.DeserializeObject<JArray>(response);
                
                var candles = new List<CandleData>();
                
                foreach (var item in jsonArray)
                {
                    var candle = new CandleData
                    {
                        Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(item[0].Value<long>()).DateTime,
                        Open = decimal.Parse(item[1].Value<string>()),
                        High = decimal.Parse(item[2].Value<string>()),
                        Low = decimal.Parse(item[3].Value<string>()),
                        Close = decimal.Parse(item[4].Value<string>()),
                        Volume = decimal.Parse(item[5].Value<string>())
                    };
                    
                    candles.Add(candle);
                }
                
                return candles;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching data: {ex.Message}");
                return new List<CandleData>();
            }
        }
    }
}