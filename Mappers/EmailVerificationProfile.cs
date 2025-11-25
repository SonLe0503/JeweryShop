using AutoMapper;
using JewelryShop.DTO;
using JewelryShop.Models;

namespace JewelryShop.Mappers
{
    public class EmailVerificationProfile : Profile
    {
        public EmailVerificationProfile() 
        {
            CreateMap<EmailVerification, EmailVerificationDTO>().ReverseMap();
        }
    }
}
