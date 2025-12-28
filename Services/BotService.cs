using JewelryShop.Models;
using Microsoft.EntityFrameworkCore;

namespace JewelryShop.Services
{
    public class BotService : IBotService
    {
        private readonly JewelryShopContext _context;

        public BotService(JewelryShopContext context)
        {
            _context = context;
        }

        public async Task<string> GetBotResponse(string inboundText, int chatRoomId)
        {
            inboundText = inboundText?.ToLowerInvariant() ?? string.Empty;

            // lấy các rule trong DB
            var candidates = await _context.BotResponses.ToListAsync();

            // tìm rule khớp keyword
            var match = candidates.FirstOrDefault(b => inboundText.Contains(b.Keyword.ToLower()));
            if (match != null) return match.Response;

            // kiểm tra xem phòng đã từng có tin nhắn bot chưa
            bool botHasRepliedBefore = await _context.Messages
                .AnyAsync(m => m.ChatRoomId == chatRoomId && m.IsBot == true);

            // nếu bot đã từng trả lời → không dùng câu mặc định nữa
            if (botHasRepliedBefore)
                return string.Empty; // hoặc null để không gửi tin nhắn

            // ngược lại → tin đầu tiên → dùng câu mặc định
            return "Cảm ơn bạn đã liên hệ! Chúng tôi sẽ phản hồi sớm.";
        }
    }
}
