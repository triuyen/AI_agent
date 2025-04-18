
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AIAgentCryptoTrading.Core.Models;
using Microsoft.AspNetCore.Mvc;
using AIAgentCryptoTrading.StrategyEngine.Strategies;
using AIAgentCryptoTrading.StrategyEngine.Services;

namespace AIAgentCryptoTrading.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StrategyController : ControllerBase
    {
        [HttpGet("mean-reversion")]
        public async Task<IActionResult> GetMeanReversionSignals(
            [FromQuery] string coinId = "bitcoin",
            [FromQuery] string currency = "usd",
            [FromQuery] int days = 90,
            [FromQuery] int rsiPeriod = 14,
            [FromQuery] double rsiOversold = 30,
            [FromQuery] double rsiOverbought = 70,
            [FromQuery] int bbPeriod = 20,
            [FromQuery] double bbStd = 2,
            [FromQuery] bool exitMiddle = true)
        {
            try
            {
                var strategyService = new MeanReversionStrategyService();
                var result = await strategyService.ExecuteStrategyAsync(
                    coinId, currency, days, 
                    rsiPeriod, rsiOversold, rsiOverbought,
                    bbPeriod, bbStd, exitMiddle);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}