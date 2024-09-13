using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using RPSSL.GameService.DTOs;
using RPSSL.GameService.GameLogic.Enums;
using RPSSL.GameService.Models;
using RPSSL.GameService.Models.Enums;
using RPSSL.GameService.Services;
using RPSSL.Shared.Models;
using System.Collections.Concurrent;

namespace RPSSL.GameService.GameHub
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class GameHub : Hub
    {
        private readonly IGameLobbyService _lobbyService;
        private readonly IGameService _gameService;
        private static readonly ConcurrentDictionary<string, (Guid RoomId, DateTime DisconnectTime)> _disconnectedUsers = new();
        private const int DISCONNECT_GRACE_PERIOD_SECONDS = 30;

        public GameHub(
            IGameLobbyService lobbyService, IGameService gameService)
        {
            _lobbyService = lobbyService;
            _gameService = gameService;     
        }

        public async Task GetRooms()
        {
            var rooms = await _lobbyService.ListRoomsAsync();
            await Clients.Caller.SendAsync("ReceiveRooms", rooms);
        }

        public async Task<Guid> CreateRoom()
        {
            var username = Context.User.Identity.Name;
            var result = await _lobbyService.CreateRoomAsync(username);
            if (result is not null)
            {
                await Clients.All.SendAsync("RoomCreated", new RoomDto(result.Id, result.Player2 == null ? 1 : 2, result.CreatedAt));
                return result.Id;
            }
            throw new HubException("Failed to create room");
        }

        public async Task JoinRoom(Guid roomId)
        {
            var username = Context.User.Identity.Name;
            var result = await _lobbyService.JoinRoomAsync(roomId, username);

            if (!result.Succeeded)
            {
                await Clients.Caller.SendAsync("JoinRoomFailed", result.Error);
                return;
            }

            // Handle new join
            await Clients.Group(roomId.ToString()).SendAsync("PlayerJoined", username);
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());

            var game = result.Room?.Games.OrderByDescending(x => x.CreatedAt).FirstOrDefault();
            var gameStateDto = game != null ? GameStateDto.FromGame(game) : null;

            if (result.IsReconnection)
            {
                if (gameStateDto != null)
                {
                    await Clients.Caller.SendAsync("GameStateUpdated", gameStateDto);
                }
                else
                {
                    await Clients.Caller.SendAsync("JoinedRoom", gameStateDto);
                }
            }
            else
            {
                await Clients.Group(roomId.ToString()).SendAsync("PlayerJoined", username);

                if (result.Room.Player1 is not null & result.Room.Player2 is not null)
                {
                    if (gameStateDto == null)
                    {
                        game = await _gameService.CreateGameAsync(roomId);
                        gameStateDto = GameStateDto.FromGame(game);
                    }
                    await Clients.Group(roomId.ToString()).SendAsync("GameCreated", gameStateDto);
                }
                else
                {
                    await Clients.Caller.SendAsync("JoinedRoom", gameStateDto);
                }
            }
        }

        /*public async Task JoinRoom(Guid roomId)
        {
            var username = Context.User.Identity.Name;
            var result = await _lobbyService.JoinRoomAsync(roomId, username);

            await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
            if (result.Succeeded)
            {
                if (result.IsReconnection)
                {
                    if (result.GameReady)
                    {
                        var gameState = await _gameService.GetLastGameForRoom(roomId);
                        var gameStateDto = GameStateDto.FromGame(gameState);
                        await Clients.Caller.SendAsync("GameStateUpdated", gameStateDto);
                    }
                    else
                    {
                        var game = await _gameService.GetLastGameForRoom(roomId);
                        if (game is not null)
                        {
                            var gameStateDto = GameStateDto.FromGame(game);
                            await Clients.Group(roomId.ToString()).SendAsync("JoinedRoom", gameStateDto); 
                        }
                        else
                        {                            
                            await Clients.Caller.SendAsync("JoinedRoom", null);
                        }
                    }
                }
                else
                {
                    await Clients.Group(roomId.ToString()).SendAsync("PlayerJoined", username);

                    if (result.GameReady)
                    {
                        // check if active game already exists
                        var game = await _gameService.GetLastGameForRoom(roomId);
                        if (game is null)
                        {
                            game = await _gameService.CreateGameAsync(roomId);
                        }                        
                        var gameStateDto = GameStateDto.FromGame(game);
                        await Clients.Group(roomId.ToString()).SendAsync("GameCreated", gameStateDto);
                    }
                    else 
                    {
                        await Clients.Caller.SendAsync("JoinedRoom", null);
                    }
                }
            }
            else
            {
                await Clients.Caller.SendAsync("JoinRoomFailed", result.Error);
            }
        }*/

        public async Task LeaveRoom(Guid roomId)
        {
            var username = Context.User.Identity.Name;
            var result = await _lobbyService.LeaveRoomAsync(roomId, username);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId.ToString());
            await Clients.Group(roomId.ToString()).SendAsync("PlayerLeft", username);

            var updatedRoom = await _lobbyService.GetRoomAsync(roomId);
            if (updatedRoom != null)
            {
                if (updatedRoom.IsEmpty())
                {
                    await _lobbyService.ArchiveRoom(updatedRoom);
                    await Clients.All.SendAsync("RoomArchived", roomId);
                }
                else
                {
                    await Clients.All.SendAsync("RoomUpdated", new RoomDto(updatedRoom.Id, updatedRoom.Player2 == null ? 1 : 2, updatedRoom.CreatedAt));
                }
            }
        }

        public async Task MakeMove(Guid gameId, int moveId)
        {
            var username = Context.User.Identity.Name;
            var result = await _gameService.MakeMoveAsync(gameId, username, (Move)moveId);
            var gameStateDto = GameStateDto.FromGame(result);
            await Clients.Group(result.RoomId.ToString()).SendAsync("GameStateUpdated", gameStateDto);
        }

        public async Task RequestNewGame(string roomId)
        {
            var playerId = Context.User.Identity.Name;
            await Clients.OthersInGroup(roomId).SendAsync("PlayerWantsNewGame", playerId);
        }

        public async Task StartNewGame(Guid roomId)
        {            
            var newGame = await _gameService.CreateGameAsync(roomId);
            var gameStateDto = GameStateDto.FromGame(newGame);
            await Clients.Group(roomId.ToString()).SendAsync("NewGameStarted", gameStateDto);
        }

        public override async Task OnConnectedAsync()
        {
            var username = Context.User.Identity.Name;

            if (_disconnectedUsers.TryRemove(username, out var disconnectInfo))
            {
                // User reconnected within the grace period
                await Groups.AddToGroupAsync(Context.ConnectionId, disconnectInfo.RoomId.ToString());
                await Clients.Group(disconnectInfo.RoomId.ToString()).SendAsync("PlayerReconnected", username);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var username = Context.User.Identity.Name;
            var room = await _lobbyService.GetUserInRoomAsync(username);

            if (room != null)
            {
                _disconnectedUsers[username] = (room.Id, DateTime.UtcNow);
                await Clients.Group(room.Id.ToString()).SendAsync("PlayerTemporarilyDisconnected", username);

                // Start a background task to handle the disconnection after the grace period
                _ = HandleDisconnectionAfterGracePeriodAsync(username, room.Id);
            }

            await base.OnDisconnectedAsync(exception);
        }

        private async Task HandleDisconnectionAfterGracePeriodAsync(string username, Guid roomId)
        {
            await Task.Delay(TimeSpan.FromSeconds(DISCONNECT_GRACE_PERIOD_SECONDS));

            if (_disconnectedUsers.TryRemove(username, out var disconnectInfo) && disconnectInfo.RoomId == roomId)
            {
                await _lobbyService.LeaveRoomAsync(roomId, username);
                await Clients.Group(roomId.ToString()).SendAsync("PlayerLeft", username);

                var updatedRoom = await _lobbyService.GetRoomAsync(roomId);
                if (updatedRoom != null)
                {
                    if (updatedRoom.IsEmpty())
                    {
                        await _lobbyService.ArchiveRoom(updatedRoom);
                        await Clients.All.SendAsync("RoomArchived", roomId);
                    }
                    else
                    {
                        await Clients.All.SendAsync("RoomUpdated", new RoomDto(updatedRoom.Id, updatedRoom.Player2 == null ? 1 : 2, updatedRoom.CreatedAt));
                    }
                }
            }
        }
    }
}
