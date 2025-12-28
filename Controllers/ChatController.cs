using AutoMapper;
using JewelryShop.DTO;
using JewelryShop.Hubs;
using JewelryShop.Models;
using JewelryShop.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace JewelryShop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : Controller
    {
        private readonly JewelryShopContext _context;
        private readonly IChatService _chatService;
        private readonly IMapper _mapper;
        private readonly IHubContext<ChatHub> _hub;
        private readonly IBotService _botService;

        public ChatController(JewelryShopContext context, IChatService chatService, IMapper mapper, IHubContext<ChatHub> hub, IBotService botService)
        {
            _context = context;
            _chatService = chatService;
            _mapper = mapper;
            _hub = hub;
            _botService = botService;
        }

        [HttpPost("room")]
        public async Task<IActionResult> GetOrCreateRoom([FromBody] CreateRoomDTO dto)
        {
            var room = await _chatService.GetOrCreateRoomAsync(dto.UserId, dto.OrderId);
            return Ok(room);
        }


        [HttpGet("rooms/user/{userId:int}")]
        public async Task<IActionResult> GetUserRooms(int userId)
        {
            var rooms = await _chatService.GetUserRoomsAsync(userId);
            return Ok(rooms);
        }

        [HttpGet("rooms/admin/{adminId:int}")]
        public async Task<IActionResult> GetAdminRooms(int adminId)
        {
            var rooms = await _chatService.GetAdminRoomsAsync(adminId);
            return Ok(rooms);
        }


        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageDTO dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.MessageText))
                return BadRequest("Message cannot be empty");
            var message = new Message
            {
                ChatRoomId = dto.ChatRoomId,
                SenderId = dto.SenderId,
                MessageText = dto.MessageText,
                CreatedAt = DateTime.Now,
                IsBot = false
            };
            var savedMsg = await _chatService.AddMessageAsync(message);
            await _hub.Clients.Group(dto.ChatRoomId.ToString())
                .SendAsync("ReceiveMessage", _mapper.Map<MessageDTO>(savedMsg));

            var sender = await _context.Users.FindAsync(dto.SenderId);
            bool isAdmin = sender != null && sender.Role == "Admin";

            // Chỉ user (ko phải admin) mới kích hoạt bot
            if (!isAdmin)
            {
                string botReply = await _botService.GetBotResponse(dto.MessageText, dto.ChatRoomId);
                if (!string.IsNullOrEmpty(botReply))
                {
                    var botMessage = new Message
                    {
                        ChatRoomId = dto.ChatRoomId,
                        SenderId = null,
                        MessageText = botReply,
                        CreatedAt = DateTime.Now,
                        IsBot = true
                    };

                    var savedBotMsg = await _chatService.AddMessageAsync(botMessage);

                    await _hub.Clients.Group(dto.ChatRoomId.ToString())
                        .SendAsync("ReceiveMessage", _mapper.Map<MessageDTO>(savedBotMsg));
                }
            }

            return Ok(_mapper.Map<MessageDTO>(savedMsg));
        }
        [HttpGet("messages/{roomId:int}")]
        public async Task<IActionResult> GetMessages(int roomId)
        {
            var messages = await _chatService.GetMessagesByRoomIdAsync(roomId);

            return Ok(_mapper.Map<List<MessageDTO>>(messages));
        }

        [HttpGet("GetUserStatus/{userId}")]
        public IActionResult GetUserStatus(int userId)
        {
            bool isOnline = ChatHub.IsUserOnline(userId);
            var lastActive = ChatHub.GetLastActive(userId);

            return Ok(new
            {
                isOnline,
                lastActive = lastActive?.ToString("o")
            });
        }

    }
}
