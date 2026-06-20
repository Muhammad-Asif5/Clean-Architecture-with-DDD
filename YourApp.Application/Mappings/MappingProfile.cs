using AutoMapper;
using YourApp.Application.Products.DTOs;
using YourApp.Domain.Entities;

namespace YourApp.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Product, ProductDto>();
        }
    }
}