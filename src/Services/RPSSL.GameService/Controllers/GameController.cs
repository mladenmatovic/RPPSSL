using Microsoft.AspNetCore.Mvc;
using RPSSL.GameService.Models;
using RPSSL.GameService.Services;
using RPSSL.GameService.GameLogic.Enums;

namespace RPSSL.GameService.Controllers
{
    [ApiController]
    [Route("api")]
    public class GameController : ControllerBase
    {
        private readonly IGameService _gameService;
        private readonly IGamePlayService _gamePlayService;

        public GameController(IGameService gameService, IGamePlayService gamePlayService)
        {
            _gameService = gameService;
            _gamePlayService = gamePlayService;
        }

        [HttpPost]
        public async Task<ActionResult<Game>> CreateGame(CreateGameRequest request)
        {
            var game = await _gameService.CreateGameAsync(request.RoomId, request.Player1Id, request.Player2Id);
            return CreatedAtAction(nameof(GetGame), new { id = game.Id }, game);
        }

        [HttpGet("game/{id}")]
        public async Task<ActionResult<Game>> GetGame(Guid id)
        {
            var game = await _gameService.GetGameAsync(id);
            if (game == null)
                return NotFound();
            return game;
        }

        [HttpPost("game/{id}/moves")]
        public async Task<ActionResult<Game>> MakeMove(Guid id, MakeMoveRequest request)
        {
            try
            {
                var game = await _gameService.MakeMoveAsync(id, request.PlayerId, request.Move);
                return Ok(game);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("choices")]
        public ActionResult<IEnumerable<Choice>> GetChoices()
        {
            return Ok(_gamePlayService.GetAllChoices());
        }

        [HttpGet("choice")]
        public async Task<ActionResult<Choice>> GetRandomChoice()
        {
            return Ok(await _gamePlayService.GetRandomChoiceAsync());
        }

        [HttpPost("play")]
        public async Task<ActionResult<PlayResult>> Play([FromBody] PlayRequest request)
        {
            return Ok(await _gamePlayService.PlayAsync(request.Player));
        }
    }

    public class CreateGameRequest
    {
        public Guid RoomId { get; set; }
        public string Player1Id { get; set; }
        public string Player2Id { get; set; }
    }

    public class MakeMoveRequest
    {
        public string PlayerId { get; set; }
        public Move Move { get; set; }
    }
}
