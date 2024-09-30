using AutoMapper;
using ProductsService.BusinessLogic.Responses;
using ProductsService.Data.Entities;

namespace ProductsService.BusinessLogic.Mapper
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            CreateMap<ProductInputModel, ProductEntity>();
        }
    }
}
