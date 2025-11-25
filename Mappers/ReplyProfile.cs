using AutoMapper;
using JewelryShop.DTO;
using JewelryShop.Models;

namespace JewelryShop.Mappers
{
    public class ReplyProfile : Profile
    {
        public ReplyProfile()
        {
            CreateMap<Reply, ReplyDTO>();
            CreateMap<ReplyRequest, Reply>();
        }
    }
}
