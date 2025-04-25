@echo off
echo Starting development environment...

:: Start the .NET backend in a new window
start cmd /k "cd /d C:\Users\Tri Uyen\Desktop\Ai_agentic\AI_agent\AIAgentCryptoTrading.Api && dotnet run"

:: Start the React frontend in a new window
start cmd /k "cd /d C:\Users\Tri Uyen\Desktop\Ai_agentic\AI_agent\frontend && npm start"

echo Both applications are starting...
echo Frontend: http://localhost:3000
echo Backend API: https://localhost:5124
echo Press any key to exit.