﻿using IClock;
using TestZeroHack.Controllers;
using TestZeroHack.Models;

Console.WriteLine("Hello");

// Program setup
var subscriptionController = new SubscriptionController();
var paymentController = new PaymentController();
var customerController = new CustomerController();

// Test 
var takingInitialPayment = false;
var attemptingRecurring = true;
var daysFromTodayToAttempt = 3;

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

#region RecurringPayments
// Recurring Payments
// Manipulate the date to +1
// Find all payments due from json file with todays date
// Process payment
// update schedule against subscription
if (attemptingRecurring) { 
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
            PaymentMethodNonce = "fake-valid-nonce",
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
                Console.WriteLine("UpdatedSub should be +1d ahead of the last " + newSub.NextDueDate.AddDays(1));
            }
            else
            {
                Console.WriteLine("Something went wrong processing the renewal, try again.");

                // Handle failures, will need to change the nonce to other fake nonces and make adjustments in a few places
                // https://developer.paypal.com/braintree/docs/guides/recurring-billing/testing-go-live/dotnet/
            }
        }
        catch (Exception x)
        {
            Console.WriteLine(x);
        }
    }
}
#endregion RecurringPayments

