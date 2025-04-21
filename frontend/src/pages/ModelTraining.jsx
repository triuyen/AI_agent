import React, { useState, useEffect } from 'react';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from 'recharts';
import api from '../services/api';

const ModelTraining = () => {
  const [trainingData, setTrainingData] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [selectedModel, setSelectedModel] = useState('tcn'); // Default model

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
    </div>
  );
};

export default ModelTraining;