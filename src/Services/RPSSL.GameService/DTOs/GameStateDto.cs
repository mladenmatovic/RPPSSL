using RPSSL.GameService.Models.Enums;
using RPSSL.GameService.Models;

namespace RPSSL.GameService.DTOs
{
    public class GameStateDto
    {
        public string Id { get; set; }
        public string RoomId { get; set; }
        public string Player1Id { get; set; }
        public string Player2Id { get; set; }
        public int? Player1Move { get; set; }
        public int? Player2Move { get; set; }
        public GameStatus Status { get; set; }
        public string WinnerId { get; set; }
        public string CreatedAt { get; set; }
        public string CompletedAt { get; set; }

        public static GameStateDto FromGame(Game game)
        {
            return new GameStateDto
            {
                Id = game.Id.ToString(),
                RoomId = game.RoomId.ToString(),
                Player1Id = game.Player1Id,
                Player2Id = game.Player2Id,
                Player1Move = game.Player1Move.HasValue ? (int)game.Player1Move.Value : null,
                Player2Move = game.Player2Move.HasValue ? (int)game.Player2Move.Value : null,
                Status = game.Status,
                WinnerId = game.WinnerId,
                CreatedAt = game.CreatedAt.ToString("o"),  // ISO 8601 format
                CompletedAt = game.CompletedAt?.ToString("o"),
            };
        }
    }
}
