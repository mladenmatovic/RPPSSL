using RPSSL.GameService.Models.Enums;
using RPSSL.GameService.Models;
using RPSSL.GameService.Repositories;
using RPSSL.GameService.GameLogic.Enums;
using RPSSL.GameService.GameLogic.Factories;

namespace RPSSL.GameService.Services
{
    public class GameService : IGameService
    {
        private readonly IGameRepository _gameRepository;

        public GameService(IGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        public async Task<Game> CreateGameAsync(Guid roomId, string player1Id, string player2Id)
        {
            var newGame = new Game
            {
                RoomId = roomId,
                Player1Id = player1Id,
                Player2Id = player2Id,
                Status = GameStatus.InProgress,
                CreatedAt = DateTime.UtcNow
            };

            return await _gameRepository.CreateGameAsync(newGame);
        }

        public async Task<Game> CreateGameAsync(Guid roomId)
        {
            var room = await _gameRepository.GetRoomAsync(roomId);

            if (room == null)
            {
                throw new ArgumentException("Room not found", nameof(roomId));
            }

            if (string.IsNullOrEmpty(room.Player1) || string.IsNullOrEmpty(room.Player2))
            {
                throw new InvalidOperationException("Room does not have two players");
            }

            var existingGame = await _gameRepository.GetActiveGameForRoomPlayersAsync(roomId, room.Player1, room.Player2);
            if (existingGame != null)
            {
                throw new InvalidOperationException("A game already exists for this room");
            }

            var game = new Game
            {
                Id = Guid.NewGuid(),
                RoomId = roomId,
                Player1Id = room.Player1,
                Player2Id = room.Player2,
                Status = GameStatus.InProgress,
                CreatedAt = DateTime.UtcNow
            };

            return await _gameRepository.CreateGameAsync(game);
        }

        public async Task<Game> GetLastGameForRoom(Guid roomId)
        {
            var room = await _gameRepository.GetRoomAsync(roomId);

            if (room == null)
            {
                throw new ArgumentException("Room not found", nameof(roomId));
            }

            if (string.IsNullOrEmpty(room.Player1) || string.IsNullOrEmpty(room.Player2))
            {
                throw new InvalidOperationException("Room does not have two players");
            }

            return await _gameRepository.GetLastGameForRoomPlayersAsync(roomId, room.Player1, room.Player2);
        }

        public async Task<Game> GetGameAsync(Guid gameId)
        {
            return await _gameRepository.GetByIdAsync(gameId);
        }

        public async Task<Game> MakeMoveAsync(Guid gameId, string playerId, Move move)
        {
            var game = await _gameRepository.GetByIdAsync(gameId);
            if (game == null)
                throw new ArgumentException("Game not found");

            if (game.Status != GameStatus.InProgress)
                throw new InvalidOperationException("Game is not in progress");

            if (playerId == game.Player1Id && !game.Player1Move.HasValue)
                game.Player1Move = move;
            else if (playerId == game.Player2Id && !game.Player2Move.HasValue)
                game.Player2Move = move;
            else
                throw new InvalidOperationException("Invalid move");

            if (game.Player1Move.HasValue && game.Player2Move.HasValue)
            {
                game.Status = GameStatus.Completed;
                game.WinnerId = DetermineWinner(game);
                game.CompletedAt = DateTime.UtcNow;
            }

            return await _gameRepository.UpdateGameAsync(game);
        }

        private string DetermineWinner(Game game)
        {
            if (game.Player1Move == game.Player2Move)
                return null; // Draw

            var player1Strategy = MoveStrategyFactory.GetStrategy(game.Player1Move!.Value);

            if (player1Strategy.Beats(game.Player2Move.Value))
                return game.Player1Id;

            return game.Player2Id;
        }
    }
}
