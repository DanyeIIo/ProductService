using Microsoft.AspNetCore.Http;
using ProductsService.BusinessLogic.Responses;

namespace ProductsService.BusinessLogic.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductInputModel>> ProcessExcelFileAsync(IFormFile file);
        Task SaveProductsAsync(IEnumerable<ProductInputModel> products);
        Task GroupProductsAsync();
        Task<IEnumerable<GroupResultModel>> GetGroupResults();
        Task<GroupProductResultModel> GetProductsByGroupId(string id);
    }
}
