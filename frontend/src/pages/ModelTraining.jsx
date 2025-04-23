import React, { useState, useEffect } from 'react';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer, BarChart, Bar } from 'recharts';
import api from '../services/api';

const ModelTraining = () => {
  // Training data state
  const [trainingData, setTrainingData] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [selectedModel, setSelectedModel] = useState('tcn'); // Default model

  // Market data state
  const [marketData, setMarketData] = useState([]);
  const [topCryptos, setTopCryptos] = useState(['BTCUSDT', 'ETHUSDT', 'BNBUSDT']);
  const [selectedCrypto, setSelectedCrypto] = useState('BTCUSDT');
  const [timeframe, setTimeframe] = useState('1d');
  const [dataSource, setDataSource] = useState('binance');
  const [loadingMarket, setLoadingMarket] = useState(true);
  const [marketError, setMarketError] = useState(null);
  const [showMarketData, setShowMarketData] = useState(true);

  // Fetch training metrics
  useEffect(() => {
    const fetchTrainingMetrics = async () => {
      try {
        setLoading(true);
        // Replace with your actual API endpoint for training metrics
        const response = await api.get(`/api/training/metrics/${selectedModel}`);
        setTrainingData(response.data);
        setError(null);
      } catch (err) {
        setError('Failed to load training data: ' + err.message);
        // Use sample data for development until API is ready
        setTrainingData(generateSampleTrainingData());
      } finally {
        setLoading(false);
      }
    };

    fetchTrainingMetrics();
  }, [selectedModel]);

  // Fetch top cryptocurrencies
  useEffect(() => {
    const fetchTopCryptos = async () => {
      try {
        const response = await api.get('/api/marketdata/top');
        if (response.data && response.data.length > 0) {
          setTopCryptos(response.data);
        }
      } catch (err) {
        console.error("Error fetching top cryptos:", err);
        // Keep default values if there's an error
      }
    };
    
    fetchTopCryptos();
  }, []);

  // Fetch market data
  useEffect(() => {
    const fetchMarketData = async () => {
      if (!showMarketData) return;
      
      setLoadingMarket(true);
      setMarketError(null);
      
      try {
        const response = await api.get(`/api/marketdata/${selectedCrypto}`, {
          params: { 
            timeframe, 
            limit: 100,
            source: dataSource
          }
        });
        
        // Format data for charts
        const formattedData = response.data.map(candle => ({
          date: new Date(candle.timestamp).toLocaleDateString(),
          timestamp: candle.timestamp,
          open: candle.open,
          high: candle.high,
          low: candle.low,
          close: candle.close,
          volume: candle.volume,
          change: candle.close - candle.open,
          changePercent: ((candle.close - candle.open) / candle.open * 100).toFixed(2)
        }));
        
        setMarketData(formattedData);
      } catch (err) {
        setMarketError(`Failed to load market data: ${err.message}`);
        console.error(err);
      } finally {
        setLoadingMarket(false);
      }
    };
    
    fetchMarketData();
  }, [selectedCrypto, timeframe, dataSource, showMarketData]);

  // Sample data generator for development
  const generateSampleTrainingData = () => {
    const data = [];
    for (let epoch = 1; epoch <= 30; epoch++) {
      data.push({
        epoch,
        loss: 1 / (epoch * 0.1 + 1) + Math.random() * 0.1,
        accuracy: 0.5 + epoch * 0.015 + Math.random() * 0.02,
        validationLoss: 1 / (epoch * 0.08 + 1) + Math.random() * 0.15,
        validationAccuracy: 0.45 + epoch * 0.014 + Math.random() * 0.03,
      });
    }
    return data;
  };

  // Format currency for display
  const formatCurrency = (value) => {
    return new Intl.NumberFormat('en-US', { 
      style: 'currency', 
      currency: 'USD',
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    }).format(value);
  };

  return (
    <div className="model-training-page">
      <h1>Model Training Visualization</h1>
      
      <div className="model-selector">
        <label htmlFor="model-select">Select Model:</label>
        <select 
          id="model-select" 
          value={selectedModel} 
          onChange={(e) => setSelectedModel(e.target.value)}
        >
          <option value="tcn">Temporal Convolutional Network</option>
          <option value="lstm">LSTM Network</option>
          <option value="transformer">Transformer Model</option>
        </select>
      </div>

      {loading && <p>Loading training data...</p>}
      
      {error && <div className="error-message">{error}</div>}
      
      {!loading && !error && (
        <div className="charts-container">
          <div className="chart-wrapper">
            <h2>Training & Validation Loss</h2>
            <ResponsiveContainer width="100%" height={300}>
              <LineChart data={trainingData}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="epoch" label={{ value: 'Epoch', position: 'bottom' }} />
                <YAxis label={{ value: 'Loss', angle: -90, position: 'insideLeft' }} />
                <Tooltip />
                <Legend />
                <Line type="monotone" dataKey="loss" stroke="#8884d8" name="Training Loss" />
                <Line type="monotone" dataKey="validationLoss" stroke="#82ca9d" name="Validation Loss" />
              </LineChart>
            </ResponsiveContainer>
          </div>
          
          <div className="chart-wrapper">
            <h2>Training & Validation Accuracy</h2>
            <ResponsiveContainer width="100%" height={300}>
              <LineChart data={trainingData}>
                <CartesianGrid strokeDasharray="3 3" />
                <XAxis dataKey="epoch" label={{ value: 'Epoch', position: 'bottom' }} />
                <YAxis label={{ value: 'Accuracy', angle: -90, position: 'insideLeft' }} />
                <Tooltip />
                <Legend />
                <Line type="monotone" dataKey="accuracy" stroke="#8884d8" name="Training Accuracy" />
                <Line type="monotone" dataKey="validationAccuracy" stroke="#82ca9d" name="Validation Accuracy" />
              </LineChart>
            </ResponsiveContainer>
          </div>
        </div>
      )}
      
      <div className="training-controls">
        <h2>Model Parameters</h2>
        <div className="parameter-grid">
          <div className="parameter">
            <label>Learning Rate:</label>
            <input type="number" step="0.001" min="0.001" max="0.1" defaultValue="0.001" />
          </div>
          <div className="parameter">
            <label>Batch Size:</label>
            <input type="number" step="8" min="8" max="256" defaultValue="32" />
          </div>
          <div className="parameter">
            <label>Epochs:</label>
            <input type="number" step="1" min="1" max="100" defaultValue="30" />
          </div>
          <div className="parameter">
            <label>Dropout Rate:</label>
            <input type="number" step="0.05" min="0" max="0.5" defaultValue="0.2" />
          </div>
        </div>
        <button className="train-button">Start Training</button>
      </div>
      
      {/* Market Data Section */}
      <div className="market-data-section">
        <div className="section-header">
          <h2>Training Data Market Context</h2>
          <div className="toggle-switch">
            <label className="switch">
              <input 
                type="checkbox" 
                checked={showMarketData} 
                onChange={() => setShowMarketData(!showMarketData)} 
              />
              <span className="slider round"></span>
            </label>
            <span>Show Market Data</span>
          </div>
        </div>
        
        {showMarketData && (
          <>
            <div className="market-controls">
              <div className="control-group">
                <label htmlFor="crypto-select">Cryptocurrency:</label>
                <select 
                  id="crypto-select"
                  value={selectedCrypto}
                  onChange={(e) => setSelectedCrypto(e.target.value)}
                  className="select-input"
                >
                  {topCryptos.map((crypto, index) => (
                    <option key={index} value={crypto}>{crypto}</option>
                  ))}
                </select>
              </div>
              
              <div className="control-group">
                <label htmlFor="timeframe-select">Timeframe:</label>
                <select
                  id="timeframe-select"
                  value={timeframe}
                  onChange={(e) => setTimeframe(e.target.value)}
                  className="select-input"
                >
                  <option value="1d">1 Day</option>
                  <option value="4h">4 Hours</option>
                  <option value="1h">1 Hour</option>
                  <option value="15m">15 Minutes</option>
                </select>
              </div>
              
              <div className="control-group">
                <label htmlFor="source-select">Data Source:</label>
                <select
                  id="source-select"
                  value={dataSource}
                  onChange={(e) => setDataSource(e.target.value)}
                  className="select-input"
                >
                  <option value="binance">Binance</option>
                  <option value="coingecko">CoinGecko</option>
                </select>
              </div>
            </div>
            
            {loadingMarket && <p className="loading-message">Loading market data...</p>}
            {marketError && <div className="error-message">{marketError}</div>}
            
            {!loadingMarket && !marketError && marketData.length > 0 && (
              <div className="market-charts">
                <div className="chart-wrapper">
                  <h3>Price Chart</h3>
                  <ResponsiveContainer width="100%" height={300}>
                    <LineChart data={marketData}>
                      <CartesianGrid strokeDasharray="3 3" />
                      <XAxis dataKey="date" />
                      <YAxis domain={['auto', 'auto']} />
                      <Tooltip />
                      <Legend />
                      <Line 
                        type="monotone" 
                        dataKey="close" 
                        stroke="#ff7300" 
                        dot={false} 
                        name="Close Price" 
                      />
                    </LineChart>
                  </ResponsiveContainer>
                </div>
                
                <div className="chart-wrapper">
                  <h3>Volume</h3>
                  <ResponsiveContainer width="100%" height={150}>
                    <BarChart data={marketData}>
                      <CartesianGrid strokeDasharray="3 3" />
                      <XAxis dataKey="date" />
                      <YAxis />
                      <Tooltip />
                      <Bar dataKey="volume" fill="#8884d8" name="Volume" />
                    </BarChart>
                  </ResponsiveContainer>
                </div>
                
                <div className="market-analysis">
                  <h3>Market Analysis</h3>
                  <p>
                    This section shows the market conditions during which your model is being trained.
                    Understanding the market context can help evaluate the model's performance in different
                    market environments.
                  </p>
                  
                  {/* Market statistics */}
                  <div className="market-stats">
                    <div className="stat-item">
                      <span className="stat-label">Average Price:</span>
                      <span className="stat-value">
                        {formatCurrency(marketData.reduce((sum, item) => sum + item.close, 0) / marketData.length)}
                      </span>
                    </div>
                    <div className="stat-item">
                      <span className="stat-label">Volatility:</span>
                      <span className="stat-value">
                        {(calculateVolatility(marketData.map(d => d.close)) * 100).toFixed(2)}%
                      </span>
                    </div>
                    <div className="stat-item">
                      <span className="stat-label">Price Range:</span>
                      <span className="stat-value">
                        {formatCurrency(Math.min(...marketData.map(d => d.low)))} - {formatCurrency(Math.max(...marketData.map(d => d.high)))}
                      </span>
                    </div>
                  </div>
                </div>
              </div>
            )}
          </>
        )}
      </div>
    </div>
  );
};

// Helper function to calculate volatility
function calculateVolatility(prices) {
  if (!prices || prices.length < 2) return 0;
  
  const returns = [];
  for (let i = 1; i < prices.length; i++) {
    returns.push((prices[i] - prices[i-1]) / prices[i-1]);
  }
  
  const mean = returns.reduce((sum, value) => sum + value, 0) / returns.length;
  const squaredDiffs = returns.map(value => Math.pow(value - mean, 2));
  const variance = squaredDiffs.reduce((sum, value) => sum + value, 0) / returns.length;
  
  return Math.sqrt(variance);
}

export default ModelTraining;