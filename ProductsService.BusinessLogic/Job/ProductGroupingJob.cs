using ProductsService.BusinessLogic.Services;
using Quartz;

public class ProductGroupingJob : IJob
{
    private readonly ProductService _productService;

    public ProductGroupingJob(ProductService productService)
    {
        _productService = productService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await _productService.GroupProductsAsync();
    }
}