using System;

namespace AIAgentCryptoTrading.StrategyEngine.Indicators
{
    /// <summary>
    /// Specialized service for Moving Average indicators
    /// </summary>
    public static class MovingAverageIndicators
    {
        /// <summary>
        /// Calculate Simple Moving Average (SMA)
        /// </summary>
        public static float[] SMA(float[] prices, int period)
        {
            return TechnicalIndicators.SMA(prices, period);
        }
        
        /// <summary>
        /// Calculate Exponential Moving Average (EMA)
        /// </summary>
        public static float[] EMA(float[] prices, int period)
        {
            return TechnicalIndicators.EMA(prices, period);
        }
        
        /// <summary>
        /// Detect Moving Average Crossovers
        /// </summary>
        public static int[] DetectCrossover(float[] shortMA, float[] longMA)
        {
            return TechnicalIndicators.MACrossover(shortMA, longMA);
        }
    }
}