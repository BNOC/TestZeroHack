namespace TestZeroPaymentService.Models
{
    public class PaymentRequest
    {
        public int SubscriptionId { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public decimal Price { get; set; } = 1.0m;
        public string PaymentMethodNonce { get; set; } = "fake-valid-nonce";
    }
}
