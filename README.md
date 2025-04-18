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
└── AIAgentCryptoTrading.Api/    
|__frontend          # Web API endpoints
*/

# to be added later :
dotnet new classlib -n AIAgentCryptoTrading.PatternRecognition
dotnet new classlib -n AIAgentCryptoTrading.SentimentAnalysis

# to run App : 
npm start

## strategy trained on : Moving Average Crossover strategy
1-Calculate two moving averages of different periods (e.g., 10-day and 30-day)
2-Generate a buy signal when the shorter moving average crosses above the longer one
3-Generate a sell signal when the shorter moving average crosses below the longer one

Strong trending markets: The strategy excels in markets with clear, sustained trends. When prices are consistently moving in one direction over time, the moving averages align properly and generate fewer false signals.

Low to moderate volatility: Markets with some volatility but not excessive choppiness work best. Extremely calm markets don't generate enough movement for profitable trades, while extremely volatile markets create too many false crossovers.

Liquid assets: Assets with high trading volume and liquidity allow for efficient entries and exits at prices close to the signals generated.

Longer timeframes: Moving average strategies typically perform better on longer timeframes (daily, weekly) than on very short timeframes (minutes, hours) which tend to be noisier.
Markets with cyclical or momentum characteristics: Assets that tend to maintain momentum once a trend is established (like many cryptocurrencies during bull markets) are ideal for this strategy.

## second compelmentary strategy : 
Mean Reversion Strategy
Core Strategy Logic:

Identify overbought/oversold conditions using oscillators (RSI, Stochastic, Bollinger Bands)
Enter trades when price deviates significantly from its average, anticipating a return to the mean
Exit when price returns to its average value or a profit target is reached

Ideal Market Environment:

Sideways/range-bound markets: Performs best when prices oscillate within a defined range
High volatility with clear boundaries: Benefits from price swings without sustained directional movement
Established support/resistance levels: Creates natural reversal points for mean reversion
Short to medium timeframes: Often works better on shorter timeframes where ranges are more defined
Markets with regular corrections: Assets that tend to correct after significant moves rather than trending continuously

## Combine traditional strategy rules with a lightweight machine learning model that:
Detects the current market regime (trending vs. range-bound)
Selects the appropriate strategy based on the regime
Applies standard entry/exit rules for that strategy
Optionally filters signals based on success probability

This approach gives you the benefits of machine learning without the complexity and data requirements of deep learning.

    1.Market Regime Detection: A Random Forest classifier to identify whether the market is trending or range-bound.
    Strategy Selection Logic: Rule-based system that selects the appropriate strategy based on the detected market regime:

    2.Moving Average Crossover for trending markets
    Mean Reversion for range-bound markets


    3.Signal Generation: Traditional technical indicators and rules to generate basic buy/sell signals:
        Moving averages, crossovers, and trend indicators
        RSI, Bollinger Bands, and oscillators


    4.Signal Quality Filter: Another Random Forest model that evaluates the probability of success for each signal based on historical performance under similar conditions.

    5.Risk Management: Rule-based position sizing and stop-loss calculation based on volatility and support/resistance levels.