using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using System.Text.Json;

namespace AIAgentCryptoTrading.StrategyEngine
{
    /// <summary>
    /// Random Forest prediction model input data class
    /// </summary>
    public class CryptoModelInput
    {
        [LoadColumn(0)] public string CryptoId { get; set; }
        [LoadColumn(1)] public float Price { get; set; }
        [LoadColumn(2)] public DateTime Timestamp { get; set; }
        [LoadColumn(3)] public float SMA10 { get; set; }
        [LoadColumn(4)] public float SMA30 { get; set; }
        [LoadColumn(5)] public float RSI14 { get; set; }
        [LoadColumn(6)] public float BollingerUpper { get; set; }
        [LoadColumn(7)] public float BollingerLower { get; set; }
        [LoadColumn(8)] public float PriceLag1 { get; set; }
        [LoadColumn(9)] public float PriceLag3 { get; set; }
        [LoadColumn(10)] public float PriceLag7 { get; set; }
        [LoadColumn(11)] public float PriceLag14 { get; set; }
        [LoadColumn(12)] public float PriceLag30 { get; set; }
        [LoadColumn(13)] public float MA3 { get; set; }
        [LoadColumn(14)] public float MA7 { get; set; }
        [LoadColumn(15)] public float MA14 { get; set; }
        [LoadColumn(16)] public float MA30 { get; set; }
        [LoadColumn(17)] public float Momentum7 { get; set; }
        [LoadColumn(18)] public float Momentum14 { get; set; }
        [LoadColumn(19)] public float Momentum30 { get; set; }
        [LoadColumn(20)] public float Volatility7 { get; set; }
        [LoadColumn(21)] public float Volatility14 { get; set; }
        [LoadColumn(22)] public float Volatility30 { get; set; }
        [LoadColumn(23)] public float Volume { get; set; }
        [LoadColumn(24)] public float VolumeMA7 { get; set; }
        [LoadColumn(25)] public float VolumeMA14 { get; set; }
        [LoadColumn(26)] public float DayOfWeek { get; set; }
        [LoadColumn(27)] public float Month { get; set; }
        [LoadColumn(28)] public float HighLowSpread { get; set; }
        [LoadColumn(29)] public float MACrossover { get; set; } // 1 for bullish, -1 for bearish, 0 for no crossover
    }

    /// <summary>
    /// Random Forest prediction output class
    /// </summary>
    public class CryptoPricePrediction
    {
        [ColumnName("Score")] public float PredictedPrice { get; set; }
    }

    /// <summary>
    /// Combined trading signal info for frontend display
    /// </summary>
    public class TradingSignal
    {
        public string CryptoId { get; set; }
        public DateTime Timestamp { get; set; }
        public float CurrentPrice { get; set; }
        public float PredictedPrice { get; set; }
        public float PredictedPriceChangePercent { get; set; }
        public string Signal { get; set; }  // "BUY", "SELL", "HOLD"
        public string Strategy { get; set; } // "RF", "MA_CROSSOVER", "MEAN_REVERSION", "COMBINED"
        public Dictionary<string, object> StrategyParameters { get; set; }
        public Dictionary<string, float> IndicatorValues { get; set; }
    }

    /// <summary>
    /// Random Forest Model Manager - handles model training, prediction and persistence
    /// </summary>
    public class RandomForestModelManager
    {
        private readonly MLContext _mlContext;
        private ITransformer _model;
        private readonly string _modelPath;
        
        public RandomForestModelManager(string modelPath = "crypto_rf_model.zip")
        {
            _mlContext = new MLContext(seed: 42);
            _modelPath = modelPath;
            
            // Try to load existing model
            if (File.Exists(_modelPath))
            {
                try
                {
                    _model = _mlContext.Model.Load(_modelPath, out _);
                    Console.WriteLine($"Loaded existing model from {_modelPath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load model: {ex.Message}");
                    _model = null;
                }
            }
        }
        
        /// <summary>
        /// Trains a new Random Forest model using the provided data
        /// </summary>
        public void TrainModel(IEnumerable<CryptoModelInput> trainingData, string targetColumn = "NextDayPrice")
        {
            Console.WriteLine("Starting Random Forest model training...");
            
            // Convert training data to IDataView
            var data = _mlContext.Data.LoadFromEnumerable(trainingData);
            
            // Split data into training and test sets
            var dataSplit = _mlContext.Data.TrainTestSplit(data, testFraction: 0.25);
            
            // Define preprocessing pipeline for both numerical and categorical data
            var pipeline = _mlContext.Transforms.Categorical.OneHotEncoding("CryptoIdEncoded", "CryptoId")
                .Append(_mlContext.Transforms.Concatenate("Features", 
                    "CryptoIdEncoded", "SMA10", "SMA30", "RSI14", 
                    "BollingerUpper", "BollingerLower", "PriceLag1", "PriceLag3", 
                    "PriceLag7", "PriceLag14", "PriceLag30", "MA3", "MA7", "MA14", 
                    "MA30", "Momentum7", "Momentum14", "Momentum30", "Volatility7", 
                    "Volatility14", "Volatility30", "Volume", "VolumeMA7", "VolumeMA14", 
                    "DayOfWeek", "Month", "HighLowSpread", "MACrossover"))
                .Append(_mlContext.Transforms.NormalizeMinMax("NormalizedFeatures", "Features"))
                .Append(_mlContext.Transforms.ReplaceMissingValues("Features"))
                .Append(_mlContext.Regression.Trainers.FastForest(
                    numberOfTrees: 100,
                    numberOfLeaves: 20,
                    minimumExampleCountPerLeaf: 10,
                    labelColumnName: targetColumn,
                    featureColumnName: "NormalizedFeatures"));
            
            // Train the model
            Console.WriteLine("Fitting model to training data...");
            _model = pipeline.Fit(dataSplit.TrainSet);
            
            // Evaluate the model
            var predictions = _model.Transform(dataSplit.TestSet);
            var metrics = _mlContext.Regression.Evaluate(predictions);
            
            Console.WriteLine($"Model training complete.");
            Console.WriteLine($"R² Score: {metrics.RSquared:F4}");
            Console.WriteLine($"RMSE: {metrics.RootMeanSquaredError:F4}");
            Console.WriteLine($"MAE: {metrics.MeanAbsoluteError:F4}");
            
            // Save the model
            _mlContext.Model.Save(_model, data.Schema, _modelPath);
            Console.WriteLine($"Model saved to {_modelPath}");
        }
        
        /// <summary>
        /// Predicts next day price using the trained model
        /// </summary>
        public float PredictNextDayPrice(CryptoModelInput input)
        {
            if (_model == null)
            {
                throw new InvalidOperationException("Model not loaded or trained");
            }
            
            // Create prediction engine
            var predEngine = _mlContext.Model.CreatePredictionEngine<CryptoModelInput, CryptoPricePrediction>(_model);
            
            // Make prediction
            var prediction = predEngine.Predict(input);
            return prediction.PredictedPrice;
        }
        
        /// <summary>
        /// Batch prediction for multiple inputs
        /// </summary>
        public IEnumerable<float> PredictBatch(IEnumerable<CryptoModelInput> inputs)
        {
            if (_model == null)
            {
                throw new InvalidOperationException("Model not loaded or trained");
            }
            
            var predictions = new List<float>();
            var predEngine = _mlContext.Model.CreatePredictionEngine<CryptoModelInput, CryptoPricePrediction>(_model);
            
            foreach (var input in inputs)
            {
                predictions.Add(predEngine.Predict(input).PredictedPrice);
            }
            
            return predictions;
        }
    }

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
        public static float[] CalculateRSI(float[] prices, int period = 14)
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
        public static (float[] upper, float[] middle, float[] lower) CalculateBollingerBands(float[] prices, int period = 20, float stdDevMultiplier = 2.0f)
        {
            if (prices == null || prices.Length < period)
                return (Array.Empty<float>(), Array.Empty<float>(), Array.Empty<float>());
                
            var middle = CalculateSMA(prices, period);
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
        /// Calculate Moving Average Convergence Divergence (MACD)
        /// </summary>
        public static (float[] macd, float[] signal, float[] histogram) CalculateMACD(float[] prices, int fastPeriod = 12, int slowPeriod = 26, int signalPeriod = 9)
        {
            if (prices == null || prices.Length < Math.Max(fastPeriod, slowPeriod) + signalPeriod)
                return (Array.Empty<float>(), Array.Empty<float>(), Array.Empty<float>());
                
            var fastEMA = CalculateEMA(prices, fastPeriod);
            var slowEMA = CalculateEMA(prices, slowPeriod);
            
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
            
            var signal = CalculateEMA(macd, signalPeriod);
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
        /// Calculate Exponential Moving Average (EMA)
        /// </summary>
        public static float[] CalculateEMA(float[] prices, int period)
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
        /// Detect Moving Average Crossovers
        /// </summary>
        public static int[] DetectMACrossover(float[] shortMA, float[] longMA)
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
        public static float[] CalculateMomentum(float[] prices, int period)
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
        public static float[] CalculateVolatility(float[] prices, int period)
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

    /// <summary>
    /// Strategy that combines Random Forest predictions with technical indicators
    /// </summary>
    public class CombinedTradingStrategy
    {
        private readonly RandomForestModelManager _modelManager;
        private readonly float _rfBuyThreshold;
        private readonly float _rfSellThreshold;
        private readonly int _shortMAPeriod;
        private readonly int _longMAPeriod;
        private readonly int _rsiPeriod;
        private readonly float _rsiOverbought;
        private readonly float _rsiOversold;
        
        public CombinedTradingStrategy(
            RandomForestModelManager modelManager,
            float rfBuyThreshold = 1.5f,
            float rfSellThreshold = -1.5f,
            int shortMAPeriod = 10,
            int longMAPeriod = 30,
            int rsiPeriod = 14,
            float rsiOverbought = 70,
            float rsiOversold = 30)
        {
            _modelManager = modelManager;
            _rfBuyThreshold = rfBuyThreshold;
            _rfSellThreshold = rfSellThreshold;
            _shortMAPeriod = shortMAPeriod;
            _longMAPeriod = longMAPeriod;
            _rsiPeriod = rsiPeriod;
            _rsiOverbought = rsiOverbought;
            _rsiOversold = rsiOversold;
        }
        
        /// <summary>
        /// Generate trading signals using multiple strategies
        /// </summary>
        public List<TradingSignal> GenerateSignals(List<CryptoModelInput> data, bool includeAllStrategies = true)
        {
            var result = new List<TradingSignal>();
            var cryptoGroups = data.GroupBy(d => d.CryptoId);
            
            foreach (var cryptoGroup in cryptoGroups)
            {
                var cryptoData = cryptoGroup.OrderBy(d => d.Timestamp).ToList();
                if (cryptoData.Count < Math.Max(_shortMAPeriod, _longMAPeriod))
                {
                    Console.WriteLine($"Not enough data for {cryptoGroup.Key}. Skipping.");
                    continue;
                }
                
                // Extract price array
                var prices = cryptoData.Select(d => d.Price).ToArray();
                
                // Calculate indicators
                var shortMA = TechnicalIndicatorService.CalculateSMA(prices, _shortMAPeriod);
                var longMA = TechnicalIndicatorService.CalculateSMA(prices, _longMAPeriod);
                var rsi = TechnicalIndicatorService.CalculateRSI(prices, _rsiPeriod);
                var (upperBB, middleBB, lowerBB) = TechnicalIndicatorService.CalculateBollingerBands(prices);
                var crossoverSignals = TechnicalIndicatorService.DetectMACrossover(shortMA, longMA);
                
                // Get Random Forest predictions
                float[] rfPredictions;
                try
                {
                    rfPredictions = _modelManager.PredictBatch(cryptoData).ToArray();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error generating RF predictions: {ex.Message}");
                    rfPredictions = new float[cryptoData.Count];
                }
                
                // Generate signals
                for (int i = 0; i < cryptoData.Count; i++)
                {
                    // Skip points where we don't have all indicators calculated
                    if (i < _longMAPeriod || float.IsNaN(shortMA[i]) || float.IsNaN(longMA[i]) || 
                        float.IsNaN(rsi[i]) || float.IsNaN(upperBB[i]) || float.IsNaN(lowerBB[i]))
                    {
                        continue;
                    }
                    
                    var currentPrice = cryptoData[i].Price;
                    var predictedPrice = rfPredictions[i];
                    var priceChangePercent = (predictedPrice - currentPrice) / currentPrice * 100;
                    
                    // Store common indicator values for all strategies
                    var indicatorValues = new Dictionary<string, float>
                    {
                        { "ShortMA", shortMA[i] },
                        { "LongMA", longMA[i] },
                        { "RSI", rsi[i] },
                        { "UpperBB", upperBB[i] },
                        { "LowerBB", lowerBB[i] },
                        { "PredictedPrice", predictedPrice },
                        { "PriceChangePercent", priceChangePercent }
                    };
                    
                    // RF Strategy
                    if (includeAllStrategies)
                    {
                        string rfSignal = "HOLD";
                        if (priceChangePercent > _rfBuyThreshold)
                            rfSignal = "BUY";
                        else if (priceChangePercent < _rfSellThreshold)
                            rfSignal = "SELL";
                        
                        result.Add(new TradingSignal
                        {
                            CryptoId = cryptoData[i].CryptoId,
                            Timestamp = cryptoData[i].Timestamp,
                            CurrentPrice = currentPrice,
                            PredictedPrice = predictedPrice,
                            PredictedPriceChangePercent = priceChangePercent,
                            Signal = rfSignal,
                            Strategy = "RF",
                            StrategyParameters = new Dictionary<string, object>
                            {
                                { "BuyThreshold", _rfBuyThreshold },
                                { "SellThreshold", _rfSellThreshold }
                            },
                            IndicatorValues = indicatorValues
                        });
                    }
                    
                    // Moving Average Crossover Strategy
                    if (includeAllStrategies || i == cryptoData.Count - 1)
                    {
                        string maCrossSignal = "HOLD";
                        if (crossoverSignals[i] == 1)
                            maCrossSignal = "BUY";
                        else if (crossoverSignals[i] == -1)
                            maCrossSignal = "SELL";
                        
                        result.Add(new TradingSignal
                        {
                            CryptoId = cryptoData[i].CryptoId,
                            Timestamp = cryptoData[i].Timestamp,
                            CurrentPrice = currentPrice,
                            PredictedPrice = 0, // Not using RF prediction
                            PredictedPriceChangePercent = 0,
                            Signal = maCrossSignal,
                            Strategy = "MA_CROSSOVER",
                            StrategyParameters = new Dictionary<string, object>
                            {
                                { "ShortPeriod", _shortMAPeriod },
                                { "LongPeriod", _longMAPeriod }
                            },
                            IndicatorValues = indicatorValues
                        });
                    }
                    
                    // Mean Reversion Strategy
                    if (includeAllStrategies || i == cryptoData.Count - 1)
                    {
                        string mrSignal = "HOLD";
                        
                        // RSI-based mean reversion
                        if (rsi[i] <= _rsiOversold)
                            mrSignal = "BUY"; // Oversold condition
                        else if (rsi[i] >= _rsiOverbought)
                            mrSignal = "SELL"; // Overbought condition
                        
                        // Bollinger Band-based mean reversion (takes precedence)
                        if (prices[i] <= lowerBB[i])
                            mrSignal = "BUY"; // Price at lower band
                        else if (prices[i] >= upperBB[i])
                            mrSignal = "SELL"; // Price at upper band
                        
                        result.Add(new TradingSignal
                        {
                            CryptoId = cryptoData[i].CryptoId,
                            Timestamp = cryptoData[i].Timestamp,
                            CurrentPrice = currentPrice,
                            PredictedPrice = 0, // Not using RF prediction
                            PredictedPriceChangePercent = 0,
                            Signal = mrSignal,
                            Strategy = "MEAN_REVERSION",
                            StrategyParameters = new Dictionary<string, object>
                            {
                                { "RSIPeriod", _rsiPeriod },
                                { "RSIOverbought", _rsiOverbought },
                                { "RSIOversold", _rsiOversold }
                            },
                            IndicatorValues = indicatorValues
                        });
                    }
                    
                    // Combined strategy (only add for the last data point or if including all)
                    if (i == cryptoData.Count - 1 || includeAllStrategies)
                    {
                        // Weighted decision based on all strategies
                        int buySignals = 0;
                        int sellSignals = 0;
                        
                        // RF signal
                        if (priceChangePercent > _rfBuyThreshold)
                            buySignals += 2; // Higher weight for RF predictions
                        else if (priceChangePercent < _rfSellThreshold)
                            sellSignals += 2;
                        
                        // MA crossover signal
                        if (crossoverSignals[i] == 1)
                            buySignals++;
                        else if (crossoverSignals[i] == -1)
                            sellSignals++;
                        
                        // Mean reversion signals
                        if (rsi[i] <= _rsiOversold || prices[i] <= lowerBB[i])
                            buySignals++;
                        else if (rsi[i] >= _rsiOverbought || prices[i] >= upperBB[i])
                            sellSignals++;
                        
                        string combinedSignal = "HOLD";
                        if (buySignals > sellSignals)
                            combinedSignal = "BUY";
                        else if (sellSignals > buySignals)
                            combinedSignal = "SELL";
                        
                        result.Add(new TradingSignal
                        {
                            CryptoId = cryptoData[i].CryptoId,
                            Timestamp = cryptoData[i].Timestamp,
                            CurrentPrice = currentPrice,
                            PredictedPrice = predictedPrice,
                            PredictedPriceChangePercent = priceChangePercent,
                            Signal = combinedSignal,
                            Strategy = "COMBINED",
                            StrategyParameters = new Dictionary<string, object>
                            {
                                { "BuySignals", buySignals },
                                { "SellSignals", sellSignals }
                            },
                            IndicatorValues = indicatorValues
                        });
                    }
                }
            }
            
            return result;
        }
    }
    
    /// <summary>
    /// Model training and data preparation utilities
    /// </summary>
    public class CryptoDataPreparer
    {
        /// <summary>
        /// Prepare data from a CSV file for model training and strategy evaluation
        /// </summary>
        public static async Task<List<CryptoModelInput>> PrepareDataFromCsv(string filePath)
        {
            var result = new List<CryptoModelInput>();
            
            try
            {
                var lines = await File.ReadAllLinesAsync(filePath);
                if (lines.Length <= 1)
                {
                    throw new InvalidOperationException("CSV file is empty or contains only headers");
                }
                
                // Parse header
                var headers = lines[0].Split(',');
                
                // Process data rows
                for (int i = 1; i < lines.Length; i++)
                {
                    var values = lines[i].Split(',');
                    if (values.Length != headers.Length)
                    {
                        Console.WriteLine($"Warning: Row {i} has {values.Length} columns, expected {headers.Length}. Skipping.");
                        continue;
                    }
                    
                    try
                    {
                        // Create basic model input with required fields
                        var input = new CryptoModelInput
                        {
                            CryptoId = GetValueByHeader(headers, values, "crypto_id"),
                            Price = float.Parse(GetValueByHeader(headers, values, "price")),
                            Timestamp = DateTime.Parse(GetValueByHeader(headers, values, "timestamp"))
                        };
                        
                        result.Add(input);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error parsing row {i}: {ex.Message}");
                    }
                }
                
                // Sort data by crypto and timestamp
                result = result.OrderBy(d => d.CryptoId).ThenBy(d => d.Timestamp).ToList();
                
                // Group by crypto for calculating indicators
                var cryptoGroups = result.GroupBy(d => d.CryptoId);
                var enrichedData = new List<CryptoModelInput>();
                
                foreach (var group in cryptoGroups)
                {
                    var cryptoData = group.OrderBy(d => d.Timestamp).ToList();
                    var prices = cryptoData.Select(d => d.Price).ToArray();
                    
                    // Calculate all technical indicators
                    var sma10 = TechnicalIndicatorService.CalculateSMA(prices, 10);
                    var sma30 = TechnicalIndicatorService.CalculateSMA(prices, 30);
                    var rsi14 = TechnicalIndicatorService.CalculateRSI(prices);
                    var (upperBB, _, lowerBB) = TechnicalIndicatorService.CalculateBollingerBands(prices);
                    var ma3 = TechnicalIndicatorService.CalculateSMA(prices, 3);
                    var ma7 = TechnicalIndicatorService.CalculateSMA(prices, 7);
                    var ma14 = TechnicalIndicatorService.CalculateSMA(prices, 14);
                    var ma30 = TechnicalIndicatorService.CalculateSMA(prices, 30);
                    var mom7 = TechnicalIndicatorService.CalculateMomentum(prices, 7);
                    var mom14 = TechnicalIndicatorService.CalculateMomentum(prices, 14);
                    var mom30 = TechnicalIndicatorService.CalculateMomentum(prices, 30);
                    var vol7 = TechnicalIndicatorService.CalculateVolatility(prices, 7);
                    var vol14 = TechnicalIndicatorService.CalculateVolatility(prices, 14);
                    var vol30 = TechnicalIndicatorService.CalculateVolatility(prices, 30);
                    var macross = TechnicalIndicatorService.DetectMACrossover(sma10, sma30);
                    
                    // Create lag features
                    for (int i = 0; i < cryptoData.Count; i++)
                    {
                        var input = cryptoData[i];
                        
                        // Add technical indicators
                        if (i >= 30) // Ensure we have enough data for all indicators
                        {
                            input.SMA10 = sma10[i];
                            input.SMA30 = sma30[i];
                            input.RSI14 = rsi14[i];
                            input.BollingerUpper = upperBB[i];
                            input.BollingerLower = lowerBB[i];
                            input.MA3 = ma3[i];
                            input.MA7 = ma7[i];
                            input.MA14 = ma14[i];
                            input.MA30 = ma30[i];
                            input.Momentum7 = mom7[i];
                            input.Momentum14 = mom14[i];
                            input.Momentum30 = mom30[i];
                            input.Volatility7 = vol7[i];
                            input.Volatility14 = vol14[i];
                            input.Volatility30 = vol30[i];
                            input.MACrossover = macross[i];
                            
                            // Add lag features
                            input.PriceLag1 = i >= 1 ? prices[i - 1] : 0;
                            input.PriceLag3 = i >= 3 ? prices[i - 3] : 0;
                            input.PriceLag7 = i >= 7 ? prices[i - 7] : 0;
                            input.PriceLag14 = i >= 14 ? prices[i - 14] : 0;
                            input.PriceLag30 = i >= 30 ? prices[i - 30] : 0;
                            
                            // Add time-based features
                            input.DayOfWeek = (float)input.Timestamp.DayOfWeek;
                            input.Month = input.Timestamp.Month;
                            
                            // Add to enriched dataset
                            enrichedData.Add(input);
                        }
                    }
                }
                
                Console.WriteLine($"Prepared {enrichedData.Count} data points for model training");
                return enrichedData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error preparing data: {ex.Message}");
                return new List<CryptoModelInput>();
            }
        }
        
        private static string GetValueByHeader(string[] headers, string[] values, string headerName)
        {
            int index = Array.IndexOf(headers, headerName);
            if (index == -1)
            {
                throw new InvalidOperationException($"Header '{headerName}' not found in CSV");
            }
            
            return values[index];
        }
    }
}

namespace AIAgentCryptoTrading.Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AIAgentCryptoTrading.StrategyEngine;
    using System.Linq;
    
    [ApiController]
    [Route("api/[controller]")]
    public class TradingStrategyController : ControllerBase
    {
        private readonly RandomForestModelManager _modelManager;
        private readonly CombinedTradingStrategy _tradingStrategy;
        
        public TradingStrategyController(RandomForestModelManager modelManager)
        {
            _modelManager = modelManager;
            _tradingStrategy = new CombinedTradingStrategy(modelManager);
        }
        
        [HttpGet("signals/{cryptoId}")]
        public async Task<ActionResult<IEnumerable<TradingSignal>>> GetSignals(string cryptoId, [FromQuery] string strategy = "COMBINED")
        {
            try
            {
                // In a real app, you'd pull this from a database or real-time data source
                var data = await CryptoDataPreparer.PrepareDataFromCsv("comprehensive_daily_crypto_data_with_derived_ohlc.csv");
                
                // Filter by requested crypto
                data = data.Where(d => d.CryptoId.Equals(cryptoId, StringComparison.OrdinalIgnoreCase)).ToList();
                
                if (!data.Any())
                {
                    return NotFound($"No data found for crypto {cryptoId}");
                }
                
                // Generate signals
                var signals = _tradingStrategy.GenerateSignals(data);
                
                // Filter by requested strategy
                if (!string.IsNullOrEmpty(strategy) && strategy != "ALL")
                {
                    signals = signals.Where(s => s.Strategy.Equals(strategy, StringComparison.OrdinalIgnoreCase)).ToList();
                }
                
                return Ok(signals);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error generating trading signals: {ex.Message}");
            }
        }
        
        [HttpGet("strategies")]
        public ActionResult<IEnumerable<string>> GetAvailableStrategies()
        {
            return Ok(new[] { "RF", "MA_CROSSOVER", "MEAN_REVERSION", "COMBINED" });
        }
        
        [HttpGet("backtest/{cryptoId}")]
        public async Task<ActionResult<object>> BacktestStrategy(
            string cryptoId, 
            [FromQuery] string strategy = "COMBINED", 
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                // Load and prepare data
                var data = await CryptoDataPreparer.PrepareDataFromCsv("comprehensive_daily_crypto_data_with_derived_ohlc.csv");
                
                // Filter by crypto and date range
                var filteredData = data
                    .Where(d => d.CryptoId.Equals(cryptoId, StringComparison.OrdinalIgnoreCase))
                    .Where(d => !startDate.HasValue || d.Timestamp >= startDate)
                    .Where(d => !endDate.HasValue || d.Timestamp <= endDate)
                    .ToList();
                
                if (!filteredData.Any())
                {
                    return NotFound($"No data found for crypto {cryptoId} in the specified date range");
                }
                
                // Generate signals
                var signals = _tradingStrategy.GenerateSignals(filteredData)
                    .Where(s => s.Strategy.Equals(strategy, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                
                // Simulate trading based on signals
                var initialBalance = 10000.0f; // Start with $10,000
                var balance = initialBalance;
                var holding = 0.0f; // Amount of crypto held
                var trades = new List<object>();
                
                for (int i = 0; i < signals.Count; i++)
                {
                    var signal = signals[i];
                    
                    if (signal.Signal == "BUY" && balance > 0)
                    {
                        // Buy with all available balance
                        var amount = balance / signal.CurrentPrice;
                        holding += amount;
                        balance = 0;
                        
                        trades.Add(new
                        {
                            Type = "BUY",
                            Date = signal.Timestamp,
                            Price = signal.CurrentPrice,
                            Amount = amount,
                            Balance = balance,
                            Holdings = holding,
                            Value = balance + (holding * signal.CurrentPrice)
                        });
                    }
                    else if (signal.Signal == "SELL" && holding > 0)
                    {
                        // Sell all holdings
                        balance += holding * signal.CurrentPrice;
                        holding = 0;
                        
                        trades.Add(new
                        {
                            Type = "SELL",
                            Date = signal.Timestamp,
                            Price = signal.CurrentPrice,
                            Amount = holding,
                            Balance = balance,
                            Holdings = holding,
                            Value = balance
                        });
                    }
                }
                
                // Calculate final value and returns
                var lastPrice = filteredData.Last().Price;
                var finalValue = balance + (holding * lastPrice);
                var returnPct = (finalValue - initialBalance) / initialBalance * 100;
                
                // Calculate benchmark buy-and-hold return
                var firstPrice = filteredData.First().Price;
                var buyHoldShares = initialBalance / firstPrice;
                var buyHoldValue = buyHoldShares * lastPrice;
                var buyHoldReturn = (buyHoldValue - initialBalance) / initialBalance * 100;
                
                return Ok(new
                {
                    Strategy = strategy,
                    InitialBalance = initialBalance,
                    FinalBalance = balance,
                    FinalHoldings = holding,
                    FinalValue = finalValue,
                    Return = returnPct,
                    BuyHoldReturn = buyHoldReturn,
                    Trades = trades,
                    PeriodStart = filteredData.First().Timestamp,
                    PeriodEnd = filteredData.Last().Timestamp
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error backtesting strategy: {ex.Message}");
            }
        }
    }
}