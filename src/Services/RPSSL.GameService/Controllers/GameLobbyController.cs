using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using RPSSL.GameService.Services;
using System.Security.Claims;

namespace RPSSL.GameService.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class GameLobbyController : ControllerBase
    {
        private readonly IGameLobbyService _lobbyService;
        private readonly IHubContext<GameHub.GameHub> _hubContext;

        public GameLobbyController(IGameLobbyService lobbyService, IHubContext<GameHub.GameHub> hubContext)
        {
            _lobbyService = lobbyService;
            _hubContext = hubContext;       
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateRoom()
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var room = await _lobbyService.CreateRoomAsync(username);
            await _hubContext.Clients.All.SendAsync("RoomCreated", new { RoomId = room.Id, Creator = username });
            return Ok(new { RoomId = room.Id });
        }

        [HttpPost("join/{roomId}")]
        public async Task<IActionResult> JoinRoom(string roomId)
        {
            Guid roomIdGuid = new Guid(roomId);
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var result = await _lobbyService.JoinRoomAsync(roomIdGuid, username);
            if (result.Succeeded)
                return Ok(new { Message = "Joined room successfully" });
            return BadRequest(result.Error);
        }

        [HttpGet("list")]
        public async Task<IActionResult> ListRooms()
        {
            var rooms = await _lobbyService.ListRoomsAsync();
            return Ok(rooms);
        }
    }
}
