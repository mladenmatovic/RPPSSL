using RPSSL.GameService.Models;
using System.Reflection.Metadata;

namespace RPSSL.GameService.Repositories
{
    public interface IRoomRepository : IGenericRepository<Room>
    {
        Task<bool> ArchiveRoom(Room room);
        Task<Room> GetActiveRoomPlayerAsync(string player);
        Task<IEnumerable<Room>> GetActiveRooms();
    }
}
