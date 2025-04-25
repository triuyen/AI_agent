
using System;
using System.Collections.Generic;
using System.Linq;
using AIAgentCryptoTrading.Core.Models;

namespace AIAgentCryptoTrading.StrategyEngine.Strategies
{
    public static class MeanReversionStrategy
    {
        /// <summary>
        /// Mean reversion strategy using RSI and Bollinger Bands
        /// </summary>
        public static Dictionary<DateTime, PriceData> ExecuteStrategy(
            Dictionary<DateTime, PriceData> data,
            int rsiPeriod = 14,
            double rsiOversold = 30,
            double rsiOverbought = 70,
            int bbPeriod = 20,
            double bbStd = 2,
            bool exitMiddle = true)
        {
            var sortedData = data.OrderBy(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            var dataList = sortedData.Values.ToList();
            var dates = sortedData.Keys.ToList();

            // Calculate RSI
            CalculateRSI(dataList, rsiPeriod);

            // Calculate Bollinger Bands
            CalculateBollingerBands(dataList, bbPeriod, bbStd);

            // Initialize signal column
            foreach (var item in dataList)
            {
                item.Signal = 0;
            }

            // Generate signals
            for (int i = 1; i < dataList.Count; i++)
            {
                // Buy signals - when price is below lower band AND RSI is oversold
                if (dataList[i].Close < dataList[i].BBLower && dataList[i].RSI < rsiOversold)
                {
                    dataList[i].Signal = 1;
                }
                // Sell signals - when price is above upper band AND RSI is overbought
                else if (dataList[i].Close > dataList[i].BBUpper && dataList[i].RSI > rsiOverbought)
                {
                    dataList[i].Signal = -1;
                }
                // Exit signals - when price crosses middle band (if enabled)
                else if (exitMiddle)
                {
                    if (dataList[i - 1].Signal == 1 && dataList[i].Close > dataList[i].BBMiddle)
                    {
                        dataList[i].Signal = 0;
                    }
                    else if (dataList[i - 1].Signal == -1 && dataList[i].Close < dataList[i].BBMiddle)
                    {
                        dataList[i].Signal = 0;
                    }
                }
            }

            // Reconstruct the dictionary with updated data
            var result = new Dictionary<DateTime, PriceData>();
            for (int i = 0; i < dates.Count; i++)
            {
                result[dates[i]] = dataList[i];
            }

            return result;
        }

        private static void CalculateRSI(List<PriceData> data, int period)
        {
            if (data.Count <= period)
            {
                foreach (var item in data)
                {
                    item.RSI = 0;
                }
                return;
            }

            List<double> gains = new List<double>();
            List<double> losses = new List<double>();

            // Calculate price changes
            for (int i = 1; i < data.Count; i++)
            {
                double change = data[i].Close - data[i - 1].Close;
                gains.Add(Math.Max(0, change));
                losses.Add(Math.Max(0, -change));
            }

            // Calculate initial averages
            double avgGain = gains.Take(period).Average();
            double avgLoss = losses.Take(period).Average();

            // Set initial RSI
            for (int i = 0; i < period; i++)
            {
                data[i].RSI = 0; // Not enough data for calculation
            }

            data[period].RSI = 100 - (100 / (1 + (avgGain / Math.Max(avgLoss, 0.0001))));

            // Calculate RSI for the rest of the data
            for (int i = period + 1; i < data.Count; i++)
            {
                avgGain = ((avgGain * (period - 1)) + gains[i - 1]) / period;
                avgLoss = ((avgLoss * (period - 1)) + losses[i - 1]) / period;
                
                double rs = avgGain / Math.Max(avgLoss, 0.0001); // Avoid division by zero
                data[i].RSI = 100 - (100 / (1 + rs));
            }
        }

        private static void CalculateBollingerBands(List<PriceData> data, int period, double stdDev)
        {
            for (int i = 0; i < data.Count; i++)
            {
                if (i < period - 1)
                {
                    data[i].BBMiddle = data[i].Close;
                    data[i].BBUpper = data[i].Close;
                    data[i].BBLower = data[i].Close;
                    continue;
                }

                var window = data.Skip(i - period + 1).Take(period).Select(d => d.Close).ToList();
                double mean = window.Average();
                double std = CalculateStandardDeviation(window, mean);

                data[i].BBMiddle = mean;
                data[i].BBUpper = mean + (std * stdDev);
                data[i].BBLower = mean - (std * stdDev);
            }
        }

        private static double CalculateStandardDeviation(List<double> values, double mean)
        {
            if (values.Count <= 1)
                return 0;

            double sumOfSquares = values.Sum(v => Math.Pow(v - mean, 2));
            return Math.Sqrt(sumOfSquares / (values.Count - 1));
        }
    }
}