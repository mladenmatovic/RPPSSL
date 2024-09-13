using RPSSL.GameService.Models;
using Microsoft.AspNetCore.Mvc;
using RPSSL.GameService.GameLogic.Enums;

namespace RPSSL.GameService.Services
{
    public interface IGameService
    {
        Task<Game> CreateGameAsync(Guid roomId, string player1Id, string player2Id);
        Task<Game> CreateGameAsync(Guid roomId);
        Task<Game> GetLastGameForRoom(Guid roomId);
        Task<Game> GetGameAsync(Guid id);
        Task<Game> MakeMoveAsync(Guid gameId, string playerId, Move move);
    }
}
