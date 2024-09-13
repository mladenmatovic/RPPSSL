using RPSSL.GameService.Data;
using RPSSL.GameService.Models.Enums;
using RPSSL.GameService.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;

namespace RPSSL.GameService.Repositories
{
    public class GameRepository : GenericRepository<Game>, IGameRepository
    {
        public GameRepository(GameDbContext context, ILogger<GameRepository> logger) : base(context, logger)
        {

        }

        public async Task<Game> CreateGameAsync(Game newGame)
        {
            if (newGame.Id == Guid.Empty)
            {
                newGame.Id = Guid.NewGuid();
            }

            if (newGame.CreatedAt == default)
            {
                newGame.CreatedAt = DateTime.UtcNow;
            }

            _context.Games.Add(newGame);
            await _context.SaveChangesAsync();
            return newGame;
        }

        public async Task<Game> GetGameAsync(Guid id)
        {
            return await _context.Games.FindAsync(id);
        }

        public async Task<Game> GetActiveGameForRoomPlayersAsync(Guid roomId, string player1, string player2)
        {
            return await _context.Games
            .FirstOrDefaultAsync(g => g.RoomId == roomId &&
            g.Player1Id == player1 && 
            g.Player2Id == player2 &&
            g.Status != GameStatus.Completed);
        }

        public async Task<Game> GetLastGameForRoomPlayersAsync(Guid roomId, string player1, string player2)
        {
            return await _context.Games
                .OrderByDescending(p=> p.CreatedAt)
                    .FirstOrDefaultAsync(g => g.RoomId == roomId &&
                    g.Player1Id == player1 &&
                    g.Player2Id == player2
                    );
        }

        public async Task<Game> UpdateGameAsync(Game game)
        {
            _context.Games.Update(game);
            await _context.SaveChangesAsync();
            return game;
        }

        public async Task<Room> GetRoomAsync(Guid roomId)
        {
            return await _context.Rooms.FindAsync(roomId);
        }
    }
}
