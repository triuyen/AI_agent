namespace AIAgentCryptoTrading.StrategyEngine.Indicators
{
    /// <summary>
    /// Specialized service for volatility-based indicators
    /// </summary>
    public static class VolatilityIndicators
    {
        /// <summary>
        /// Calculate Bollinger Bands
        /// </summary>
        public static (float[] upper, float[] middle, float[] lower) BollingerBands(
            float[] prices, 
            int period = 20, 
            float stdDevMultiplier = 2.0f)
        {
            return TechnicalIndicators.BollingerBands(prices, period, stdDevMultiplier);
        }
        
        /// <summary>
        /// Calculate volatility (standard deviation)
        /// </summary>
        public static float[] Volatility(float[] prices, int period)
        {
            return TechnicalIndicators.Volatility(prices, period);
        }
        
        /// <summary>
        /// Get Bollinger Bands-based trading signals
        /// </summary>
        public static int[] GetBollingerSignals(float[] prices, float[] upper, float[] lower)
        {
            if (prices == null || upper == null || lower == null || 
                prices.Length != upper.Length || prices.Length != lower.Length)
                return System.Array.Empty<int>();
                
            var signals = new int[prices.Length];
            
            for (int i = 0; i < prices.Length; i++)
            {
                if (float.IsNaN(prices[i]) || float.IsNaN(upper[i]) || float.IsNaN(lower[i]))
                {
                    signals[i] = 0;
                    continue;
                }
                
                if (prices[i] <= lower[i])
                {
                    signals[i] = 1; // Buy signal (price at lower band)
                }
                else if (prices[i] >= upper[i])
                {
                    signals[i] = -1; // Sell signal (price at upper band)
                }
                else
                {
                    signals[i] = 0; // No signal
                }
            }
            
            return signals;
        }
    }
}