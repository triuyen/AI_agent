using AIAgentCryptoTrading.StrategyEngine.Indicators;

namespace AIAgentCryptoTrading.StrategyEngine
{
    /// <summary>
    /// Technical indicator calculation service
    /// </summary>
    public class TechnicalIndicatorService
    {
        /// <summary>
        /// Calculate Simple Moving Average
        /// </summary>
        public static float[] CalculateSMA(float[] prices, int period)
        {
            return TechnicalIndicators.SMA(prices, period);
        }
        
        /// <summary>
        /// Calculate Relative Strength Index (RSI)
        /// </summary>
        public static float[] CalculateRSI(float[] prices, int period = 14)
        {
            return TechnicalIndicators.RSI(prices, period);
        }
        
        /// <summary>
        /// Calculate Bollinger Bands
        /// </summary>
        public static (float[] upper, float[] middle, float[] lower) CalculateBollingerBands(float[] prices, int period = 20, float stdDevMultiplier = 2.0f)
        {
            return TechnicalIndicators.BollingerBands(prices, period, stdDevMultiplier);
        }
        
        /// <summary>
        /// Calculate Moving Average Convergence Divergence (MACD)
        /// </summary>
        public static (float[] macd, float[] signal, float[] histogram) CalculateMACD(
            float[] prices, 
            int fastPeriod = 12, 
            int slowPeriod = 26, 
            int signalPeriod = 9)
        {
            return TechnicalIndicators.MACD(prices, fastPeriod, slowPeriod, signalPeriod);
        }
        
        /// <summary>
        /// Calculate Exponential Moving Average (EMA)
        /// </summary>
        public static float[] CalculateEMA(float[] prices, int period)
        {
            return TechnicalIndicators.EMA(prices, period);
        }
        
        /// <summary>
        /// Detect Moving Average Crossovers
        /// </summary>
        public static int[] DetectMACrossover(float[] shortMA, float[] longMA)
        {
            return TechnicalIndicators.MACrossover(shortMA, longMA);
        }
        
        /// <summary>
        /// Calculate momentum
        /// </summary>
        public static float[] CalculateMomentum(float[] prices, int period)
        {
            return TechnicalIndicators.Momentum(prices, period);
        }
        
        /// <summary>
        /// Calculate volatility (standard deviation)
        /// </summary>
        public static float[] CalculateVolatility(float[] prices, int period)
        {
            return TechnicalIndicators.Volatility(prices, period);
        }
    }
}