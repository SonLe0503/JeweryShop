using AutoMapper;
using JewelryShop.DTO;
using JewelryShop.Models;

namespace JewelryShop.Mappers
{
    public class OrderProfile : Profile
    {
        public OrderProfile() 
        {
            CreateMap<CreateOrderDTO, Order>()
                .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalPrice))
                .ForMember(dest => dest.OrderDate, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.OrderDetails, opt => opt.MapFrom(src => src.OrderDetails));

            CreateMap<Order, OrderDTO>()
                .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.TotalAmount))
                .ForMember(dest => dest.OrderDetails, opt => opt.MapFrom(src => src.OrderDetails));
        }
    }
}
