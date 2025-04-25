
namespace AIAgentCryptoTrading.Core.Models
{
    public class PriceData
    {
        public DateTime Timestamp { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public double Volume { get; set; }
        
        // Technical indicators
        public double RSI { get; set; }
        public double BBMiddle { get; set; }
        public double BBUpper { get; set; }
        public double BBLower { get; set; }
        public int Signal { get; set; }

        public PriceData() { }

        public PriceData(double open, double high, double low, double close, double volume)
        {
            Open = open;
            High = high;
            Low = low;
            Close = close;
            Volume = volume;
            Signal = 0;
        }
    }
}