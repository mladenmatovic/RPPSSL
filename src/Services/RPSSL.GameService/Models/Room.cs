using System.ComponentModel.DataAnnotations;

namespace RPSSL.GameService.Models
{
    public class Room
    {
        [Key]
        public Guid Id { get; set; }
        public string? Player1 { get; set; }
        public string? Player2 { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsArchived { get; set; }
        public ICollection<Game> Games { get; set; } = new List<Game>();

        public bool IsEmpty()
        { 
            return Player1 == null && Player2 == null;
        }
    }
}
