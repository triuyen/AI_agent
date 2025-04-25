import React, { useState, useEffect } from 'react';
import { 
  AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip, 
  ResponsiveContainer, ReferenceLine 
} from 'recharts';
import api from '../services/api';

const LiveTrading = () => {
  const [candleData, setCandleData] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [selectedSymbol, setSelectedSymbol] = useState('BTCUSDT');
  const [timeframe, setTimeframe] = useState('1h');
  const [signals, setSignals] = useState([]);

  useEffect(() => {
    const fetchCandleData = async () => {
      try {
        setLoading(true);
        // Replace with your actual API endpoint for candle data
        const response = await api.get(`/api/market-data/${selectedSymbol}?timeframe=${timeframe}`);
        setCandleData(response.data);
        
        // Fetch AI signals
        const signalsResponse = await api.get(`/api/signals/${selectedSymbol}?timeframe=${timeframe}`);
        setSignals(signalsResponse.data);
        
        setError(null);
      } catch (err) {
        setError('Failed to load market data: ' + err.message);
        // Use sample data for development
        setCandleData(generateSampleCandleData());
        setSignals(generateSampleSignals());
      } finally {
        setLoading(false);
      }
    };

    fetchCandleData();
    
    // Set up polling for live data
    const intervalId = setInterval(fetchCandleData, 60000); // Refresh every minute
    
    return () => clearInterval(intervalId);
  }, [selectedSymbol, timeframe]);

  // Sample data generators for development
  const generateSampleCandleData = () => {
    const data = [];
    let close = 35000 + Math.random() * 2000;
    
    for (let i = 0; i < 100; i++) {
      const change = (Math.random() - 0.5) * 200;
      close += change;
      
      const high = close + Math.random() * 100;
      const low = close - Math.random() * 100;
      const open = close - change;
      
      const date = new Date();
      date.setHours(date.getHours() - (100 - i));
      
      data.push({
        timestamp: date.toISOString(),
        date: date.toLocaleString(),
        open,
        high,
        low,
        close,
        volume: Math.random() * 1000000
      });
    }
    
    return data;
  };
  
  const generateSampleSignals = () => {
    return [
      { type: 'BUY', timestamp: new Date(Date.now() - 3600000 * 20).toISOString(), price: 35400, confidence: 0.85 },
      { type: 'SELL', timestamp: new Date(Date.now() - 3600000 * 10).toISOString(), price: 36200, confidence: 0.78 },
      { type: 'BUY', timestamp: new Date(Date.now() - 3600000 * 5).toISOString(), price: 35800, confidence: 0.92 }
    ];
  };
  
  // Find signals in the visible data range
  const findSignalIndexes = () => {
    if (!candleData.length || !signals.length) return [];
    
    return signals.map(signal => {
      const index = candleData.findIndex(candle => 
        new Date(candle.timestamp).getTime() >= new Date(signal.timestamp).getTime()
      );
      return { ...signal, index: index >= 0 ? index : null };
    }).filter(signal => signal.index !== null);
  };
  
  const visibleSignals = findSignalIndexes();

  return (
    <div className="live-trading-page">
      <h1>Live Trading Analysis</h1>
      
      <div className="controls">
        <div className="symbol-selector">
          <label htmlFor="symbol-select">Symbol:</label>
          <select 
            id="symbol-select" 
            value={selectedSymbol} 
            onChange={(e) => setSelectedSymbol(e.target.value)}
          >
            <option value="BTCUSDT">Bitcoin (BTC/USDT)</option>
            <option value="ETHUSDT">Ethereum (ETH/USDT)</option>
            <option value="SOLUSDT">Solana (SOL/USDT)</option>
            <option value="BNBUSDT">Binance Coin (BNB/USDT)</option>
          </select>
        </div>
        
        <div className="timeframe-selector">
          <label htmlFor="timeframe-select">Timeframe:</label>
          <select 
            id="timeframe-select" 
            value={timeframe} 
            onChange={(e) => setTimeframe(e.target.value)}
          >
            <option value="1m">1 minute</option>
            <option value="5m">5 minutes</option>
            <option value="15m">15 minutes</option>
            <option value="1h">1 hour</option>
            <option value="4h">4 hours</option>
            <option value="1d">1 day</option>
          </select>
        </div>
      </div>

      {loading && <p>Loading market data...</p>}
      
      {error && <div className="error-message">{error}</div>}
      
      {!loading && !error && (
        <div className="chart-container">
          <h2>{selectedSymbol} Price Chart ({timeframe})</h2>
          <ResponsiveContainer width="100%" height={500}>
            <AreaChart data={candleData}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="date" />
              <YAxis domain={['auto', 'auto']} />
              <Tooltip 
                labelFormatter={(label) => `Time: ${label}`}
                formatter={(value) => [`$${value.toFixed(2)}`, 'Price']}
              />
              <defs>
                <linearGradient id="colorPrice" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="5%" stopColor="#8884d8" stopOpacity={0.8}/>
                  <stop offset="95%" stopColor="#8884d8" stopOpacity={0.1}/>
                </linearGradient>
              </defs>
              <Area 
                type="monotone" 
                dataKey="close" 
                stroke="#8884d8" 
                fillOpacity={1} 
                fill="url(#colorPrice)" 
              />
              
              {/* Map buy signals */}
              {visibleSignals
                .filter(signal => signal.type === 'BUY')
                .map((signal, idx) => (
                  <ReferenceLine 
                    key={`buy-${idx}`} 
                    x={signal.index} 
                    stroke="green" 
                    strokeWidth={2}
                    strokeDasharray="3 3"
                    label={{ value: 'BUY', position: 'insideTopLeft', fill: 'green' }}
                  />
                ))
              }
              
              {/* Map sell signals */}
              {visibleSignals
                .filter(signal => signal.type === 'SELL')
                .map((signal, idx) => (
                  <ReferenceLine 
                    key={`sell-${idx}`} 
                    x={signal.index} 
                    stroke="red" 
                    strokeWidth={2}
                    strokeDasharray="3 3"
                    label={{ value: 'SELL', position: 'insideTopRight', fill: 'red' }}
                  />
                ))
              }
            </AreaChart>
          </ResponsiveContainer>
        </div>
      )}
      
      <div className="signals-panel">
        <h2>AI Trading Signals</h2>
        <div className="signals-list">
          {signals.length === 0 ? (
            <p>No signals detected in the current timeframe</p>
          ) : (
            <table className="signals-table">
              <thead>
                <tr>
                  <th>Type</th>
                  <th>Time</th>
                  <th>Price</th>
                  <th>Confidence</th>
                </tr>
              </thead>
              <tbody>
                {signals.map((signal, idx) => (
                  <tr key={idx} className={signal.type === 'BUY' ? 'buy-signal' : 'sell-signal'}>
                    <td>{signal.type}</td>
                    <td>{new Date(signal.timestamp).toLocaleString()}</td>
                    <td>${signal.price.toFixed(2)}</td>
                    <td>{(signal.confidence * 100).toFixed(1)}%</td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      </div>
    </div>
  );
};

export default LiveTrading;