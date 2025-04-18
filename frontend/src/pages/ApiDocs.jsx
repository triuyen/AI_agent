// import React from 'react';

// const ApiDocs = () => {
//   // Replace with your actual Swagger URL
//   const swaggerUrl = 'http://localhost:5124/swagger/index.html';
  
//   return (
//     <div className="api-docs-page">
//       <h1>API Documentation</h1>
//       <p>This page provides access to the complete API documentation for the AI Agent Crypto Trading system.</p>
      
//       <div className="swagger-container">
//         <iframe 
//           src={swaggerUrl}
//           title="API Documentation"
//           width="100%"
//           height="800px"
//           style={{ border: 'none' }}
//         />
//       </div>
//     </div>
//   );
// };

// export default ApiDocs;


import React, { useState, useEffect } from 'react';

const ApiDocs = () => {
  const [apiStatus, setApiStatus] = useState('checking');
  // Use HTTPS instead of HTTP for secure connection
  const swaggerUrl = 'http://localhost:5124/swagger/index.html';
  
  useEffect(() => {
    // Check if the API is accessible
    const checkApiStatus = async () => {
      try {
        const response = await fetch('http://localhost:5124/swagger/v1/swagger.json', {
          method: 'HEAD'
        });
        setApiStatus(response.ok ? 'online' : 'offline');
      } catch (error) {
        console.error('API connection error:', error);
        setApiStatus('offline');
      }
    };
    
    checkApiStatus();
  }, []);
  
  const openSwaggerDocs = () => {
    window.open(swaggerUrl, '_blank', 'noopener,noreferrer');
  };
  
  return (
    <div className="api-docs-page">
      <h1>API Documentation</h1>
      <p>This page provides access to the complete API documentation for the AI Agent Crypto Trading system.</p>
      
      {apiStatus === 'checking' && (
        <div className="api-status checking">
          Checking API availability...
        </div>
      )}
      
      {apiStatus === 'offline' && (
        <div className="api-status offline">
          <p>⚠️ The API appears to be offline or inaccessible.</p>
          <p>Please ensure that:</p>
          <ul>
            <li>The API is running on your local machine</li>
            <li>It's accessible at <code>{swaggerUrl}</code></li>
            <li>There are no firewall or network issues blocking the connection</li>
          </ul>
          <button onClick={openSwaggerDocs} className="try-anyway-btn">
            Try Opening Swagger UI Anyway
          </button>
        </div>
      )}
      
      {apiStatus === 'online' && (
        <div className="api-status online">
          <p>✅ API is online and accessible</p>
          <button onClick={openSwaggerDocs} className="view-docs-btn">
            Open Swagger Documentation
          </button>
          <p className="swagger-info">
            You can also access the Swagger UI directly at: <a href={swaggerUrl} target="_blank" rel="noopener noreferrer">{swaggerUrl}</a>
          </p>
        </div>
      )}
    </div>
  );
};

export default ApiDocs;