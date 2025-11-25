using AutoMapper;
using JewelryShop.DTO;
using JewelryShop.Models;

namespace JewelryShop.Mappers
{
    public class CategoryProfile : Profile
    {
        public CategoryProfile()
        {
            CreateMap<Category, CategoryDTO>()
           .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Name))
           .ForMember(dest => dest.ProductCount, opt => opt.MapFrom(src => src.Products.Count))
           .ForMember(dest => dest.Products, otp => otp.MapFrom(src => src.Products));

            CreateMap<CategoryRequest, Category>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.CategoryName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));
        }
    }
}
