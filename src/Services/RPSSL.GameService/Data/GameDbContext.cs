using Microsoft.EntityFrameworkCore;
using RPSSL.GameService.Models;

namespace RPSSL.GameService.Data
{
    public class GameDbContext : DbContext
    {
        public GameDbContext(DbContextOptions<GameDbContext> options) : base(options) { }

        public DbSet<Game> Games { get; set; }
        public DbSet<Room> Rooms { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Game>()
                .Property(g => g.Player1Move)
                .HasConversion<string>();

            modelBuilder.Entity<Game>()
                .Property(g => g.Player2Move)
                .HasConversion<string>();

            modelBuilder.Entity<Game>()
                .Property(g => g.Status)
                .HasConversion<string>();

            modelBuilder.Entity<Room>()
            .HasMany(r => r.Games)
            .WithOne(g => g.Room)
            .HasForeignKey(g => g.RoomId)
            .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
