using IClock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TestZeroHack.Models;

namespace TestZeroHack.Controllers
{
    public class SubscriptionController
    {
        public string Path;
        public JsonSerializerOptions JsonOptions;
        public SubscriptionController()
        {
            Path = @"C:\Users\SeanKeenan\Source\Repos\TestZeroHack\TestZeroHack\Database\Subscriptions.json";
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

        // Write to db
        public void RecordSubscription(Subscription subscription)
        {
            if (!File.Exists(Path))
            {
                File.Create(Path);
            }

            // Get existing records and update
            string readText = File.ReadAllText(Path);

            var subscriptions = JsonSerializer.Deserialize<List<Subscription>>(readText);

            // If it doesn't exist already add it
            if(subscriptions.Where(x => x.Id == subscription.Id).Any())
            {
                subscriptions.Add(subscription);
            }

            // Serialize and record
            var updatedSubscriptions = JsonSerializer.Serialize(subscriptions, JsonOptions);

            File.WriteAllText(Path, updatedSubscriptions);
        }

        // Read db and get list of subscriptions where payment is due
        public List<Subscription> GetDuePayments(TestClock tc)
        {
            string readText = File.ReadAllText(Path);

            var subscriptions = JsonSerializer.Deserialize<List<Subscription>>(readText);
            
            return subscriptions.Where(x => x.NextPaymentAttemptDate == tc.GetTime().Date).ToList();
        }

        // Update subscription after successful renewal 
        public Subscription UpdateSubscriptionRecord(int subscriptionId)
        {
            string readText = File.ReadAllText(Path);

            var subscriptions = JsonSerializer.Deserialize<List<Subscription>>(readText);

            var updatedSub = subscriptions.Where(x => x.Id == subscriptionId).FirstOrDefault();

            // Just add a day for now
            var newDate = updatedSub.PeriodEndDate.AddMonths(1);
            updatedSub.PeriodEndDate = newDate;
            updatedSub.NextPaymentAttemptDate = updatedSub.PeriodEndDate.AddDays(-8);

            // record subscription
            RecordSubscription(updatedSub);
            var updatedSubscriptions = JsonSerializer.Serialize(subscriptions, JsonOptions);

            File.WriteAllText(Path, updatedSubscriptions);


            return JsonSerializer.Deserialize<List<Subscription>>(readText).Where(x => x.Id == subscriptionId).FirstOrDefault();
        }
    }
}
