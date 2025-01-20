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

        [HttpGet("topScore")]
        public ActionResult<List<ScoreboardEntry>> GetTopScoreRecords()
        {
            var records = _scoreboardService.GetTopScoreRecords();
            return Ok(records);
        }
    }
}