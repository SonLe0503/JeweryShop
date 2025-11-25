using AutoMapper;
using JewelryShop.DTO;
using JewelryShop.Models;

namespace JewelryShop.Mappers
{
    public class PaymentTransactionProfile : Profile
    {
        public PaymentTransactionProfile() 
        {
            CreateMap<CreatePaymentTransactionDTO, PaymentTransaction>()
                .ForMember(dest => dest.PaymentDate, opt => opt.MapFrom(src => DateTime.Now));
            CreateMap<PaymentTransaction, PaymentTransactionDTO>()
                .ForMember(dest => dest.OrderTotalPrice, opt => opt.MapFrom(src => src.Order != null ? src.Order.TotalAmount : (decimal?)null))
                .ForMember(dest => dest.OrderStatus, opt => opt.MapFrom(src => src.Order != null ? src.Order.Status : null));
        }
    }
}
