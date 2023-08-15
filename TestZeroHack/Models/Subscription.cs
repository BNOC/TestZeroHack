using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestZeroHack.Models
{
    public class Subscription
    {
        public int Id { get; set; }
        public string CustomerId { get; set; } = string.Empty;

        public IList<Product> Products { get; set; } = new List<Product>();

        public DateTime StartDate { get; set; } // Static month -8, every first attempt
        public DateTime PeriodEndDate { get; set; } // Static month -8, every first attempt
        public DateTime? NextPaymentAttemptDate { get; set; } // Attempt, retry

    }
}
