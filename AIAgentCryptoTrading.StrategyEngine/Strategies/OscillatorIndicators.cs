namespace AIAgentCryptoTrading.StrategyEngine.Indicators
{
    /// <summary>
    /// Specialized service for oscillator indicators like RSI
    /// </summary>
    public static class OscillatorIndicators
    {
        /// <summary>
        /// Calculate Relative Strength Index (RSI)
        /// </summary>
        public static float[] RSI(float[] prices, int period = 14)
        {
            return TechnicalIndicators.RSI(prices, period);
        }
        
        /// <summary>
        /// Get RSI-based trading signals
        /// </summary>
        public static int[] GetSignals(float[] rsi, float overbought = 70, float oversold = 30)
        {
            if (rsi == null)
                return System.Array.Empty<int>();
                
            var signals = new int[rsi.Length];
            
            for (int i = 0; i < rsi.Length; i++)
            {
                if (float.IsNaN(rsi[i]))
                {
                    signals[i] = 0;
                    continue;
                }
                
                if (rsi[i] <= oversold)
                {
                    signals[i] = 1; // Buy signal (oversold)
                }
                else if (rsi[i] >= overbought)
                {
                    signals[i] = -1; // Sell signal (overbought)
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