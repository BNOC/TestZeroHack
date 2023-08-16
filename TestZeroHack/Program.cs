using IClock;
using TestZeroHack.Controllers;
using TestZeroHack.Models;
using TestZeroPaymentService.Controllers;
using TestZeroPaymentService.Models;
using TestZeroRecordService.Controllers;
using TestZeroSubscriptionService.Controllers;
using TestZeroSubscriptionService.Models;

Console.WriteLine("Hello");

// Program setup
var subscriptionController = new SubscriptionController();
var paymentController = new PaymentController();
var customerController = new CustomerController();
var recordController = new RecordController();


#region Journey Setup + Braintree Customer hack
// Setup JourneyDetails model as if we went through the flow
// Do this with console.readline and output product options if we want to go harder with it
var journeyDetails = new JourneyDetails()
{
    FirstName = "Sean",
    LastName = "Keenan",
    Email = "Sean.Keenan@bennetts.co.uk",
    Products = new List<Product>()
    {
        new Product() { Id = 1, Name="Setup", Price=2.0m, StartDate = DateTime.UtcNow, IsActive = true },
        new Product() { Id = 2, Name="Charge", Price=5.0m, StartDate = DateTime.UtcNow, IsActive = true },
        new Product() { Id = 3, Name="AProduct", Price=13.0m, StartDate = DateTime.UtcNow, IsActive = true },
    }
};

// Setup a customer to use in Braintree
var customer = customerController.Setup(journeyDetails);
#endregion

#region After Journey, process initial payment and record the sub & transaction.
// Build payment request
bool shouldPaymentFail = false;
PaymentRequest newPaymentRequest = new()
{
    Price = journeyDetails.Products.Sum(x => x.Price),
    PaymentMethodNonce = shouldPaymentFail ? "failure-nonce" : "fake-valid-nonce"
};
var paymentStatus = paymentController.InitialPaymentProcess(newPaymentRequest);

if (paymentStatus.IsSuccess())
{
    // Create Subscription
    var subscription = subscriptionController.CreateSubscription(customer, journeyDetails.Products);
    // Record Subscription
    recordController.RecordSubscription(subscription);
    // Record Transaction
    recordController.RecordTransaction(paymentStatus.Target, subscription);
}
#endregion


#region InitialDeferredPayment
//if (takingInitialDeferredPayment)
//{

//    // Setup a customer
//    var newCustomer = customerController.Setup();

//    // Setup a subscription, select products etc
//    var newSubscription = subscriptionController.Setup(newCustomer.Id, 10);
//    var proRataPrice = subscriptionController.ProRataProducts(newSubscription);

//    var totalPrice = proRataPrice + newSubscription.Products.Sum(x => x.Price);

//    // Handle initial payment
//    //// Setup paymentRequest
//    PaymentRequest newPayment = new()
//    {
//        CustomerId = newSubscription.CustomerId,
//        Price = totalPrice,
//        PaymentMethodNonce = "fake-valid-nonce"
//    };

//    //// Attempt payment
//    try
//    {
//        var newPaymentResult = paymentController.Checkout(newPayment);

//        if ((string)newPaymentResult == "Succeded")
//        {
//            // Record subscription
//            subscriptionController.RecordSubscription(newSubscription);
//        }
//        else
//        {
//            Console.WriteLine("Something went wrong processing the payment, try again.");
//            // Not using this for batch processing so no need to handle errors here for now
//        }
//    }
//    catch (Exception x)
//    {
//        Console.WriteLine(x);
//    }
//}
#endregion InitialDeferredPayment









#region Cancellation
////// Cancel day before payment due

////var subscription = subscriptionController.Setup("81827778434");

////Console.WriteLine($"Do you want to cancel this subscription? Access to {subscription.Products.Count} products will end on {subscription.PeriodEndDate}");
////var answer = Console.ReadLine();
////if (answer == "1")
////{
////    subscription.NextPaymentAttemptDate = null;
////}

////var newDate = subscription.NextPaymentAttemptDate == null ? "null" : subscription.NextPaymentAttemptDate.ToString();
////Console.WriteLine($"Next payment date has been updated to {newDate}");
#endregion Cancellation



//// ---------------------------------------------------- + Insurance

#region RecurringPaymentsInsurance
//// Attempt payment
//if (attemptingRecurringInsurance)
//{
//    var tc = new TestClock(DateTime.UtcNow.AddDays(daysFromTodayToAttempt));
//    var duePayments = subscriptionController.GetDuePayments(tc);

//    var duePaymentsThatHaveInsurance = new List<Subscription>();
//    foreach(var due in duePayments)
//    {
//        if (due.Products.Where(x => x.Name == "Insurance").Any())
//        {
//            duePaymentsThatHaveInsurance.Add(due);
//        }
//    }


//    List<PaymentRequest> duePaymentsRequestList = new();
//    for (var i = 0; i < duePaymentsThatHaveInsurance.Count; i++)
//    {
//        duePaymentsRequestList.Add(new PaymentRequest()
//        {
//            SubscriptionId = duePaymentsThatHaveInsurance[i].Id,
//            CustomerId = duePaymentsThatHaveInsurance[i].CustomerId,
//            Price = duePaymentsThatHaveInsurance[i].Products.Sum(x => x.Price),
//            PaymentMethodNonce = "fake-valid-nonce",
//        });
//    }

//    foreach (var duePaymentRequest in duePaymentsRequestList)
//    {
//        try
//        {
//            var newPaymentResult = paymentController.Checkout(duePaymentRequest);

//            if ((string)newPaymentResult == "Succeded")
//            {
//                // Record payment record
//                // update nextpaymentdate
//                //var newSub = subscriptionController.UpdateSubscriptionRecord(duePaymentRequest.SubscriptionId);

//                Console.WriteLine("Processed payment");
//            }
//            else
//            {
//                Console.WriteLine("Something went wrong processing the renewal, try again.");

//            }
//        }
//        catch (Exception x)
//        {
//            Console.WriteLine(x);
//        }
//    }
//}

#endregion

#region CancelJustInsuranceProduct 
//if (cancelJustInsuranceProduct)
//{
//    // Cancel day before payment due

//    var justInsuranceSubs = subscriptionController.GetSubsContainingInsurance();
//    var singleInsuranceSub = justInsuranceSubs.Single();

//    Console.WriteLine($"Do you want to cancel your insurance?");
//    var answer = Console.ReadLine();
//    if (answer == "1")
//    {
//        var insuranceProduct = singleInsuranceSub.Products.Where(x => x.Name == "Insurance").First();
//        insuranceProduct.IsActive = false;


//    }

//    // Update record in json file
//    // test recurring payment again for updated price

//    Console.WriteLine($"Next price is ...");
//}
#endregion CancelJustInsuranceProduct



