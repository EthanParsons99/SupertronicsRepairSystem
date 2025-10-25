namespace SupertronicsRepairSystem.Models.Customer
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public decimal WasPrice { get; set; }
        public int DiscountPercentage { get; set; }
    }
}