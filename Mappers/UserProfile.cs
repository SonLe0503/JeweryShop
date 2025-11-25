using AutoMapper;
using JewelryShop.DTO;
using JewelryShop.Models;

namespace JewelryShop.Mappers
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserResponse>()
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.Avatar))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));
            CreateMap<User, UpdateUserRequest>()
               .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
               .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber ?? string.Empty))
               .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role ?? string.Empty))
               .ForMember(dest => dest.Password, opt => opt.Ignore())
               .ForMember(dest => dest.OldPassword, opt => opt.Ignore());
        }
    }
}
