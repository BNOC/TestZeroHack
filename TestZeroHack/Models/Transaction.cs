using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestZeroHack.Models
{
    public class Transaction
    {
        // Subscription
        public int SubscriptionId { get; set; }
        public string CustomerId { get; set; }

        // Braintree transaction
        public decimal? Amount { get; set; }
        public string TransactionId { get; set; }   
        public string GraphQLId { get; set; }
        public string ProcessorResponseText { get; set; }
        public string NetworkTransactionId { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
