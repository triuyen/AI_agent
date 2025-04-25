import React, { useState, useEffect, useCallback } from 'react';

import {
  LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer,
  Bar, BarChart, ComposedChart, Scatter
} from 'recharts';
import { 
  Card, CardContent, CardHeader, 
  Grid, MenuItem, Select, Button, 
  Tab, Tabs, Box, Typography,
  Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Paper,
  FormControl, InputLabel, Chip, CircularProgress, Alert, Switch,
  FormControlLabel
} from '@mui/material';
import {  
  ArrowUpward, ArrowDownward, RemoveCircleOutline 
} from '@mui/icons-material';
import api from '../services/api';

// Token colors for consistency across charts
const CRYPTO_COLORS = {
  bitcoin: '#F7931A',
  ethereum: '#627EEA',
  binancecoin: '#F3BA2F',
  cardano: '#0033AD',
  dogecoin: '#C2A633',
  ripple: '#23292F',
  solana: '#00FFA3',
  tether: '#26A17B',
  tron: '#EF0027',
  'usd-coin': '#2775CA',
  'BTCUSDT': '#F7931A',
  'ETHUSDT': '#627EEA',
  'BNBUSDT': '#F3BA2F'
};

// Strategy colors
const STRATEGY_COLORS = {
  RF: '#4CAF50',
  MA_CROSSOVER: '#2196F3',
  MEAN_REVERSION: '#9C27B0',
  COMBINED: '#FF5722'
};

// Format currency values
const formatCurrency = (value) => {
  return new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
    minimumFractionDigits: 2,
    maximumFractionDigits: 2
  }).format(value);
};

// Format percentage values
const formatPercent = (value) => {
  return new Intl.NumberFormat('en-US', {
    style: 'percent',
    minimumFractionDigits: 2,
    maximumFractionDigits: 2
  }).format(value / 100);
};

// Format dates
const formatDate = (dateString) => {
  const date = new Date(dateString);
  return date.toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric'
  });
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

// TabPanel component for Material UI Tabs
function TabPanel(props) {
  const { children, value, index, ...other } = props;

  return (
    <div
      role="tabpanel"
      hidden={value !== index}
      id={`tabpanel-${index}`}
      aria-labelledby={`tab-${index}`}
      {...other}
    >
      {value === index && (
        <Box sx={{ pt: 3 }}>
          {children}
        </Box>
      )}
    </div>
  );
}

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

  // Trading Strategy State
  const [selectedStrategy, setSelectedStrategy] = useState('COMBINED');
  const [strategies] = useState(['RF', 'MA_CROSSOVER', 'MEAN_REVERSION', 'COMBINED']);
  const [tradingSignals, setTradingSignals] = useState([]);
  const [backtestResults, setBacktestResults] = useState(null);
  const [timeRange, setTimeRange] = useState('30D');
  const [currentTab, setCurrentTab] = useState(0);
  const [loadingStrategy, setLoadingStrategy] = useState(false);
  const [strategyError, setStrategyError] = useState(null);

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

  
  // Generate sample market data
  const generateSampleMarketData = useCallback(() => {
    const data = [];
    let price = 30000; // Starting price for BTC-like asset
    
    if (selectedCrypto === 'ETHUSDT') price = 2000;
    if (selectedCrypto === 'BNBUSDT') price = 300;
    
    const now = new Date();
    
    for (let i = 99; i >= 0; i--) {
      const date = new Date(now);
      date.setDate(now.getDate() - i);
      
      // Simulate some price movement
      const change = price * (Math.random() * 0.06 - 0.03);
      const open = price;
      const close = price + change;
      const high = Math.max(open, close) + Math.random() * Math.abs(change) * 0.5;
      const low = Math.min(open, close) - Math.random() * Math.abs(change) * 0.5;
      
      // Update price for next iteration
      price = close;
      
      data.push({
        date: date.toLocaleDateString(),
        timestamp: date.getTime(),
        open,
        high,
        low,
        close,
        volume: Math.random() * 10000 + 5000,
        change: close - open,
        changePercent: ((close - open) / open * 100).toFixed(2)
      });
    }
    
    return data;
  }, [selectedCrypto]); // Add selectedCrypto as a dependency
  
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
        
        // Use sample data for development
        const sampleData = generateSampleMarketData();
        setMarketData(sampleData);
      } finally {
        setLoadingMarket(false);
      }
    };
    
    fetchMarketData();
  }, [selectedCrypto, timeframe, dataSource, showMarketData, generateSampleMarketData ]);

  // Fetch trading signals
  useEffect(() => {
    if (selectedCrypto) {
      setLoadingStrategy(true);
      setStrategyError(null);
      
      const fetchSignals = async () => {
        try {
          // Convert from 'BTCUSDT' format to 'bitcoin' format if needed
          const cryptoId = selectedCrypto.replace('USDT', '').toLowerCase();
          
          // In a real app, this would be a real API call
          // const response = await fetch(`/api/TradingStrategy/signals/${cryptoId}?strategy=${selectedStrategy}`);
          // const data = await response.json();
          
          // For now, generate sample data
          const data = generateSampleTradingSignals(cryptoId, selectedStrategy);
          setTradingSignals(data);
        } catch (err) {
          setStrategyError(`Error fetching trading signals: ${err.message}`);
          setTradingSignals([]);
        } finally {
          setLoadingStrategy(false);
        }
      };
      
      fetchSignals();
    }
  }, [selectedCrypto, selectedStrategy]);

  // Run backtest when crypto, strategy, or time range changes
  useEffect(() => {
    if (selectedCrypto && selectedStrategy) {
      setLoadingStrategy(true);
      setStrategyError(null);
      
      const fetchBacktest = async () => {
        try {
          // In a real app, this would be a real API call
          // const response = await fetch(`/api/TradingStrategy/backtest/${cryptoId}?strategy=${selectedStrategy}`);
          // const data = await response.json();
          
          // For now, generate sample data
          const cryptoId = selectedCrypto.replace('USDT', '').toLowerCase();
          const data = generateSampleBacktestResults(cryptoId, selectedStrategy, timeRange);
          setBacktestResults(data);
        } catch (err) {
          setStrategyError(`Error running backtest: ${err.message}`);
          setBacktestResults(null);
        } finally {
          setLoadingStrategy(false);
        }
      };
      
      fetchBacktest();
    }
  }, [selectedCrypto, selectedStrategy, timeRange]);

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

  // Generate sample trading signals
  const generateSampleTradingSignals = (cryptoId, strategy) => {
    const signals = [];
    const now = new Date();
    const price = cryptoId.includes('bitcoin') ? 30000 : 
                 cryptoId.includes('ethereum') ? 2000 : 300;
    
    for (let i = 30; i >= 0; i--) {
      const date = new Date(now);
      date.setDate(now.getDate() - i);
      
      // Create random signal
      const signalTypes = ['BUY', 'SELL', 'HOLD'];
      const signal = signalTypes[Math.floor(Math.random() * signalTypes.length)];
      
      // Create price change
      const priceChange = Math.random() * 10 - 5; // -5% to +5%
      const currentPrice = price * (1 + (Math.random() * 0.2 - 0.1)); // Vary price by ±10%
      const predictedPrice = currentPrice * (1 + priceChange / 100);
      
      signals.push({
        cryptoId,
        timestamp: date.toISOString(),
        currentPrice,
        predictedPrice,
        predictedPriceChangePercent: priceChange,
        signal,
        strategy,
        strategyParameters: {
          parameter1: Math.random() * 5,
          parameter2: Math.random() * 10
        },
        indicatorValues: {
          RSI: 30 + Math.random() * 40,
          ShortMA: currentPrice * 0.95,
          LongMA: currentPrice * 0.9,
          UpperBB: currentPrice * 1.05,
          LowerBB: currentPrice * 0.95
        }
      });
    }
    
    return signals;
  };

  // Generate sample backtest results
  const generateSampleBacktestResults = (cryptoId, strategy, timeRange) => {
    // Determine date range based on timeRange
    const endDate = new Date();
    let startDate = new Date();
    
    switch (timeRange) {
      case '7D':
        startDate.setDate(endDate.getDate() - 7);
        break;
      case '30D':
        startDate.setDate(endDate.getDate() - 30);
        break;
      case '90D':
        startDate.setDate(endDate.getDate() - 90);
        break;
      case '1Y':
        startDate.setFullYear(endDate.getFullYear() - 1);
        break;
      default:
        startDate.setDate(endDate.getDate() - 30);
    }
    
    // Generate sample trade data
    const trades = [];
    const initialBalance = 10000;
    let balance = initialBalance;
    let holdings = 0;
    const price = cryptoId.includes('bitcoin') ? 30000 : 
                 cryptoId.includes('ethereum') ? 2000 : 300;
    
    // Create some sample trades
    const numTrades = 5 + Math.floor(Math.random() * 10);
    const daySpacing = Math.floor((endDate - startDate) / (numTrades * 2));
    
    for (let i = 0; i < numTrades * 2; i++) {
      const tradeDate = new Date(startDate.getTime() + daySpacing * i);
      const tradePrice = price * (1 + (Math.random() * 0.4 - 0.2)); // Vary price by ±20%
      
      if (i % 2 === 0) {
        // Buy trade
        const amount = balance / tradePrice;
        holdings += amount;
        balance = 0;
        
        trades.push({
          type: 'BUY',
          date: tradeDate.toISOString(),
          price: tradePrice,
          amount,
          balance,
          holdings,
          value: holdings * tradePrice
        });
      } else {
        // Sell trade
        balance += holdings * tradePrice;
        const value = balance;
        holdings = 0;
        
        trades.push({
          type: 'SELL',
          date: tradeDate.toISOString(),
          price: tradePrice,
          amount: 0,
          balance,
          holdings,
          value
        });
      }
    }
    
    // Calculate final values
    const finalPrice = price * (1 + (Math.random() * 0.4 - 0.2));
    const finalValue = balance + (holdings * finalPrice);
    const returnPct = (finalValue - initialBalance) / initialBalance * 100;
    
    // Calculate benchmark buy-and-hold return
    const firstPrice = price * (1 - Math.random() * 0.1);
    const buyHoldShares = initialBalance / firstPrice;
    const buyHoldValue = buyHoldShares * finalPrice;
    const buyHoldReturn = (buyHoldValue - initialBalance) / initialBalance * 100;
    
    return {
      strategy,
      initialBalance,
      finalBalance: balance,
      finalHoldings: holdings,
      finalValue,
      return: returnPct,
      buyHoldReturn,
      trades,
      periodStart: startDate.toISOString(),
      periodEnd: endDate.toISOString()
    };
  };

  // Handle crypto selection change
  const handleCryptoChange = (event) => {
    setSelectedCrypto(event.target.value);
  };

  // Handle strategy selection change
  const handleStrategyChange = (event) => {
    setSelectedStrategy(event.target.value);
  };

  // Handle model selection change
  const handleModelChange = (event) => {
    setSelectedModel(event.target.value);
  };

  // Handle time range change
  const handleTimeRangeChange = (range) => {
    setTimeRange(range);
  };

  // Handle tab change
  const handleTabChange = (event, newValue) => {
    setCurrentTab(newValue);
  };

  // Filter signals for price chart data
  const getPriceChartData = () => {
    if (!tradingSignals.length) return [];
    
    const filteredSignals = [...tradingSignals]
      .sort((a, b) => new Date(a.timestamp) - new Date(b.timestamp));
    
    // Map to chart data format
    return filteredSignals.map(signal => ({
      date: new Date(signal.timestamp),
      price: signal.currentPrice,
      predicted: signal.predictedPrice > 0 ? signal.predictedPrice : undefined,
      signal: signal.signal,
      sma10: signal.indicatorValues?.ShortMA,
      sma30: signal.indicatorValues?.LongMA
    }));
  };

  // Get signals for display in the signals table
  const getSignalsTableData = () => {
    return [...tradingSignals]
      .sort((a, b) => new Date(b.timestamp) - new Date(a.timestamp))
      .slice(0, 20); // Show last 20 signals
  };

  // Render signal icon based on signal type
  const renderSignalIcon = (signal) => {
    switch (signal) {
      case 'BUY':
        return <ArrowUpward style={{ color: 'green' }} />;
      case 'SELL':
        return <ArrowDownward style={{ color: 'red' }} />;
      default:
        return <RemoveCircleOutline style={{ color: 'grey' }} />;
    }
  };

  // Render backtest results
  const renderBacktestResults = () => {
    if (!backtestResults) return null;
    
    return (
      <Card>
        <CardHeader 
          title="Backtest Results" 
          subheader={`${formatDate(backtestResults.periodStart)} to ${formatDate(backtestResults.periodEnd)}`} 
        />
        <CardContent>
          <Grid container spacing={2}>
            <Grid item xs={12} md={6}>
              <Typography variant="h6">Performance Summary</Typography>
              <TableContainer component={Paper}>
                <Table size="small">
                  <TableBody>
                    <TableRow>
                      <TableCell>Initial Investment</TableCell>
                      <TableCell align="right">{formatCurrency(backtestResults.initialBalance)}</TableCell>
                    </TableRow>
                    <TableRow>
                      <TableCell>Final Value</TableCell>
                      <TableCell align="right">{formatCurrency(backtestResults.finalValue)}</TableCell>
                    </TableRow>
                    <TableRow>
                      <TableCell>Return</TableCell>
                      <TableCell align="right" style={{ color: backtestResults.return >= 0 ? 'green' : 'red' }}>
                        {formatPercent(backtestResults.return)}
                      </TableCell>
                    </TableRow>
                    <TableRow>
                      <TableCell>Buy & Hold Return</TableCell>
                      <TableCell align="right" style={{ color: backtestResults.buyHoldReturn >= 0 ? 'green' : 'red' }}>
                        {formatPercent(backtestResults.buyHoldReturn)}
                      </TableCell>
                    </TableRow>
                    <TableRow>
                      <TableCell>Outperformance</TableCell>
                      <TableCell align="right" style={{ color: (backtestResults.return - backtestResults.buyHoldReturn) >= 0 ? 'green' : 'red' }}>
                        {formatPercent(backtestResults.return - backtestResults.buyHoldReturn)}
                      </TableCell>
                    </TableRow>
                    <TableRow>
                      <TableCell>Number of Trades</TableCell>
                      <TableCell align="right">{backtestResults.trades.length}</TableCell>
                    </TableRow>
                  </TableBody>
                </Table>
              </TableContainer>
            </Grid>

            <Grid item xs={12} md={6}>
              <Typography variant="h6">Performance Chart</Typography>
              <ResponsiveContainer width="100%" height={250}>
                <LineChart
                  data={backtestResults.trades.map((trade, index) => ({
                    date: new Date(trade.date),
                    portfolioValue: trade.value,
                    trade: trade.type
                  }))}
                  margin={{ top: 5, right: 20, bottom: 5, left: 20 }}
                >
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis 
                    dataKey="date" 
                    scale="time" 
                    type="number"
                    domain={['dataMin', 'dataMax']}
                    tickFormatter={(timestamp) => new Date(timestamp).toLocaleDateString()}
                  />
                  <YAxis />
                  <Tooltip 
                    labelFormatter={(timestamp) => new Date(timestamp).toLocaleDateString()}
                    formatter={(value) => [formatCurrency(value), 'Portfolio Value']}
                  />
                  <Legend />
                  <Line 
                    type="monotone" 
                    dataKey="portfolioValue" 
                    stroke={STRATEGY_COLORS[selectedStrategy]} 
                    dot={false}
                    name="Portfolio Value"
                  />
                  <Scatter 
                    dataKey="portfolioValue"
                    fill="red"
                    name="Trades"
                    shape={(props) => {
                      const { cx, cy, payload } = props;
                      return payload.trade === 'BUY' ? (
                        <polygon 
                          points={`${cx},${cy-8} ${cx+6},${cy+4} ${cx-6},${cy+4}`} 
                          fill="green" 
                          stroke="none" 
                        />
                      ) : (
                        <polygon 
                          points={`${cx},${cy+8} ${cx+6},${cy-4} ${cx-6},${cy-4}`} 
                          fill="red" 
                          stroke="none" 
                        />
                      );
                    }}
                  />
                </LineChart>
              </ResponsiveContainer>
            </Grid>

            <Grid item xs={12}>
              <Typography variant="h6">Trade History</Typography>
              <TableContainer component={Paper} style={{ maxHeight: 300 }}>
                <Table stickyHeader size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell>Date</TableCell>
                      <TableCell>Type</TableCell>
                      <TableCell align="right">Price</TableCell>
                      <TableCell align="right">Amount</TableCell>
                      <TableCell align="right">Value</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {backtestResults.trades.map((trade, index) => (
                      <TableRow key={index} style={{ backgroundColor: trade.type === 'BUY' ? 'rgba(0, 128, 0, 0.05)' : 'rgba(255, 0, 0, 0.05)' }}>
                        <TableCell>{formatDate(trade.date)}</TableCell>
                        <TableCell>
                          <Chip 
                            label={trade.type} 
                            color={trade.type === 'BUY' ? 'success' : 'error'} 
                            size="small" 
                            icon={trade.type === 'BUY' ? <ArrowUpward /> : <ArrowDownward />} 
                          />
                        </TableCell>
                        <TableCell align="right">{formatCurrency(trade.price)}</TableCell>
                        <TableCell align="right">{trade.amount.toFixed(6)}</TableCell>
                        <TableCell align="right">{formatCurrency(trade.value)}</TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            </Grid>
          </Grid>
        </CardContent>
      </Card>
    );
  };

  // Main render function
  return (
    <div className="model-training-page" style={{ padding: '20px' }}>
      <Typography variant="h4" gutterBottom>AI-Agent Crypto Trading Platform</Typography>
      
      <Box sx={{ borderBottom: 1, borderColor: 'divider', mb: 3 }}>
        <Tabs value={currentTab} onChange={handleTabChange} aria-label="trading dashboard tabs">
          <Tab label="Model Training" />
          <Tab label="Trading Strategies" />
          <Tab label="Performance Analysis" />
          <Tab label="Market Data" />
        </Tabs>
      </Box>
      
      {/* Model Training Tab */}
      <TabPanel value={currentTab} index={0}>
        <div className="model-selector">
          <FormControl sx={{ minWidth: 200, mb: 3 }}>
            <InputLabel id="model-select-label">Select Model</InputLabel>
            <Select
              labelId="model-select-label"
              id="model-select" 
              value={selectedModel} 
              label="Select Model"
              onChange={handleModelChange}
            >
              <MenuItem value="tcn">Temporal Convolutional Network</MenuItem>
              <MenuItem value="lstm">LSTM Network</MenuItem>
              <MenuItem value="transformer">Transformer Model</MenuItem>
              <MenuItem value="rf">Random Forest</MenuItem>
            </Select>
          </FormControl>
        </div>

        {loading ? <CircularProgress /> : null}
        {error ? <Alert severity="error">{error}</Alert> : null}
        
        {!loading && !error && (
          <div className="charts-container">
            <Grid container spacing={3}>
              <Grid item xs={12} md={6}>
                <Card>
                  <CardHeader title="Training & Validation Loss" />
                  <CardContent>
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
                  </CardContent>
                </Card>
              </Grid>
              
              <Grid item xs={12} md={6}>
                <Card>
                  <CardHeader title="Training & Validation Accuracy" />
                  <CardContent>
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
                  </CardContent>
                </Card>
              </Grid>
              
              <Grid item xs={12}>
                <Card>
                  <CardHeader title="Model Parameters" />
                  <CardContent>
                    <Grid container spacing={2}>
                      <Grid item xs={6} md={3}>
                        <FormControl fullWidth variant="outlined" size="small">
                          <InputLabel>Learning Rate</InputLabel>
                          <Select
                            value="0.001"
                            label="Learning Rate"
                          >
                            <MenuItem value="0.0001">0.0001</MenuItem>
                            <MenuItem value="0.001">0.001</MenuItem>
                            <MenuItem value="0.01">0.01</MenuItem>
                            <MenuItem value="0.1">0.1</MenuItem>
                          </Select>
                        </FormControl>
                      </Grid>
                      <Grid item xs={6} md={3}>
                        <FormControl fullWidth variant="outlined" size="small">
                          <InputLabel>Batch Size</InputLabel>
                          <Select
                            value="32"
                            label="Batch Size"
                          >
                            <MenuItem value="8">8</MenuItem>
                            <MenuItem value="16">16</MenuItem>
                            <MenuItem value="32">32</MenuItem>
                            <MenuItem value="64">64</MenuItem>
                            <MenuItem value="128">128</MenuItem>
                          </Select>
                        </FormControl>
                      </Grid>
                      <Grid item xs={6} md={3}>
                        <FormControl fullWidth variant="outlined" size="small">
                          <InputLabel>Epochs</InputLabel>
                          <Select
                            value="30"
                            label="Epochs"
                          >
                            <MenuItem value="10">10</MenuItem>
                            <MenuItem value="30">30</MenuItem>
                            <MenuItem value="50">50</MenuItem>
                            <MenuItem value="100">100</MenuItem>
                          </Select>
                        </FormControl>
                      </Grid>
                      <Grid item xs={6} md={3}>
                        <FormControl fullWidth variant="outlined" size="small">
                          <InputLabel>Dropout Rate</InputLabel>
                          <Select
                            value="0.2"
                            label="Dropout Rate"
                          >
                            <MenuItem value="0">0.0</MenuItem>
                            <MenuItem value="0.1">0.1</MenuItem>
                            <MenuItem value="0.2">0.2</MenuItem>
                            <MenuItem value="0.3">0.3</MenuItem>
                            <MenuItem value="0.5">0.5</MenuItem>
                          </Select>
                        </FormControl>
                      </Grid>
                    </Grid>
                    <Box sx={{ mt: 2 }}>
                      <Button variant="contained" color="primary">Start Training</Button>
                    </Box>
                  </CardContent>
                </Card>
              </Grid>
            </Grid>
          </div>
        )}
      </TabPanel>
      
      {/* Trading Strategies Tab */}
      <TabPanel value={currentTab} index={1}>
        <Grid container spacing={3}>
          <Grid item xs={12}>
            <Card>
              <CardContent>
                <Grid container spacing={2} alignItems="center">
                  <Grid item xs={12} md={4}>
                    <FormControl fullWidth>
                      <InputLabel id="crypto-select-label">Cryptocurrency</InputLabel>
                      <Select
                        labelId="crypto-select-label"
                        value={selectedCrypto}
                        label="Cryptocurrency"
                        onChange={handleCryptoChange}
                      >
                        {topCryptos.map((crypto) => (
                          <MenuItem key={crypto} value={crypto}>{crypto}</MenuItem>
                        ))}
                      </Select>
                    </FormControl>
                  </Grid>
                  <Grid item xs={12} md={4}>
                    <FormControl fullWidth>
                      <InputLabel id="strategy-select-label">Trading Strategy</InputLabel>
                      <Select
                        labelId="strategy-select-label"
                        value={selectedStrategy}
                        label="Trading Strategy"
                        onChange={handleStrategyChange}
                      >
                        {strategies.map((strategy) => (
                          <MenuItem key={strategy} value={strategy}>
                            {strategy === 'RF' ? 'Random Forest' :
                             strategy === 'MA_CROSSOVER' ? 'Moving Average Crossover' :
                             strategy === 'MEAN_REVERSION' ? 'Mean Reversion' : 'Combined Strategy'}
                          </MenuItem>
                        ))}
                      </Select>
                    </FormControl>
                  </Grid>
                  <Grid item xs={12} md={4}>
                    <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
                      {['7D', '30D', '90D', '1Y', 'ALL'].map((range) => (
                        <Button
                          key={range}
                          variant={timeRange === range ? 'contained' : 'outlined'}
                          color="primary"
                          size="small"
                          onClick={() => handleTimeRangeChange(range)}
                        >
                          {range}
                        </Button>
                      ))}
                    </Box>
                  </Grid>
                </Grid>
              </CardContent>
            </Card>
          </Grid>
          
          {loadingStrategy && (
            <Grid item xs={12} sx={{ display: 'flex', justifyContent: 'center', p: 3 }}>
              <CircularProgress />
            </Grid>
          )}
          
          {strategyError && (
            <Grid item xs={12}>
              <Alert severity="error">{strategyError}</Alert>
            </Grid>
          )}
          
          {!loadingStrategy && !strategyError && (
            <>
              <Grid item xs={12}>
                <Card>
                  <CardHeader title={`Price Chart & Signals: ${selectedCrypto}`} />
                  <CardContent>
                    <ResponsiveContainer width="100%" height={400}>
                      <ComposedChart data={getPriceChartData()}>
                        <CartesianGrid strokeDasharray="3 3" />
                        <XAxis 
                          dataKey="date" 
                          scale="time" 
                          type="number"
                          domain={['dataMin', 'dataMax']}
                          tickFormatter={(timestamp) => new Date(timestamp).toLocaleDateString()}
                        />
                        <YAxis 
                          yAxisId="price"
                          domain={['auto', 'auto']} 
                          label={{ value: 'Price (USD)', angle: -90, position: 'insideLeft' }}
                        />
                        <Tooltip 
                          labelFormatter={(timestamp) => new Date(timestamp).toLocaleDateString()}
                          formatter={(value, name) => [formatCurrency(value), name]}
                        />
                        <Legend />
                        <Line 
                          yAxisId="price"
                          type="monotone" 
                          dataKey="price" 
                          stroke={CRYPTO_COLORS[selectedCrypto.replace('USDT', '').toLowerCase()] || '#000'} 
                          name="Current Price" 
                          dot={false}
                        />
                        <Line 
                          yAxisId="price"
                          type="monotone" 
                          dataKey="predicted" 
                          stroke="#ff7300" 
                          strokeDasharray="5 5"
                          name="Predicted Price" 
                          dot={false}
                        />
                        <Line 
                          yAxisId="price"
                          type="monotone" 
                          dataKey="sma10" 
                          stroke="#8884d8" 
                          strokeDasharray="3 3"
                          name="SMA (10)" 
                          dot={false}
                        />
                        <Line 
                          yAxisId="price"
                          type="monotone" 
                          dataKey="sma30" 
                          stroke="#82ca9d" 
                          strokeDasharray="3 3"
                          name="SMA (30)" 
                          dot={false}
                        />
                        <Scatter 
                          yAxisId="price"
                          dataKey="price"
                          data={getPriceChartData().filter(point => point.signal === 'BUY')}
                          name="Buy Signal"
                          fill="green"
                          shape="triangle"
                        />
                        <Scatter 
                          yAxisId="price"
                          dataKey="price"
                          data={getPriceChartData().filter(point => point.signal === 'SELL')}
                          name="Sell Signal"
                          fill="red"
                          shape="triangle"
                        />
                      </ComposedChart>
                    </ResponsiveContainer>
                  </CardContent>
                </Card>
              </Grid>
              
              <Grid item xs={12} md={6}>
                <Card sx={{ height: '100%' }}>
                  <CardHeader title="Recent Signals" />
                  <CardContent>
                    <TableContainer sx={{ maxHeight: 400 }}>
                      <Table stickyHeader size="small">
                        <TableHead>
                          <TableRow>
                            <TableCell>Date</TableCell>
                            <TableCell>Signal</TableCell>
                            <TableCell align="right">Price</TableCell>
                            <TableCell align="right">Predicted</TableCell>
                            <TableCell align="right">Change %</TableCell>
                          </TableRow>
                        </TableHead>
                        <TableBody>
                          {getSignalsTableData().map((signal, index) => (
                            <TableRow key={index}>
                              <TableCell>{formatDate(signal.timestamp)}</TableCell>
                              <TableCell>
                                <Box sx={{ display: 'flex', alignItems: 'center' }}>
                                  {renderSignalIcon(signal.signal)}
                                  <span style={{ marginLeft: 4 }}>{signal.signal}</span>
                                </Box>
                              </TableCell>
                              <TableCell align="right">{formatCurrency(signal.currentPrice)}</TableCell>
                              <TableCell align="right">{formatCurrency(signal.predictedPrice)}</TableCell>
                              <TableCell align="right" style={{ 
                                color: signal.predictedPriceChangePercent > 0 ? 'green' : 
                                       signal.predictedPriceChangePercent < 0 ? 'red' : 'inherit' 
                              }}>
                                {signal.predictedPriceChangePercent > 0 ? '+' : ''}
                                {signal.predictedPriceChangePercent.toFixed(2)}%
                              </TableCell>
                            </TableRow>
                          ))}
                        </TableBody>
                      </Table>
                    </TableContainer>
                  </CardContent>
                </Card>
              </Grid>
              
              <Grid item xs={12} md={6}>
                <Card sx={{ height: '100%' }}>
                  <CardHeader title="Technical Indicators" />
                  <CardContent>
                    <TableContainer>
                      <Table size="small">
                        <TableHead>
                          <TableRow>
                            <TableCell>Indicator</TableCell>
                            <TableCell align="right">Value</TableCell>
                            <TableCell>Signal</TableCell>
                          </TableRow>
                        </TableHead>
                        <TableBody>
                          {tradingSignals.length > 0 && (
                            <>
                              <TableRow>
                                <TableCell>RSI (14)</TableCell>
                                <TableCell align="right">
                                  {tradingSignals[0].indicatorValues.RSI.toFixed(2)}
                                </TableCell>
                                <TableCell>
                                  {tradingSignals[0].indicatorValues.RSI > 70 ? (
                                    <Chip size="small" label="Overbought" color="error" />
                                  ) : tradingSignals[0].indicatorValues.RSI < 30 ? (
                                    <Chip size="small" label="Oversold" color="success" />
                                  ) : (
                                    <Chip size="small" label="Neutral" color="default" />
                                  )}
                                </TableCell>
                              </TableRow>
                              <TableRow>
                                <TableCell>SMA Crossover</TableCell>
                                <TableCell align="right">
                                  {tradingSignals[0].indicatorValues.ShortMA.toFixed(2)} / {tradingSignals[0].indicatorValues.LongMA.toFixed(2)}
                                </TableCell>
                                <TableCell>
                                  {tradingSignals[0].indicatorValues.ShortMA > tradingSignals[0].indicatorValues.LongMA ? (
                                    <Chip size="small" label="Bullish" color="success" />
                                  ) : (
                                    <Chip size="small" label="Bearish" color="error" />
                                  )}
                                </TableCell>
                              </TableRow>
                              <TableRow>
                                <TableCell>Bollinger Bands</TableCell>
                                <TableCell align="right">
                                  {tradingSignals[0].indicatorValues.LowerBB.toFixed(2)} / {tradingSignals[0].indicatorValues.UpperBB.toFixed(2)}
                                </TableCell>
                                <TableCell>
                                  {tradingSignals[0].currentPrice >= tradingSignals[0].indicatorValues.UpperBB ? (
                                    <Chip size="small" label="Overbought" color="error" />
                                  ) : tradingSignals[0].currentPrice <= tradingSignals[0].indicatorValues.LowerBB ? (
                                    <Chip size="small" label="Oversold" color="success" />
                                  ) : (
                                    <Chip size="small" label="Inside Bands" color="default" />
                                  )}
                                </TableCell>
                              </TableRow>
                              <TableRow>
                                <TableCell>Random Forest</TableCell>
                                <TableCell align="right">
                                  {tradingSignals[0].predictedPriceChangePercent > 0 ? '+' : ''}
                                  {tradingSignals[0].predictedPriceChangePercent.toFixed(2)}%
                                </TableCell>
                                <TableCell>
                                  {tradingSignals[0].predictedPriceChangePercent > 1.5 ? (
                                    <Chip size="small" label="Strong Buy" color="success" />
                                  ) : tradingSignals[0].predictedPriceChangePercent > 0 ? (
                                    <Chip size="small" label="Buy" color="success" />
                                  ) : tradingSignals[0].predictedPriceChangePercent < -1.5 ? (
                                    <Chip size="small" label="Strong Sell" color="error" />
                                  ) : tradingSignals[0].predictedPriceChangePercent < 0 ? (
                                    <Chip size="small" label="Sell" color="error" />
                                  ) : (
                                    <Chip size="small" label="Hold" color="default" />
                                  )}
                                </TableCell>
                              </TableRow>
                            </>
                          )}
                        </TableBody>
                      </Table>
                    </TableContainer>
                  </CardContent>
                </Card>
              </Grid>
            </>
          )}
        </Grid>
      </TabPanel>
      
      {/* Performance Analysis Tab */}
      <TabPanel value={currentTab} index={2}>
        <Grid container spacing={3}>
          <Grid item xs={12}>
            <Card>
              <CardContent>
                <Grid container spacing={2} alignItems="center">
                  <Grid item xs={12} md={4}>
                    <FormControl fullWidth>
                      <InputLabel id="crypto-backtest-label">Cryptocurrency</InputLabel>
                      <Select
                        labelId="crypto-backtest-label"
                        value={selectedCrypto}
                        label="Cryptocurrency"
                        onChange={handleCryptoChange}
                      >
                        {topCryptos.map((crypto) => (
                          <MenuItem key={crypto} value={crypto}>{crypto}</MenuItem>
                        ))}
                      </Select>
                    </FormControl>
                  </Grid>
                  <Grid item xs={12} md={4}>
                    <FormControl fullWidth>
                      <InputLabel id="strategy-backtest-label">Trading Strategy</InputLabel>
                      <Select
                        labelId="strategy-backtest-label"
                        value={selectedStrategy}
                        label="Trading Strategy"
                        onChange={handleStrategyChange}
                      >
                        {strategies.map((strategy) => (
                          <MenuItem key={strategy} value={strategy}>
                            {strategy === 'RF' ? 'Random Forest' :
                             strategy === 'MA_CROSSOVER' ? 'Moving Average Crossover' :
                             strategy === 'MEAN_REVERSION' ? 'Mean Reversion' : 'Combined Strategy'}
                          </MenuItem>
                        ))}
                      </Select>
                    </FormControl>
                  </Grid>
                  <Grid item xs={12} md={4}>
                    <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
                      {['7D', '30D', '90D', '1Y', 'ALL'].map((range) => (
                        <Button
                          key={range}
                          variant={timeRange === range ? 'contained' : 'outlined'}
                          color="primary"
                          size="small"
                          onClick={() => handleTimeRangeChange(range)}
                        >
                          {range}
                        </Button>
                      ))}
                    </Box>
                  </Grid>
                </Grid>
              </CardContent>
            </Card>
          </Grid>
          
          {loadingStrategy && (
            <Grid item xs={12} sx={{ display: 'flex', justifyContent: 'center', p: 3 }}>
              <CircularProgress />
            </Grid>
          )}
          
          {strategyError && (
            <Grid item xs={12}>
              <Alert severity="error">{strategyError}</Alert>
            </Grid>
          )}
          
          {!loadingStrategy && !strategyError && (
            <Grid item xs={12}>
              {renderBacktestResults()}
            </Grid>
          )}
        </Grid>
      </TabPanel>
      
      {/* Market Data Tab */}
      <TabPanel value={currentTab} index={3}>
        <div className="market-data-section">
          <Grid container spacing={3}>
            <Grid item xs={12}>
              <Card>
                <CardContent>
                  <Grid container spacing={2} justifyContent="space-between" alignItems="center">
                    <Grid item>
                      <Typography variant="h6">Market Data</Typography>
                    </Grid>
                    <Grid item>
                      <FormControlLabel
                        control={
                          <Switch
                            checked={showMarketData}
                            onChange={() => setShowMarketData(!showMarketData)}
                            color="primary"
                          />
                        }
                        label="Show Market Data"
                      />
                    </Grid>
                  </Grid>
                </CardContent>
              </Card>
            </Grid>
          
            {showMarketData && (
              <>
                <Grid item xs={12}>
                  <Card>
                    <CardContent>
                      <Grid container spacing={2}>
                        <Grid item xs={12} md={4}>
                          <FormControl fullWidth variant="outlined" size="small">
                            <InputLabel>Cryptocurrency</InputLabel>
                            <Select
                              value={selectedCrypto}
                              label="Cryptocurrency"
                              onChange={handleCryptoChange}
                            >
                              {topCryptos.map((crypto) => (
                                <MenuItem key={crypto} value={crypto}>{crypto}</MenuItem>
                              ))}
                            </Select>
                          </FormControl>
                        </Grid>
                        <Grid item xs={12} md={4}>
                          <FormControl fullWidth variant="outlined" size="small">
                            <InputLabel>Timeframe</InputLabel>
                            <Select
                              value={timeframe}
                              label="Timeframe"
                              onChange={(e) => setTimeframe(e.target.value)}
                            >
                              <MenuItem value="1d">1 Day</MenuItem>
                              <MenuItem value="4h">4 Hours</MenuItem>
                              <MenuItem value="1h">1 Hour</MenuItem>
                              <MenuItem value="15m">15 Minutes</MenuItem>
                            </Select>
                          </FormControl>
                        </Grid>
                        <Grid item xs={12} md={4}>
                          <FormControl fullWidth variant="outlined" size="small">
                            <InputLabel>Data Source</InputLabel>
                            <Select
                              value={dataSource}
                              label="Data Source"
                              onChange={(e) => setDataSource(e.target.value)}
                            >
                              <MenuItem value="binance">Binance</MenuItem>
                              <MenuItem value="coingecko">CoinGecko</MenuItem>
                            </Select>
                          </FormControl>
                        </Grid>
                      </Grid>
                    </CardContent>
                  </Card>
                </Grid>
                
                {loadingMarket && (
                  <Grid item xs={12} sx={{ display: 'flex', justifyContent: 'center', p: 3 }}>
                    <CircularProgress />
                  </Grid>
                )}
                
                {marketError && (
                  <Grid item xs={12}>
                    <Alert severity="error">{marketError}</Alert>
                  </Grid>
                )}
                
                {!loadingMarket && !marketError && marketData.length > 0 && (
                  <>
                    <Grid item xs={12}>
                      <Card>
                        <CardHeader title="Price Chart" />
                        <CardContent>
                          <ResponsiveContainer width="100%" height={300}>
                            <LineChart data={marketData}>
                              <CartesianGrid strokeDasharray="3 3" />
                              <XAxis dataKey="date" />
                              <YAxis domain={['auto', 'auto']} />
                              <Tooltip formatter={(value) => [formatCurrency(value)]} />
                              <Legend />
                              <Line 
                                type="monotone" 
                                dataKey="close" 
                                stroke={CRYPTO_COLORS[selectedCrypto.replace('USDT', '').toLowerCase()] || '#ff7300'} 
                                dot={false} 
                                name="Close Price" 
                              />
                            </LineChart>
                          </ResponsiveContainer>
                        </CardContent>
                      </Card>
                    </Grid>
                    
                    <Grid item xs={12}>
                      <Card>
                        <CardHeader title="Trading Volume" />
                        <CardContent>
                          <ResponsiveContainer width="100%" height={150}>
                            <BarChart data={marketData}>
                              <CartesianGrid strokeDasharray="3 3" />
                              <XAxis dataKey="date" />
                              <YAxis />
                              <Tooltip />
                              <Bar dataKey="volume" fill="#8884d8" name="Volume" />
                            </BarChart>
                          </ResponsiveContainer>
                        </CardContent>
                      </Card>
                    </Grid>
                    
                    <Grid item xs={12}>
                      <Card>
                        <CardHeader title="Market Analysis" />
                        <CardContent>
                          <Typography paragraph>
                            This section shows the market conditions during which your model is being trained.
                            Understanding the market context can help evaluate the model's performance in different
                            market environments.
                          </Typography>
                          
                          <Grid container spacing={2}>
                            <Grid item xs={12} md={4}>
                              <Paper sx={{ p: 2 }}>
                                <Typography variant="subtitle1">Average Price</Typography>
                                <Typography variant="h6">
                                  {formatCurrency(marketData.reduce((sum, item) => sum + item.close, 0) / marketData.length)}
                                </Typography>
                              </Paper>
                            </Grid>
                            <Grid item xs={12} md={4}>
                              <Paper sx={{ p: 2 }}>
                                <Typography variant="subtitle1">Volatility</Typography>
                                <Typography variant="h6">
                                  {(calculateVolatility(marketData.map(d => d.close)) * 100).toFixed(2)}%
                                </Typography>
                              </Paper>
                            </Grid>
                            <Grid item xs={12} md={4}>
                              <Paper sx={{ p: 2 }}>
                                <Typography variant="subtitle1">Price Range</Typography>
                                <Typography variant="h6">
                                  {formatCurrency(Math.min(...marketData.map(d => d.low)))} - {formatCurrency(Math.max(...marketData.map(d => d.high)))}
                                </Typography>
                              </Paper>
                            </Grid>
                          </Grid>
                        </CardContent>
                      </Card>
                    </Grid>
                  </>
                )}
              </>
            )}
          </Grid>
        </div>
      </TabPanel>
    </div>
  );
};

export default ModelTraining;