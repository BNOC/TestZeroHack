using System.Text.Json;
using TestZeroSubscriptionService.Models;

namespace TestZeroSubscriptionService.Controllers
{
    public class SubscriptionController
    {
        public string Path;
        public JsonSerializerOptions JsonOptions;
        public SubscriptionController()
        {
            Path = @"C:\Users\Imsea\source\repos\TestProjects\TestZero\TestZeroHack\TestZeroHack\Database\Subscriptions.json";
            JsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
        }

        public Subscription Setup(string customerId, int deferredStartDays = 0)
        {
            var subscription = new Subscription()
            {
                Id = 1, 
                CustomerId = customerId,
                Products= new List<Product>() { 
                    new Product() { Id = 1, Name="Product1", Price=2.0m, StartDate = DateTime.UtcNow }, 
                    new Product() { Id = 2, Name="Product2", Price=5.0m, StartDate = DateTime.UtcNow }, 
                    new Product() { 
                        Id = 3, Name="Product3", Price=13.0m,
                        StartDate = deferredStartDays != 0 ? DateTime.UtcNow.AddDays(deferredStartDays) : DateTime.UtcNow, 
                    }              
                }, 
                StartDate = DateTime.UtcNow,
            };
            subscription.PeriodEndDate = subscription.Products.OrderByDescending(p => p.StartDate).FirstOrDefault().StartDate.AddMonths(1);
            subscription.NextPaymentAttemptDate = subscription.PeriodEndDate.AddDays(-8);

            return subscription;
        }

        public decimal ProRataProducts(Subscription subscription)
        {
            // get highest start date product
            // compare it against sub start date

            var highestStartDate = subscription.Products.OrderByDescending(p => p.StartDate).FirstOrDefault().StartDate;
            var products = subscription.Products.Where(x => x.StartDate < highestStartDate).ToList();

            var proRataPrice = products.Sum(x => x.Price);


            Console.WriteLine(proRataPrice);

            // sub start date 
            // period end date
            return (((proRataPrice * 12) / 365) * 10);
        }

        public List<Subscription> GetSubsContainingInsurance()
        {
            string readText = File.ReadAllText(Path);

            var subscriptions = JsonSerializer.Deserialize<List<Subscription>>(readText);

            var subsWithInusrnace = new List<Subscription>();
            foreach (var sub in subscriptions)
            {
                if (sub.Products.Where(x => x.Name == "Insurance").Any())
                {
                    subsWithInusrnace.Add(sub);
                }
            }

            return subsWithInusrnace;
        }

        public Subscription CreateSubscription(Braintree.Customer customer, List<Product> products)
        {
            Random rand = new Random();

            return new Subscription()
            {
                Id = rand.Next(1, 1500000), 
                CustomerId = customer.Id,
                Products = products,
                StartDate = DateTime.UtcNow,
                PeriodEndDate = DateTime.UtcNow.AddMonths(1),
                NextPaymentAttemptDate = DateTime.UtcNow.AddMonths(1).AddDays(-8),
            };

        }
    }
}
