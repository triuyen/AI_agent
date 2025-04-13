
import axios from 'axios';

// SWAGGER backend URL
const API_URL = 'http://localhost:5124';

const api = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

export default api;