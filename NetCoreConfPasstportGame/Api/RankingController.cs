using Microsoft.AspNetCore.Mvc;
using NetCoreConfPasstportGame.Options;
using NetCoreConfPasstportGame.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace NetCoreConfPasstportGame.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class RankingController : ControllerBase
    {
        // GET: api/<RankingController>
        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            var ranking = new RankingService();

            return Ok(await ranking.GetRanking());
        }

        [HttpGet]
        [Route("partners")]
        public async Task<IActionResult> Partners()
        {
            return Ok(SearchOptions.Partners);
        }
    }
}
