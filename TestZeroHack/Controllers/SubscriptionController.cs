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
            Path = @"C:\Users\Imsea\source\repos\TestProjects\TestZero\TestZeroHack\TestZeroHack\Database\Subscriptions.json";
            JsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
        }

        public Subscription Setup(string customerId)
        {
            var subscription = new Subscription()
            {
                Id = 1, 
                CustomerId = customerId,
                Products= new List<Product>() { 
                    new Product() { Id = 1, Name="Product1", Price=2.0m }, 
                    new Product() { Id = 2, Name="Product2", Price=5.0m }, 
                    new Product() { Id = 3, Name="Product3", Price=13.0m }              
                }, 
                NextDueDate = DateTime.UtcNow.AddDays(1).Date, // Setup schedule for tomorrow
                NextPaymentAttemptDate = DateTime.UtcNow.AddDays(1).Date // same as it is first attempt
            };

            return subscription;
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
            var newDate = updatedSub.NextDueDate.AddDays(1);
            updatedSub.NextDueDate = newDate;
            updatedSub.NextPaymentAttemptDate = newDate;

            // record subscription
            RecordSubscription(updatedSub);
            var updatedSubscriptions = JsonSerializer.Serialize(subscriptions, JsonOptions);

            File.WriteAllText(Path, updatedSubscriptions);


            return JsonSerializer.Deserialize<List<Subscription>>(readText).Where(x => x.Id == subscriptionId).FirstOrDefault();
        }
    }
}
