namespace JewelryShop.Services
{
    public interface IBotService
    {
        Task<string> GetBotResponse(string inboundText, int chatRoomId);
    }
}
