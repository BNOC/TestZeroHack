﻿using Braintree;
using IClock;
using System.Text.Json;
using System.Web.Http;
using TestZeroPaymentService.Models;
using TestZeroRecordService.Controllers;
using TestZeroSubscriptionService.Controllers;
using TestZeroSubscriptionService.Models;

namespace TestZeroPaymentService.Controllers
{
    public class PaymentController
    {
        private readonly SubscriptionController _subscriptionController;
        private readonly RecordController _recordController;
        private readonly BraintreeConfiguration _braintreeConfig;

        public PaymentController()
        {
            _subscriptionController = new SubscriptionController();
            _recordController = new RecordController();
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
        public List<TestZeroSubscriptionService.Models.Subscription>? GetDuePayments(TestClock tc)
        {
            string readText = File.ReadAllText(_subscriptionController.Path);
            if (readText != string.Empty)
            {
                var subscriptions = JsonSerializer.Deserialize<List<TestZeroSubscriptionService.Models.Subscription>>(readText);
                return subscriptions.Where(x => x.NextPaymentAttemptDate.Value.Date == tc.GetTime().Date).ToList();
            }

            Console.WriteLine("No payments found, check TestZeroHack.Database.Subscriptions.Json and run the TestZeroHack Program to populate");
            return null;
        }

        /// <summary>
        /// Processes due payments
        /// </summary>
        /// <param name="duePaymentRequests"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public List<Result<Transaction>> ProcessDuePayments(List<PaymentRequest> duePaymentRequests)
        {
            Console.WriteLine($"{duePaymentRequests.Count} payments to process");
            var successfulPayments = 0;

            List<Result<Transaction>> paymentResults = new();

            // Attempt payment
            foreach (var duePaymentRequest in duePaymentRequests)
            {
                paymentResults.Add(this.Checkout(duePaymentRequest));

                // Update sub records with new dates
                foreach (var paymentResult in paymentResults)
                {

                    var subscription = _subscriptionController.Get(duePaymentRequest.SubscriptionId);
                    if (paymentResult.IsSuccess())
                    {
                        successfulPayments++;
                        // UpdateSub
                        _recordController.UpdateSubscriptionRecord(subscription);
                    }

                    // Record Transaction
                    _recordController.RecordTransaction(paymentResult.Target, subscription);
                }
            }

            Console.WriteLine($"{successfulPayments} Successful Payments taken");
            return paymentResults;
        }

        public void RecurringPayments() {
            var daysFromTodayToAttempt = 1;

            #region RecurringPayments
            // Set system date to whatever is relevant for testing
            var tc = new TestClock(DateTime.UtcNow.AddDays(daysFromTodayToAttempt));
            // Get all payments from the sub db where the NextPaymentDate matches
            var duePayments = this.GetDuePayments(tc);

            if (duePayments != null)
            {
                // Create a list of paymentRequests to process
                List<PaymentRequest> duePaymentRequests = new();
                for (var i = 0; i < duePayments.Count; i++)
                {
                    duePaymentRequests.Add(new PaymentRequest()
                    {
                        SubscriptionId = duePayments[i].Id,
                        CustomerId = duePayments[i].CustomerId,
                        Price = duePayments[i].Products.Where(x => x.IsActive).Sum(x => x.Price),
                        PaymentMethodNonce = "fake-valid-nonce",
                    });
                }
                // Process payments
                var paymentResults = this.ProcessDuePayments(duePaymentRequests);
            }
            #endregion RecurringPayments

        }
    }
}
