using RPSSL.GameService.Models;
using System.Reflection.Metadata;

namespace RPSSL.GameService.Repositories
{
    public interface IGameRepository : IGenericRepository<Game>
    {
        Task<Game> CreateGameAsync(Game newGame);
        Task<Game> GetActiveGameForRoomPlayersAsync(Guid roomId, string player1, string player2);
        Task<Game> GetLastGameForRoomPlayersAsync(Guid roomId, string player1, string player2);
        Task<Room> GetRoomAsync(Guid roomId);
        Task<Game> UpdateGameAsync(Game game);
    }
}
