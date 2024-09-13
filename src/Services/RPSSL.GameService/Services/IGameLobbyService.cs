using RPSSL.GameService.DTOs;
using RPSSL.GameService.Models;

namespace RPSSL.GameService.Services
{
    public interface IGameLobbyService
    {
        Task<Room> CreateRoomAsync(string username);
        Task<(bool Succeeded, string Error, Room? Room, bool IsReconnection)> JoinRoomAsync(Guid roomId, string username);
        Task<IEnumerable<RoomDto>> ListRoomsAsync();
        Task<(bool Succeeded, string Error)> LeaveRoomAsync(Guid roomId, string username);
        Task<Room> GetUserInRoomAsync(string username);
        Task<Room> GetRoomAsync(Guid roomId);
        Task<bool> ArchiveRoom(Room room);
    }
}
