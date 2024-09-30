namespace ProductsService.BusinessLogic.Responses
{
    public class ProductInputModel
    {
        public string Name { get; set; }
        public string Unit { get; set; }
        public decimal PricePerUnit { get; set; }
        public int Quantity { get; set; }
    }
}
