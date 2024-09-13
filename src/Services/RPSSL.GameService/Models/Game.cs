using RPSSL.GameService.GameLogic.Enums;
using RPSSL.GameService.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace RPSSL.GameService.Models
{
    public class Game
    {
        [Key]
        public Guid Id { get; set; }
        public Guid RoomId { get; set; }
        public Room Room { get; set; }
        public string Player1Id { get; set; }
        public string Player2Id { get; set; }
        public Move? Player1Move { get; set; }
        public Move? Player2Move { get; set; }
        public GameStatus Status { get; set; }
        public string? WinnerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        
    }
}
