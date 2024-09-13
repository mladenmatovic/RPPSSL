using Microsoft.EntityFrameworkCore;
using RPSSL.GameService.Data;
using RPSSL.GameService.DTOs;
using RPSSL.GameService.Models;
using RPSSL.GameService.Repositories;

namespace RPSSL.GameService.Services
{
    public class GameLobbyService : IGameLobbyService
    {
        private readonly GameDbContext _context;
        private readonly IRoomRepository _roomRepository;
        private readonly IGameService _gameService;

        public GameLobbyService(
            GameDbContext context,
            IGameService gameService,
            IRoomRepository roomRepository)
        {
            _context = context;
            _gameService = gameService;
            _roomRepository = roomRepository;
        }

        public async Task<Room> CreateRoomAsync(string username)
        {
            var room = new Room
            {
                Id = Guid.NewGuid(),
                Player1 = username,
                CreatedAt = DateTime.UtcNow
            };

            await _roomRepository.Add(room);            

            return room;
        }

        public async Task<Room> GetRoomAsync(Guid roomId)
        {
            return await _roomRepository.GetByIdAsync(roomId); 
        }

        public async Task<IEnumerable<Room>> GetActiveRoomsAsync()
        {
            return await _roomRepository.GetActiveRooms();
        }

        public async Task<Room> GetUserInRoomAsync(string username)
        {
            return await _roomRepository.GetActiveRoomPlayerAsync(username);
        }

        public async Task<(bool Succeeded, string Error, Room Room, bool IsReconnection)> JoinRoomAsync(Guid roomId, string username)
        {
            var room = await _context.Rooms
                .Include(r => r.Games)
                .FirstOrDefaultAsync(r => r.Id == roomId);

            if (room == null)
            {
                return (false, "Room not found", null, false);
            }

            // Check if the user is already in the room (reconnection scenario)
            if (room.Player1 == username || room.Player2 == username)
            {
                return (true, "Reconnected to the room", room, true);
            }

            if (room.Player1 != null && room.Player2 != null)
            {
                return (false, "Room is full", null, false);
            }

            if (room.Player1 == null)
            {
                room.Player1 = username;
            }
            else
            {
                room.Player2 = username;
            }

            await _context.SaveChangesAsync();            

            return (true, "Joined the room successfully", room, false);
        }

        public async Task<IEnumerable<RoomDto>> ListRoomsAsync()
        {
            return await _context.Rooms
                .Where(r => !r.IsArchived) 
                .Select(r => new RoomDto(r.Id, r.Player2 == null ? 1 : 2, r.CreatedAt))
                .ToListAsync();
        }

        public async Task<(bool Succeeded, string Error)> LeaveRoomAsync(Guid roomId, string username)
        {
            var room = await _context.Rooms.FindAsync(roomId);

            if (room == null)
            {
                return (false, "Room not found");
            }

            if (room.Player1 == username)
            {
                if (room.Player2 != null)
                {
                    // If there's a second player, make them the new room owner
                    room.Player1 = room.Player2;
                    room.Player2 = null;
                }
                else
                {
                    room.Player1 = null;
                }
            }
            else if (room.Player2 == username)
            {
                room.Player2 = null;
            }
            else
            {
                return (false, "User is not in this room");
            }

            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<bool> ArchiveRoom(Room room)
        {
            return await _roomRepository.ArchiveRoom(room);
        }
    }
}
