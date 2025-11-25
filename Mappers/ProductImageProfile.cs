using AutoMapper;
using JewelryShop.DTO;
using JewelryShop.Models;

namespace JewelryShop.Mappers
{
    public class ProductImageProfile : Profile
    {
        public ProductImageProfile()
        {
            CreateMap<ProductImage, ProductImageDTO>();
            CreateMap<CreateProductImageDTO, ProductImage>();
            CreateMap<UpdateProductImageDTO, ProductImage>();
        }
    }
}
