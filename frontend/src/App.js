import React from "react";
import { BrowserRouter as Router, Routes, Route, Link } from 'react-router-dom';
import ModelTraining from './pages/ModelTraining';
import LiveTrading from "./pages/LiveTrading";
import ApiDocs from "./pages/ApiDocs";
import "./App.css";

function App(){
  return(
    <Router>
      <div className="App">
        <nav>
          <h1>AI agent Trading bot</h1>
          <ul className="nav_links">
           <li><Link to ="/">Model Trainning</Link></li>
           <li><Link to ="/live-trading">Live Trading</Link></li>
           <li><Link to = "/api-docs">API Documentation</Link></li>
          </ul>
        </nav>
        <p>© 2023 Ai gent Trading Bot. All rights reserved.</p>
        <div className="content">
          <Routes>
            <Route path="/" element={<ModelTraining />} />
            <Route path="/live-trading" element={<LiveTrading />} />
            <Route path="/api-docs" element={<ApiDocs />} />
          </Routes> 
        </div>
      </div>
    </Router>
  );
}

export default App;