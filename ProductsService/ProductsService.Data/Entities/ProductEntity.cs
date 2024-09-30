namespace ProductsService.Data.Entities
{
    public class ProductEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public decimal PricePerUnit { get; set; }
        public int Quantity { get; set; }
        public bool IsProcessed { get; set; } = false;

        public int? GroupResultId { get; set; }
        public GroupResultEntity? GroupResult { get; set; }
    }
}
