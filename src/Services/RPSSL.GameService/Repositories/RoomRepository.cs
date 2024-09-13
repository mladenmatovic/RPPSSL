using RPSSL.GameService.Data;
using RPSSL.GameService.Models;
using Microsoft.EntityFrameworkCore;

namespace RPSSL.GameService.Repositories
{
    public class RoomRepository : GenericRepository<Room>, IRoomRepository
    {
        public RoomRepository(GameDbContext context, ILogger<RoomRepository> logger) : base(context, logger)
        {

        }

        public async Task<bool> ArchiveRoom(Room room)
        {
            room.IsArchived = true;
            _context.Rooms.Update(room);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Room> GetActiveRoomPlayerAsync(string player)
        {
            return await _context.Rooms
                .Where(r => !r.IsArchived)
                .FirstOrDefaultAsync(r =>
                    r.Player1 == player ||
                    r.Player2 == player);
        }

        public async Task<IEnumerable<Room>> GetActiveRooms()
        {
            return await _context.Rooms
                .Where(r => !r.IsArchived)
                .ToListAsync();
        }

    }
}
