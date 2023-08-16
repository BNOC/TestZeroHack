namespace TestZeroSubscriptionService.Models
{
    public class Product
    {
        public int Id { get; set; }
        public ProductType ProductType {get;set;}
        public decimal Price { get; set; } = 1.0m;
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; } 

        public bool IsActive { get; set; }
        public DateTime PeriodEndDate { get; set; }
    }

    public enum ProductType
    {
        Product = 0,
        Insurance = 1,
    }
}
