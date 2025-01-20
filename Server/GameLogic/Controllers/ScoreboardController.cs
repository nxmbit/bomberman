using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Bomberman.Server.GameLogic;

namespace Bomberman.Server.Controllers
{
    [ApiController]
    [Route("scoreboard/[controller]")]
    public class ScoreboardController : ControllerBase
    {
        private readonly ScoreboardService _scoreboardService;

        public ScoreboardController(ScoreboardService scoreboardService)
        {
            _scoreboardService = scoreboardService;
        }

        [HttpGet("top20")]
        public ActionResult<List<ScoreboardEntry>> GetTop20Records()
        {
            var records = _scoreboardService.GetTop20Records();
            return Ok(records);
        }
    }
}