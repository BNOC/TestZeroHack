using Braintree;
using System.Web.Http;
using TestZeroHack.Models;

namespace TestZeroHack.Controllers
{
    public class PaymentController
    {
        private readonly BraintreeConfiguration _braintreeConfig;
        
        
        public PaymentController()
        {
            _braintreeConfig = new BraintreeConfiguration();
        }

        [HttpGet, Route("GenerateToken")]
        public object GenerateToken()
        {
            var gateway = _braintreeConfig.GetGateway();
            var clientToken = gateway.ClientToken.Generate();
            return clientToken;
        }

        [HttpPost, Route("Checkout")]
        public object Checkout(PaymentRequest model)
        {
            string paymentStatus = string.Empty;
            var gateway = _braintreeConfig.GetGateway();

            var request = new TransactionRequest
            {
                Amount = model.Price,
                PaymentMethodNonce = model.PaymentMethodNonce,
                Options = new TransactionOptionsRequest
                {
                    SubmitForSettlement = true
                },
                CustomerId = model.CustomerId
            };

            Result<Transaction> result = gateway.Transaction.Sale(request);
            if (result.IsSuccess())
            {
                paymentStatus = "Succeded";
            }
            else
            {
                string errorMessages = "";
                foreach (ValidationError error in result.Errors.DeepAll())
                {
                    errorMessages += "Error: " + (int)error.Code + " - " + error.Message + "\n";
                }

                paymentStatus = errorMessages;
            }


            Console.WriteLine("Payment Status" + paymentStatus);

            return paymentStatus;
        }
    }
}
