using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AIAgentCryptoTrading.Core.Models;

namespace AIAgentCryptoTrading.DataCollector.Services
{
    public class CoinGeckoDataProvider
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://api.coingecko.com/api/v3";
        
        public CoinGeckoDataProvider()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "AIAgentCryptoTrading/1.0");
        }

        /// <summary>
        /// Gets historical price data from CoinGecko for a specific cryptocurrency
        /// </summary>
        public async Task<Dictionary<DateTime, PriceData>> GetHistoricalMarketDataAsync(string coinId, string vsCurrency, int days)
        {
            try
            {
                string endpoint = $"/coins/{coinId}/market_chart";
                string url = $"{_baseUrl}{endpoint}?vs_currency={vsCurrency}&days={days}&interval=daily";
                
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var jsonDocument = JsonDocument.Parse(content);
                
                var prices = jsonDocument.RootElement.GetProperty("prices").EnumerateArray();
                var volumes = jsonDocument.RootElement.GetProperty("total_volumes").EnumerateArray();
                
                var priceDataDict = new Dictionary<DateTime, PriceData>();
                var pricesList = new List<(DateTime timestamp, double price, double volume)>();
                
                foreach (var priceItem in prices)
                {
                    long timestamp = (long)priceItem[0].GetDecimal();
                    var dateTime = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime;
                    double price = (double)priceItem[1].GetDecimal();
                    
                    pricesList.Add((dateTime, price, 0));
                }
                
                int volumeIndex = 0;
                foreach (var volumeItem in volumes)
                {
                    if (volumeIndex >= pricesList.Count) break;
                    
                    long timestamp = (long)volumeItem[0].GetDecimal();
                    var dateTime = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime;
                    double volume = (double)volumeItem[1].GetDecimal();
                    
                    for (int i = 0; i < pricesList.Count; i++)
                    {
                        if (pricesList[i].timestamp.Date == dateTime.Date)
                        {
                            var item = pricesList[i];
                            pricesList[i] = (item.timestamp, item.price, volume);
                            break;
                        }
                    }
                    
                    volumeIndex++;
                }
                
                foreach (var (timestamp, price, volume) in pricesList)
                {
                    priceDataDict[timestamp] = new PriceData(price, price, price, price, volume);
                }
                
                return priceDataDict;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Error fetching data from CoinGecko: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                throw new Exception($"Error parsing JSON response: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Gets OHLC price data for a cryptocurrency
        /// </summary>
        public async Task<Dictionary<DateTime, PriceData>> GetOHLCDataAsync(string coinId, string vsCurrency, string days)
        {
            try
            {
                string endpoint = $"/coins/{coinId}/ohlc";
                string url = $"{_baseUrl}{endpoint}?vs_currency={vsCurrency}&days={days}";
                
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var jsonArray = JsonDocument.Parse(content).RootElement;
                
                var priceDataDict = new Dictionary<DateTime, PriceData>();
                
                foreach (var item in jsonArray.EnumerateArray())
                {
                    long timestamp = (long)item[0].GetDecimal();
                    var dateTime = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime;
                    
                    double open = (double)item[1].GetDecimal();
                    double high = (double)item[2].GetDecimal();
                    double low = (double)item[3].GetDecimal();
                    double close = (double)item[4].GetDecimal();
                    
                    priceDataDict[dateTime] = new PriceData(open, high, low, close, 0);
                }
                
                return priceDataDict;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Error fetching OHLC data: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                throw new Exception($"Error parsing OHLC JSON response: {ex.Message}", ex);
            }
        }
    }
}