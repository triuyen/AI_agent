using System;
using System.Linq;

namespace AIAgentCryptoTrading.StrategyEngine.Indicators
{
    /// <summary>
    /// Core technical indicators implementation
    /// </summary>
    public static class TechnicalIndicators
    {
        /// <summary>
        /// Calculate Simple Moving Average (SMA)
        /// </summary>
        public static float[] SMA(float[] prices, int period)
        {
            if (prices == null || prices.Length < period)
                return Array.Empty<float>();
                
            var sma = new float[prices.Length];
            
            for (int i = 0; i < prices.Length; i++)
            {
                if (i < period - 1)
                {
                    sma[i] = float.NaN;
                    continue;
                }
                
                float sum = 0;
                for (int j = 0; j < period; j++)
                {
                    sum += prices[i - j];
                }
                
                sma[i] = sum / period;
            }
            
            return sma;
        }
        
        /// <summary>
        /// Calculate Relative Strength Index (RSI)
        /// </summary>
        public static float[] RSI(float[] prices, int period = 14)
        {
            if (prices == null || prices.Length <= period)
                return Array.Empty<float>();
                
            var rsi = new float[prices.Length];
            var gains = new float[prices.Length];
            var losses = new float[prices.Length];
            
            // Calculate price changes
            for (int i = 1; i < prices.Length; i++)
            {
                float change = prices[i] - prices[i - 1];
                gains[i] = Math.Max(0, change);
                losses[i] = Math.Max(0, -change);
            }
            
            // Calculate initial averages
            float avgGain = gains.Skip(1).Take(period).Average();
            float avgLoss = losses.Skip(1).Take(period).Average();
            
            // Fill in RSI values
            for (int i = 0; i < period; i++)
            {
                rsi[i] = float.NaN;
            }
            
            // Calculate first RSI
            if (avgLoss == 0) rsi[period] = 100;
            else
            {
                float rs = avgGain / avgLoss;
                rsi[period] = 100 - (100 / (1 + rs));
            }
            
            // Calculate remaining RSI values using Wilder's smoothing method
            for (int i = period + 1; i < prices.Length; i++)
            {
                avgGain = ((avgGain * (period - 1)) + gains[i]) / period;
                avgLoss = ((avgLoss * (period - 1)) + losses[i]) / period;
                
                if (avgLoss == 0) rsi[i] = 100;
                else
                {
                    float rs = avgGain / avgLoss;
                    rsi[i] = 100 - (100 / (1 + rs));
                }
            }
            
            return rsi;
        }
        
        /// <summary>
        /// Calculate Bollinger Bands
        /// </summary>
        public static (float[] upper, float[] middle, float[] lower) BollingerBands(float[] prices, int period = 20, float stdDevMultiplier = 2.0f)
        {
            if (prices == null || prices.Length < period)
                return (Array.Empty<float>(), Array.Empty<float>(), Array.Empty<float>());
                
            var middle = SMA(prices, period);
            var upper = new float[prices.Length];
            var lower = new float[prices.Length];
            
            for (int i = period - 1; i < prices.Length; i++)
            {
                float sum = 0;
                for (int j = 0; j < period; j++)
                {
                    float deviation = prices[i - j] - middle[i];
                    sum += deviation * deviation;
                }
                
                float stdDev = (float)Math.Sqrt(sum / period);
                upper[i] = middle[i] + (stdDevMultiplier * stdDev);
                lower[i] = middle[i] - (stdDevMultiplier * stdDev);
            }
            
            return (upper, middle, lower);
        }
        
        /// <summary>
        /// Calculate Exponential Moving Average (EMA)
        /// </summary>
        public static float[] EMA(float[] prices, int period)
        {
            if (prices == null || prices.Length < period)
                return Array.Empty<float>();
                
            var ema = new float[prices.Length];
            float multiplier = 2.0f / (period + 1);
            
            // Initialize EMA with SMA
            float initialSma = 0;
            for (int i = 0; i < period; i++)
            {
                ema[i] = float.NaN;
                initialSma += prices[i];
            }
            initialSma /= period;
            
            ema[period - 1] = initialSma;
            
            // Calculate EMA
            for (int i = period; i < prices.Length; i++)
            {
                ema[i] = (prices[i] - ema[i - 1]) * multiplier + ema[i - 1];
            }
            
            return ema;
        }
        
        /// <summary>
        /// Calculate Moving Average Convergence Divergence (MACD)
        /// </summary>
        public static (float[] macd, float[] signal, float[] histogram) MACD(float[] prices, int fastPeriod = 12, int slowPeriod = 26, int signalPeriod = 9)
        {
            if (prices == null || prices.Length < Math.Max(fastPeriod, slowPeriod) + signalPeriod)
                return (Array.Empty<float>(), Array.Empty<float>(), Array.Empty<float>());
                
            var fastEMA = EMA(prices, fastPeriod);
            var slowEMA = EMA(prices, slowPeriod);
            
            var macd = new float[prices.Length];
            
            for (int i = 0; i < prices.Length; i++)
            {
                if (float.IsNaN(fastEMA[i]) || float.IsNaN(slowEMA[i]))
                {
                    macd[i] = float.NaN;
                }
                else
                {
                    macd[i] = fastEMA[i] - slowEMA[i];
                }
            }
            
            var signal = EMA(macd, signalPeriod);
            var histogram = new float[prices.Length];
            
            for (int i = 0; i < prices.Length; i++)
            {
                if (float.IsNaN(macd[i]) || float.IsNaN(signal[i]))
                {
                    histogram[i] = float.NaN;
                }
                else
                {
                    histogram[i] = macd[i] - signal[i];
                }
            }
            
            return (macd, signal, histogram);
        }
        
        /// <summary>
        /// Detect Moving Average Crossovers
        /// </summary>
        public static int[] MACrossover(float[] shortMA, float[] longMA)
        {
            if (shortMA == null || longMA == null || shortMA.Length != longMA.Length)
                return Array.Empty<int>();
                
            var signals = new int[shortMA.Length];
            
            // First point can't determine a crossover
            for (int i = 0; i < Math.Min(signals.Length, 1); i++)
            {
                signals[i] = 0;
            }
            
            // Detect crossovers
            for (int i = 1; i < shortMA.Length; i++)
            {
                if (float.IsNaN(shortMA[i]) || float.IsNaN(longMA[i]) || 
                    float.IsNaN(shortMA[i-1]) || float.IsNaN(longMA[i-1]))
                {
                    signals[i] = 0;
                    continue;
                }
                
                // Bullish crossover: short MA crosses above long MA
                if (shortMA[i] > longMA[i] && shortMA[i-1] <= longMA[i-1])
                {
                    signals[i] = 1; // Buy signal
                }
                // Bearish crossover: short MA crosses below long MA
                else if (shortMA[i] < longMA[i] && shortMA[i-1] >= longMA[i-1])
                {
                    signals[i] = -1; // Sell signal
                }
                else
                {
                    signals[i] = 0; // No crossover
                }
            }
            
            return signals;
        }
        
        /// <summary>
        /// Calculate momentum
        /// </summary>
        public static float[] Momentum(float[] prices, int period)
        {
            if (prices == null || prices.Length <= period)
                return Array.Empty<float>();
                
            var momentum = new float[prices.Length];
            
            for (int i = 0; i < period; i++)
            {
                momentum[i] = float.NaN;
            }
            
            for (int i = period; i < prices.Length; i++)
            {
                momentum[i] = prices[i] / prices[i - period] - 1;
            }
            
            return momentum;
        }
        
        /// <summary>
        /// Calculate volatility (standard deviation)
        /// </summary>
        public static float[] Volatility(float[] prices, int period)
        {
            if (prices == null || prices.Length < period)
                return Array.Empty<float>();
                
            var volatility = new float[prices.Length];
            
            for (int i = 0; i < period - 1; i++)
            {
                volatility[i] = float.NaN;
            }
            
            for (int i = period - 1; i < prices.Length; i++)
            {
                float sum = 0;
                float mean = 0;
                
                // Calculate mean
                for (int j = 0; j < period; j++)
                {
                    mean += prices[i - j];
                }
                mean /= period;
                
                // Calculate variance
                for (int j = 0; j < period; j++)
                {
                    float deviation = prices[i - j] - mean;
                    sum += deviation * deviation;
                }
                
                volatility[i] = (float)Math.Sqrt(sum / period);
            }
            
            return volatility;
        }
    }
}