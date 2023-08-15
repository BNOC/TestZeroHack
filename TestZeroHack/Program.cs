using IClock;
using TestZeroHack.Controllers;
using TestZeroHack.Models;

Console.WriteLine("Hello");

// Program setup
var subscriptionController = new SubscriptionController();
var paymentController = new PaymentController();
var customerController = new CustomerController();

// Test 
var takingInitialPayment = false;
var takingInitialDeferredPayment = false;
var attemptingRecurring = false;
var attemptingRecurringInsurance = false;
var cancelJustInsuranceProduct = false;
var daysFromTodayToAttempt = 1;

#region InitialPayment
if (takingInitialPayment)
{
    // Setup a customer
    var newCustomer = customerController.Setup();

    // Setup a subscription, select products etc
    var newSubscription = subscriptionController.Setup(newCustomer.Id);

    // Handle initial payment
    //// Setup paymentRequest
    PaymentRequest newPayment = new()
    {
        CustomerId = newSubscription.CustomerId,
        Price = newSubscription.Products.Sum(x => x.Price),
        PaymentMethodNonce = "fake-valid-nonce"
    };

    //// Attempt payment
    try
    {
        var newPaymentResult = paymentController.Checkout(newPayment);

        if ((string)newPaymentResult == "Succeded")
        {
            // Record subscription
            subscriptionController.RecordSubscription(newSubscription);
        }
        else
        {
            Console.WriteLine("Something went wrong processing the payment, try again.");
            // Not using this for batch processing so no need to handle errors here for now
        }
    }
    catch (Exception x)
    {
        Console.WriteLine(x);
    }
}
#endregion InitialPayment

#region InitialDeferredPayment
if (takingInitialDeferredPayment)
{

    // Setup a customer
    var newCustomer = customerController.Setup();

    // Setup a subscription, select products etc
    var newSubscription = subscriptionController.Setup(newCustomer.Id, 10);
    var proRataPrice = subscriptionController.ProRataProducts(newSubscription);

    var totalPrice = proRataPrice + newSubscription.Products.Sum(x => x.Price);

    // Handle initial payment
    //// Setup paymentRequest
    PaymentRequest newPayment = new()
    {
        CustomerId = newSubscription.CustomerId,
        Price = totalPrice,
        PaymentMethodNonce = "fake-valid-nonce"
    };

    //// Attempt payment
    try
    {
        var newPaymentResult = paymentController.Checkout(newPayment);

        if ((string)newPaymentResult == "Succeded")
        {
            // Record subscription
            subscriptionController.RecordSubscription(newSubscription);
        }
        else
        {
            Console.WriteLine("Something went wrong processing the payment, try again.");
            // Not using this for batch processing so no need to handle errors here for now
        }
    }
    catch (Exception x)
    {
        Console.WriteLine(x);
    }
}
#endregion InitialDeferredPayment

#region Cancellation
//// Cancel day before payment due

//var subscription = subscriptionController.Setup("81827778434");

//Console.WriteLine($"Do you want to cancel this subscription? Access to {subscription.Products.Count} products will end on {subscription.PeriodEndDate}");
//var answer = Console.ReadLine();
//if (answer == "1")
//{
//    subscription.NextPaymentAttemptDate = null;
//}

//var newDate = subscription.NextPaymentAttemptDate == null ? "null" : subscription.NextPaymentAttemptDate.ToString();
//Console.WriteLine($"Next payment date has been updated to {newDate}");
#endregion Cancellation

#region RecurringPayments
if (attemptingRecurring)
{
    var tc = new TestClock(DateTime.UtcNow.AddDays(daysFromTodayToAttempt));
    var duePayments = subscriptionController.GetDuePayments(tc);


    List<PaymentRequest> duePaymentsRequestList = new();
    for (var i = 0; i < duePayments.Count; i++)
    {
        duePaymentsRequestList.Add(new PaymentRequest()
        {
            SubscriptionId = duePayments[i].Id,
            CustomerId = duePayments[i].CustomerId,
            Price = duePayments[i].Products.Sum(x => x.Price),
            PaymentMethodNonce = "fake--nonce",
        });
    }

    Console.WriteLine(duePaymentsRequestList.Count + "dprl");

    // Attempt payment
    foreach (var duePaymentRequest in duePaymentsRequestList)
    {
        try
        {
            var newPaymentResult = paymentController.Checkout(duePaymentRequest);

            if ((string)newPaymentResult == "Succeded")
            {
                // Record payment record
                // update nextpaymentdate
                var newSub = subscriptionController.UpdateSubscriptionRecord(duePaymentRequest.SubscriptionId);
                Console.WriteLine("UpdatedSub PeriodEndDate should be +1m ahead of the last " + newSub.PeriodEndDate.AddMonths(1));
                Console.WriteLine("UpdatedSub NextPaymentDate should be +1m-8 ahead of the last " + newSub.PeriodEndDate.AddMonths(1).AddDays(-8));
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
                var newPaymentResult2 = paymentController.Checkout(duePaymentRequest);

                Console.WriteLine("One off payment " + newPaymentResult2);

                if ((string)newPaymentResult2 == "Succeded")
                {
                    // Record payment record
                    // update nextpaymentdate
                    var newSub = subscriptionController.UpdateSubscriptionRecord(duePaymentRequest.SubscriptionId);
                    Console.WriteLine("UpdatedSub PeriodEndDate should be +1m ahead of the last " + newSub.PeriodEndDate.AddMonths(1));
                    Console.WriteLine("UpdatedSub NextPaymentDate should be +1m-8 ahead of the last " + newSub.PeriodEndDate.AddMonths(1).AddDays(-8));
                }
            }
        }
        catch (Exception x)
        {
            Console.WriteLine(x);
        }
    }
}
#endregion RecurringPayments

// ---------------------------------------------------- + Insurance

#region RecurringPaymentsInsurance
// Attempt payment
if (attemptingRecurringInsurance)
{
    var tc = new TestClock(DateTime.UtcNow.AddDays(daysFromTodayToAttempt));
    var duePayments = subscriptionController.GetDuePayments(tc);

    var duePaymentsThatHaveInsurance = new List<Subscription>();
    foreach(var due in duePayments)
    {
        if (due.Products.Where(x => x.Name == "Insurance").Any())
        {
            duePaymentsThatHaveInsurance.Add(due);
        }
    }


    List<PaymentRequest> duePaymentsRequestList = new();
    for (var i = 0; i < duePaymentsThatHaveInsurance.Count; i++)
    {
        duePaymentsRequestList.Add(new PaymentRequest()
        {
            SubscriptionId = duePaymentsThatHaveInsurance[i].Id,
            CustomerId = duePaymentsThatHaveInsurance[i].CustomerId,
            Price = duePaymentsThatHaveInsurance[i].Products.Sum(x => x.Price),
            PaymentMethodNonce = "fake-valid-nonce",
        });
    }

    foreach (var duePaymentRequest in duePaymentsRequestList)
    {
        try
        {
            var newPaymentResult = paymentController.Checkout(duePaymentRequest);

            if ((string)newPaymentResult == "Succeded")
            {
                // Record payment record
                // update nextpaymentdate
                //var newSub = subscriptionController.UpdateSubscriptionRecord(duePaymentRequest.SubscriptionId);

                Console.WriteLine("Processed payment");
            }
            else
            {
                Console.WriteLine("Something went wrong processing the renewal, try again.");

            }
        }
        catch (Exception x)
        {
            Console.WriteLine(x);
        }
    }
}

#endregion

#region CancelJustInsuranceProduct 
if (cancelJustInsuranceProduct)
{
    // Cancel day before payment due

    var justInsuranceSubs = subscriptionController.GetSubsContainingInsurance();
    var singleInsuranceSub = justInsuranceSubs.Single();

    Console.WriteLine($"Do you want to cancel your insurance?");
    var answer = Console.ReadLine();
    if (answer == "1")
    {
        var insuranceProduct = singleInsuranceSub.Products.Where(x => x.Name == "Insurance").First();
        insuranceProduct.IsActive = false;


    }

    // Update record in json file
    // test recurring payment again for updated price

    Console.WriteLine($"Next price is ...");
}
#endregion CancelJustInsuranceProduct