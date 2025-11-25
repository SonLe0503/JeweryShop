using AutoMapper;
using JewelryShop.DTO;
using JewelryShop.Models;

namespace JewelryShop.Mappers
{
    public class ProductProfile : Profile 
    {
        public ProductProfile()
        {
            CreateMap<Product, ProductDTO>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null))
            .ForMember(dest => dest.CollectionName, opt => opt.MapFrom(src => src.Collection != null ? src.Collection.Name : null))
            .ForMember(dest => dest.ProductImages, opt => opt.MapFrom(src => src.ProductImages.Select(pi => pi.ImageUrl).ToList()))
            .ForMember(dest => dest.Reviews, opt => opt.MapFrom(src => src.Reviews));

        }
    }
}
