using Braintree;
using IClock;
using System.Text.Json;
using System.Web.Http;
using TestZeroPaymentService.Models;
using TestZeroSubscriptionService.Controllers;

namespace TestZeroPaymentService.Controllers
{
    public class PaymentController
    {
        private readonly SubscriptionController _subscriptionController;
        private readonly BraintreeConfiguration _braintreeConfig;

        public PaymentController()
        {
            _subscriptionController = new SubscriptionController();
            _braintreeConfig = new BraintreeConfiguration();
        }
        
        [HttpGet, Route("GenerateToken")]
        public object GenerateToken()
        {
            var gateway = _braintreeConfig.GetGateway();
            var clientToken = gateway.ClientToken.Generate();
            return clientToken;
        }

        /// <summary>
        /// Braintree Checkout
        /// </summary>
        /// <param name="model"></param>
        /// <returns>string "Succeded" or error messages</returns>
        [HttpPost, Route("Checkout")]
        public Result<Braintree.Transaction> Checkout(PaymentRequest model)
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

            Result<Braintree.Transaction> result = gateway.Transaction.Sale(request);
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


            return result;
        }

        /// <summary>
        /// Take an initial payment - Mocks an existing customer and assumes product selection have been provided
        /// In the real world, the Braintree Customer record will be created by Braintree as part of the payment etc.
        /// </summary>
        /// <param name="customer">Mocked Customer record</param>
        /// <param name="details">Simulate journey detail gathering</param>
        /// <param name="shouldFail">Should this attempt fail?</param>
        public Result<Braintree.Transaction> InitialPaymentProcess(PaymentRequest request)
        {
            // Attempt payment and return result
            return this.Checkout(request);
        }

        /// <summary>
        /// Reads Subscription DB (JSON) and returns a list of subs where the NextPaymentDate matches the TestClock
        /// </summary>
        /// <param name="tc"></param>
        /// <returns></returns>
        public List<TestZeroSubscriptionService.Models.Subscription> GetDuePayments(TestClock tc)
        {
            string readText = File.ReadAllText(_subscriptionController.Path);
            var subscriptions = JsonSerializer.Deserialize<List<TestZeroSubscriptionService.Models.Subscription>>(readText);

            return subscriptions.Where(x => x.NextPaymentAttemptDate.Value.Date == tc.GetTime().Date).ToList();
        }

        /// <summary>
        /// Processes due payments
        /// </summary>
        /// <param name="duePaymentRequests"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public void ProcessDuePayments(List<PaymentRequest> duePaymentRequests)
        {
            // Attempt payment
            foreach (var duePaymentRequest in duePaymentRequests)
            {
                try
                {
                    var newPaymentResult = this.Checkout(duePaymentRequest);

                    if (newPaymentResult.IsSuccess())
                    {
                        // Record payment record
                        // update nextpaymentdate
                        //var newSub = subscriptionController.UpdateSubscriptionRecord(duePaymentRequest.SubscriptionId);
                        //Console.WriteLine("UpdatedSub PeriodEndDate should be +1m ahead of the last " + newSub.PeriodEndDate.AddMonths(1));
                        //Console.WriteLine("UpdatedSub NextPaymentDate should be +1m-8 ahead of the last " + newSub.PeriodEndDate.AddMonths(1).AddDays(-8));
                    }
                    else
                    {
                        Console.WriteLine("Something went wrong processing the renewal, try again.");

                        // Handle failures, will need to change the nonce to other fake nonces and make adjustments in a few places
                        // https://developer.paypal.com/braintree/docs/guides/recurring-billing/testing-go-live/dotnet/
                        // Send them to UI to make a payment
                        // Go to braintree and make payment
                        Console.WriteLine("We're in here");

                        duePaymentRequest.PaymentMethodNonce = "fake-valid-nonce";
                        newPaymentResult = this.Checkout(duePaymentRequest);

                        if (newPaymentResult.IsSuccess())
                        {
                            // Record payment record
                            // update nextpaymentdate
                            //var newSub = subscriptionController.UpdateSubscriptionRecord(duePaymentRequest.SubscriptionId);
                            //Console.WriteLine("UpdatedSub PeriodEndDate should be +1m ahead of the last " + newSub.PeriodEndDate.AddMonths(1));
                            //Console.WriteLine("UpdatedSub NextPaymentDate should be +1m-8 ahead of the last " + newSub.PeriodEndDate.AddMonths(1).AddDays(-8));
                        }
                    }
                }
                catch (Exception x)
                {
                    Console.WriteLine(x);
                }
            }
        }
    }
}
