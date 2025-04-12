using AIAgentCryptoTrading.Core.Interfaces;
using AIAgentCryptoTrading.Core.Models;
using AIAgentCryptoTrading.Api.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;


namespace AIAgentCryptoTrading.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BacktestController : ControllerBase
    {
        private readonly IBacktester _backtester;
        
        public BacktestController(IBacktester backtester)
        {
            _backtester = backtester;
        }
        
        [HttpPost("run")]
        public async Task<ActionResult<BacktestResult>> RunBacktest([FromBody] BacktestRequest request)
        {
            try
            {
                var result = await _backtester.RunBacktestAsync(
                    request.Symbol,
                    request.Strategy,
                    request.StartDate,
                    request.EndDate
                );
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error running backtest: {ex.Message}");
            }
        }
        
        [HttpGet("strategies")]
        public ActionResult GetAvailableStrategies()
        {
            // Return a list of available strategies
            var strategies = new[]
            {
                new { Name = "sma", Description = "Simple Moving Average Crossover Strategy" },
                new { Name = "macd", Description = "MACD Crossover Strategy" }
            };
            
            return Ok(strategies);
        }
    }
    
}