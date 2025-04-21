import React from 'react';

const ApiDocs = () => {
  // Replace with your actual Swagger URL
  const swaggerUrl = 'http://localhost:5124';
  
  return (
    <div className="api-docs-page">
      <h1>API Documentation</h1>
      <p>This page provides access to the complete API documentation for the AI Agent Crypto Trading system.</p>
      
      <div className="swagger-container">
        <iframe 
          src={swaggerUrl}
          title="API Documentation"
          width="100%"
          height="800px"
          style={{ border: 'none' }}
        />
      </div>
    </div>
  );
};

export default ApiDocs;