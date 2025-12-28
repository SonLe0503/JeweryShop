using JewelryShop.Models;

namespace JewelryShop.Services
{
    public interface IChatService
    {
        Task<ChatRoom> GetOrCreateRoomAsync(int userId, int? orderId = null);
        Task<IEnumerable<ChatRoom>> GetUserRoomsAsync(int userId);
        Task<IEnumerable<ChatRoom>> GetAdminRoomsAsync(int adminId);
        Task<Message> AddMessageAsync(Message message);
        Task<List<Message>> GetMessagesByRoomIdAsync(int roomId);

    }
}
