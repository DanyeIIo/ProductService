using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using ProductsService.BusinessLogic.Interfaces;
using ProductsService.BusinessLogic.Responses;
using ProductsService.Data;
using ProductsService.Data.Entities;
using System.Text.RegularExpressions;

namespace ProductsService.BusinessLogic.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ProductService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProductInputModel>> ProcessExcelFileAsync(IFormFile file)
        {
            var products = new List<ProductInputModel>();

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows + 1;

                for (int row = 2; row <= rowCount; row++)
                {
                    products.Add(new ProductInputModel
                    {
                        Name = worksheet.Cells[row, 1].Text,
                        Unit = worksheet.Cells[row, 2].Text,
                        PricePerUnit = decimal.Parse(worksheet.Cells[row, 3].Text),
                        Quantity = int.Parse(worksheet.Cells[row, 4].Text)
                    });
                }
            }

            return products;
        }

        public async Task SaveProductsAsync(IEnumerable<ProductInputModel> products)
        {
            var entityProducts = _mapper.Map<List<ProductEntity>>(products);
            await _context.Products.AddRangeAsync(entityProducts);

            await _context.SaveChangesAsync();
        }

        public async Task GroupProductsAsync()
        {
            var productsToProcess = await _context.Products
                .Where(p => !p.IsProcessed)
                .ToListAsync();

            if (productsToProcess.Count == 0)
            {
                return;
            }
            
            List<GroupResultEntity> groupedProducts = new List<GroupResultEntity>();
            GroupResultEntity currentGroup = new GroupResultEntity { Name = $"Group {groupedProducts.Count + 1}" };
            decimal currentGroupTotal = 0;

            productsToProcess.ForEach(product => product.IsProcessed = true);

            _context.Products.RemoveRange(productsToProcess);

            while (productsToProcess.Count > 0)
            {
                var product = productsToProcess[0];
                decimal productTotalPrice = product.PricePerUnit * product.Quantity;

                if (currentGroupTotal + productTotalPrice <= 200)
                {
                    currentGroup.Products.Add(new ProductEntity
                    {
                        Name = product.Name,
                        Unit = product.Unit,
                        PricePerUnit = product.PricePerUnit,
                        Quantity = product.Quantity,
                        IsProcessed = true,
                    });
                    currentGroupTotal += productTotalPrice;

                    productsToProcess.RemoveAt(0);
                }
                else
                {
                    int availableQuantity = (int)((200 - currentGroupTotal) / product.PricePerUnit);

                    if (availableQuantity > 0)
                    {
                        currentGroup.Products.Add(new ProductEntity
                        {
                            Name = product.Name,
                            Unit = product.Unit,
                            PricePerUnit = product.PricePerUnit,
                            Quantity = availableQuantity,
                            IsProcessed = true,
                        });
                        currentGroupTotal += availableQuantity * product.PricePerUnit;

                        product.Quantity -= availableQuantity;

                        if (product.Quantity == 0)
                        {
                            productsToProcess.RemoveAt(0);
                        }
                    }
                    else
                    {
                        if (currentGroup.Products.Count > 0)
                        {
                            currentGroup.TotalPrice = currentGroupTotal;
                            groupedProducts.Add(currentGroup);

                            currentGroup = new GroupResultEntity { Name = $"Group {groupedProducts.Count + 1}" };
                            currentGroupTotal = 0;
                        }
                        else
                        {
                            currentGroup.Products.Add(new ProductEntity
                            {
                                Name = product.Name,
                                Unit = product.Unit,
                                PricePerUnit = product.PricePerUnit,
                                Quantity = product.Quantity,
                                IsProcessed = true,
                            });
                            currentGroupTotal += productTotalPrice;

                            productsToProcess.RemoveAt(0);
                        }
                    }
                }
            }

            if (currentGroup.Products.Count > 0)
            {
                currentGroup.TotalPrice = currentGroupTotal;
                groupedProducts.Add(currentGroup);
            }
            _context.GroupResults.AddRange(groupedProducts);

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<GroupResultModel>> GetGroupResults()
        {
            var groupResults = await _context.GroupResults
                .Select(gr => new GroupResultModel
                {
                    GroupName = gr.Name,
                    TotalCost = gr.TotalPrice
                })
                .ToListAsync();

            return groupResults;
        }

        public async Task<GroupProductResultModel> GetProductsByGroupId(string id)
        {
            var groupResults = await _context.GroupResults
                .Include(gr => gr.Products)
                .ToListAsync();

            var groupResult = groupResults.FirstOrDefault(
                g => id == ExtractNumberFromEnd(g.Name));

            if (groupResult == null)
            {
                return null;
            }

            var groupProductResult = new GroupProductResultModel
            {
                GroupName = groupResult.Name,
                ProductInputModels = groupResult.Products.Select(p => new ProductInputModel
                {
                    Name = p.Name,
                    Unit = p.Unit,
                    PricePerUnit = p.PricePerUnit,
                    Quantity = p.Quantity
                }).ToList()
            };

            return groupProductResult;
        }

        private string ExtractNumberFromEnd(string input)
        {
            var match = Regex.Match(input, @"(\d+)$");

            return match.Success ? match.Value : string.Empty;
        }
    }
}

