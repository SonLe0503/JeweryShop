using AutoMapper;
using JewelryShop.DTO;
using JewelryShop.Models;

namespace JewelryShop.Mappers
{
    public class ChatProfile : Profile
    {
        public ChatProfile() 
        {
            CreateMap<ChatRoom, ChatRoomDTO>().ReverseMap();
            CreateMap<Message, MessageDTO>().ReverseMap();
        }
    }
}
