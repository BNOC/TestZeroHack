namespace TestZeroSubscriptionService.Models
{
    public class Product
    {
        public int Id { get; set; }
        public decimal Price { get; set; } = 1.0m;
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; } 

        public bool IsActive { get; set; }
    }
}
