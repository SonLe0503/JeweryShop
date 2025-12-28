using JewelryShop.Models;
using Microsoft.EntityFrameworkCore;

namespace JewelryShop.Services
{
    public class ChatService : IChatService
    {
        private readonly JewelryShopContext _context;

        public ChatService(JewelryShopContext context)
        {
            _context = context;
        }

        private int GetAvailableAdminId()
        {
            var adminRoomCounts = _context.Users
                .Where(u => u.Role == "Admin")
                .Select(admin => new
                {
                    AdminId = admin.UserId,
                    RoomCount = _context.ChatRooms.Count(r => r.AdminId == admin.UserId)
                })
                .OrderBy(x => x.RoomCount)
                .FirstOrDefault();

            if (adminRoomCounts == null)
                throw new Exception("Không có admin nào trong hệ thống!");

            return adminRoomCounts.AdminId;
        }

        public async Task<ChatRoom> GetOrCreateRoomAsync(int userId, int? orderId = null)
        {
            var room = await _context.ChatRooms
                .FirstOrDefaultAsync(r =>
                    r.UserId == userId &&
                    r.OrderId == orderId &&
                    r.Status == "Active"
                );

            // ✅ Nếu room đã tồn tại nhưng chưa có admin → GÁN NGAY
            if (room != null)
            {
                if (room.AdminId == null)
                {
                    room.AdminId = GetAvailableAdminId();
                    await _context.SaveChangesAsync();
                }

                return room;
            }

            int adminId = GetAvailableAdminId();

            room = new ChatRoom
            {
                UserId = userId,
                OrderId = orderId,
                AdminId = adminId,               
                CreatedAt = DateTime.Now,
                Status = "Active"
            };

            _context.ChatRooms.Add(room);
            await _context.SaveChangesAsync();

            return room;
        }

        public async Task<IEnumerable<ChatRoom>> GetUserRoomsAsync(int userId)
        {
            return await _context.ChatRooms
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ChatRoom>> GetAdminRoomsAsync(int adminId)
        {
            return await _context.ChatRooms
                .Where(r => r.AdminId == adminId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<Message> AddMessageAsync(Message message)
        {
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task<List<Message>> GetMessagesByRoomIdAsync(int roomId)
        {
            return await _context.Messages
                .Where(m => m.ChatRoomId == roomId)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }
    }
}
