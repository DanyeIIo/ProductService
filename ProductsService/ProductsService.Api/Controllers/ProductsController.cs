using Microsoft.AspNetCore.Mvc;
using ProductsService.BusinessLogic.Interfaces;
using ProductsService.BusinessLogic.Responses;

namespace ProductsService.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _service;

        public ProductsController(IProductService service)
        {
            _service = service;
        }

        [HttpPost("upload")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0 || Path.GetExtension(file.FileName) != ".xlsx")
            {
                return BadRequest("Invalid file.");
            }

            var products = await _service.ProcessExcelFileAsync(file);

            await _service.SaveProductsAsync(products);
            return Ok("File uploaded");
        }

        [HttpGet]
        [ProducesResponseType(typeof(GroupResultModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetGroupResults()
        {
            var groupResults = await _service.GetGroupResults();

            if (groupResults.Any())
            {
                return Ok(groupResults);
            }

            return NotFound();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(IEnumerable<GroupProductResultModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProductsByGroupId(string id)
        {
            var groupProductResult = await _service.GetProductsByGroupId(id);

            if (groupProductResult == null)
            {
                return NotFound();
            }

            return Ok(groupProductResult);
        }
    }
}
