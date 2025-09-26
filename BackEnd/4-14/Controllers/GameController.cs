using Microsoft.AspNetCore.Mvc;
using AllEnBackend.Services;
using AllEnBackend.Dtos;
using AllEnBackend.Models;

namespace AllEnBackend.Controllers
{
    [ApiController]
    [Route("api/game")]
    public class Game: ControllerBase
    {
        private readonly IGameService _gameService;

        public Game(IGameService gameService)
        {
            _gameService = gameService;
        }

        // ��ȡ�ɹ������Դ�б�
        [HttpGet("get-puzzle")]
        public async Task<IActionResult> GetPuzzle()
        {
            var puzzles = await _gameService.GetPuzzle();
            return Ok(new { puzzles });
        }

        // ��ȡ���е���Դ
        [HttpGet("get-clue")]
        public async Task<IActionResult> GetClue(string id)
        {
            var clues = await _gameService.GetClue(id);
            return Ok(new { clues });
        }

    }
}