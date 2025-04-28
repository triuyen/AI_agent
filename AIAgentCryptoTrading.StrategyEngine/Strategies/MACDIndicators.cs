namespace AIAgentCryptoTrading.StrategyEngine.Indicators
{
    /// <summary>
    /// Specialized service for MACD indicators
    /// </summary>
    public static class MACDIndicators
    {
        /// <summary>
        /// Calculate Moving Average Convergence Divergence (MACD)
        /// </summary>
        public static (float[] macd, float[] signal, float[] histogram) Calculate(
            float[] prices, 
            int fastPeriod = 12, 
            int slowPeriod = 26, 
            int signalPeriod = 9)
        {
            return TechnicalIndicators.MACD(prices, fastPeriod, slowPeriod, signalPeriod);
        }
        
        /// <summary>
        /// Get MACD-based trading signals
        /// </summary>
        public static int[] GetSignals(float[] macd, float[] signal)
        {
            if (macd == null || signal == null || macd.Length != signal.Length)
                return Array.Empty<int>();
                
            var signals = new int[macd.Length];
            
            // First point can't determine a crossover
            for (int i = 0; i < System.Math.Min(signals.Length, 1); i++)
            {
                signals[i] = 0;
            }
            
            // Detect crossovers
            for (int i = 1; i < macd.Length; i++)
            {
                if (float.IsNaN(macd[i]) || float.IsNaN(signal[i]) || 
                    float.IsNaN(macd[i-1]) || float.IsNaN(signal[i-1]))
                {
                    signals[i] = 0;
                    continue;
                }
                
                // Bullish crossover: MACD crosses above signal line
                if (macd[i] > signal[i] && macd[i-1] <= signal[i-1])
                {
                    signals[i] = 1; // Buy signal
                }
                // Bearish crossover: MACD crosses below signal line
                else if (macd[i] < signal[i] && macd[i-1] >= signal[i-1])
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
    }
}