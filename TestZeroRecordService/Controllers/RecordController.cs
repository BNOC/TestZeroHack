using System.Text.Json;
using TestZeroRecordService.Models;
using TestZeroSubscriptionService.Models;

namespace TestZeroRecordService.Controllers
{
    public class RecordController
    {
        private readonly string SubscriptionsRecordPath;
        private readonly string TransactionsRecordPath;
        public JsonSerializerOptions JsonOptions;

        public RecordController()
        {
            SubscriptionsRecordPath = @"C:\Users\Imsea\source\repos\TestProjects\TestZero\TestZeroHack\TestZeroHack\Database\Subscriptions.json";
            TransactionsRecordPath = @"C:\Users\Imsea\source\repos\TestProjects\TestZero\TestZeroHack\TestZeroHack\Database\Transactions.json";
            JsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
        }

        #region Subscriptions
        public void RecordSubscription(Subscription subscription)
        {
            if (!File.Exists(SubscriptionsRecordPath))
            {
                File.Create(SubscriptionsRecordPath);
            }

            // Read db file
            string readText = File.ReadAllText(SubscriptionsRecordPath);

            // If the file doesn't contain any records already
            if (readText != string.Empty)
            {
                var subscriptions = JsonSerializer.Deserialize<List<Subscription>>(readText);
                
                // If a record for this sub doesn't exist already add it
                if (!subscriptions.Where(x => x.Id == subscription.Id).Any())
                {
                    subscriptions.Add(subscription);
                }
                else
                {
                    // Update existing record
                    UpdateSubscriptionRecord(subscription.Id);

                    // Re-read the file and deserialize it
                    readText = File.ReadAllText(SubscriptionsRecordPath);
                    subscriptions = JsonSerializer.Deserialize<List<Subscription>>(readText);
                }

                // Serialize result and record
                var updatedSubscriptions = JsonSerializer.Serialize(subscriptions, JsonOptions);

                File.WriteAllText(SubscriptionsRecordPath, updatedSubscriptions);
            }
            else
            {
                // If nothing exists in the file, pass in single record as a list so it creates the right structure in JSON file
                var subscriptionList = new List<Subscription>
                {
                    subscription
                };
                var newSubscriptionRecord = JsonSerializer.Serialize(subscriptionList, JsonOptions);
                File.WriteAllText(SubscriptionsRecordPath, newSubscriptionRecord);
            }
            
        }

        // Update an existing subscription record
        public void UpdateSubscriptionRecord(int subscriptionId)
        {
            string readText = File.ReadAllText(SubscriptionsRecordPath);

            var subscriptions = JsonSerializer.Deserialize<List<Subscription>>(readText);

            var updatedSub = subscriptions.Where(x => x.Id == subscriptionId).FirstOrDefault();

            // Do whatever in here but moving dates as calling on recurring payment success
            var newDate = updatedSub.PeriodEndDate.AddMonths(1);
            updatedSub.PeriodEndDate = newDate;
            updatedSub.NextPaymentAttemptDate = updatedSub.PeriodEndDate.AddDays(-8);
            

            var updatedSubscriptions = JsonSerializer.Serialize(subscriptions, JsonOptions);

            File.WriteAllText(SubscriptionsRecordPath, updatedSubscriptions);
        }
        #endregion Subscriptions

        #region Transactions
        public void RecordTransaction(Braintree.Transaction transaction, Subscription subscription)
        {
            if (!File.Exists(TransactionsRecordPath))
            {
                File.Create(TransactionsRecordPath);
            }
            // Create transaction model
            var newTransaction = new Transaction()
            {
                SubscriptionId = subscription.Id,
                CustomerId = subscription.CustomerId,
                Amount = transaction.Amount,
                GraphQLId = transaction.GraphQLId,
                TransactionId = transaction.Id,
                CreatedAt = transaction.CreatedAt,
                NetworkTransactionId = transaction.NetworkTransactionId,
                ProcessorResponseText = transaction.ProcessorResponseText
            };

            // Read db file
            string readText = File.ReadAllText(TransactionsRecordPath);

            // If the db isn't empty
            if (readText != string.Empty)
            {
                var transactions = JsonSerializer.Deserialize<List<Transaction>>(readText);

                // Contents not empty, write new record to the array
                transactions.Add(newTransaction);

                // Serialize result and record
                var updatedTransactions = JsonSerializer.Serialize(transactions, JsonOptions);

                File.WriteAllText(TransactionsRecordPath, updatedTransactions);
            }
            else
            {
                // If nothing exists in the file, pass in single record as a list so it creates the right structure in JSON file
                var transactionsList = new List<Transaction>
                {
                    newTransaction
                };
                var newSubscriptionRecord = JsonSerializer.Serialize(transactionsList, JsonOptions);
                File.WriteAllText(TransactionsRecordPath, newSubscriptionRecord);
            }
        }
        #endregion Transactions
    }
}
