using TestZeroSubscriptionService.Models;

namespace TestZeroHack.Models
{
    public class JourneyDetails
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty; 
        public List<Product> Products { get; set; } = new List<Product>();
    }
}
