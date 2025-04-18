# AI_agent
Ai agent Hackathon microsoft 2025


# project idea : 
AI agent (trading bot) that would readjust strategy and algos using RL 
build a HFT simulator (maybe)
Crypto binance donnée (crypto) l’order book. 
SVR ( ML)
Linear Regression

Main_doc:
https://docs.google.com/document/d/1nM4O-MDaC8PKeEep85yH2aPi-AFLXG019XNaknRmLec/edit?tab=t.0#heading=h.o4rytw2azsu7

version of .NET used : 9.0.102

/*
 * Crypto Trading AI System Architecture
 * 
 * This solution structure outlines a high-performance C# implementation
 * of an AI-driven cryptocurrency trading system with pattern recognition,
 * sentiment analysis, and reinforcement learning capabilities.
 */

// Project structure overview:
/*
AIAgentCryptoTrading/
├── AIAgentCryptoTrading.Core/             # Core domain models and interfaces
├── AIAgentCryptoTrading.DataCollector/    # Data collection from crypto exchanges
├── AIAgentCryptoTrading.StrategyEngine/   # Trading strategy implementations
└── AIAgentCryptoTrading.Api/              # Web API endpoints
*/
# to be added later :
dotnet new classlib -n AIAgentCryptoTrading.PatternRecognition
dotnet new classlib -n AIAgentCryptoTrading.SentimentAnalysis

# to run App : 
npm start

# strategy trained on : Moving Average Crossover strategy
1-Calculate two moving averages of different periods (e.g., 10-day and 30-day)
2-Generate a buy signal when the shorter moving average crosses above the longer one
3-Generate a sell signal when the shorter moving average crosses below the longer one

Strong trending markets: The strategy excels in markets with clear, sustained trends. When prices are consistently moving in one direction over time, the moving averages align properly and generate fewer false signals.

Low to moderate volatility: Markets with some volatility but not excessive choppiness work best. Extremely calm markets don't generate enough movement for profitable trades, while extremely volatile markets create too many false crossovers.

Liquid assets: Assets with high trading volume and liquidity allow for efficient entries and exits at prices close to the signals generated.

Longer timeframes: Moving average strategies typically perform better on longer timeframes (daily, weekly) than on very short timeframes (minutes, hours) which tend to be noisier.
Markets with cyclical or momentum characteristics: Assets that tend to maintain momentum once a trend is established (like many cryptocurrencies during bull markets) are ideal for this strategy.


link : (to be removed in the end)
https://colab.research.google.com/drive/10Qb11tN7z0Vbtiw0kQPdGFlrGcJVqk7A?usp=sharing