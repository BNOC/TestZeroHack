using IClock;
using TestZeroHack.Controllers;
using TestZeroHack.Models;
using TestZeroPaymentService.Controllers;
using TestZeroPaymentService.Models;
using TestZeroRecordService.Controllers;
using TestZeroSubscriptionService.Controllers;
using TestZeroSubscriptionService.Models;

Console.WriteLine("Running TestZeroHack");

// Program setup
var subscriptionController = new SubscriptionController();
var paymentController = new PaymentController();
var customerController = new CustomerController();
var recordController = new RecordController();

Console.WriteLine("Welcome to TestZeroHack.");
Console.WriteLine("What would you like to do:");
Console.WriteLine("[I]nitial payment");
Console.WriteLine("[D]eferred payment");
Console.WriteLine("[R]eccuring payment");
Console.WriteLine("[C]ancel process");
var answer = Console.ReadLine()?.ToUpper();

if (answer != null)
{
    switch (answer)
    {
        case "I":
            TakeInitialPayment();
            break;
        case "D":
            TakeInitialDeferredPayment();
            break;
        case "R": 
            paymentController.RecurringPayments();
            break;
        case "C":
            CancellationProcess();
            break;
        default:
            throw new Exception();
    }
}

void TakeInitialPayment()
{
    #region Journey Setup + Braintree Customer hack
    // Setup JourneyDetails model as if we went through the flow
    // Do this with console.readline and output product options etc if we want to go harder with it
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
}


#region InitialDeferredPayment
// SK -This was setup in the original branch, needs rework - Probably wont work.
// I've just added the journeyDetails here as well which was a change to the Customer.Setup method I made yesterday, might need some more work but you could just comment this method out to build.
void TakeInitialDeferredPayment()
{
    // Setup JourneyDetails model as if we went through the flow
    // Do this with console.readline and output product options etc if we want to go harder with it
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

    // Setup a customer
    var newCustomer = customerController.Setup(journeyDetails);

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

        if (newPaymentResult.IsSuccess())
        {
            // Record subscription
            recordController.RecordSubscription(newSubscription);
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
void CancellationProcess()
{
    // Cancel day before payment due

    var subscription = subscriptionController.Setup("81827778434");

    Console.WriteLine($"Do you want to cancel this subscription? Access to {subscription.Products.Count} products will end on {subscription.PeriodEndDate}");
    var answer = Console.ReadLine();
    if (answer == "1")
    {
        subscription.NextPaymentAttemptDate = null;
    }
    else if (answer == "2")
    {
        var insuranceProduct = subscription.Products.Where(x => x.ProductType == ProductType.Insurance).First();
        var productType = insuranceProduct.ProductType.ToString();
        insuranceProduct.PeriodEndDate = DateTime.UtcNow.Date;
        insuranceProduct.IsActive = false;
        recordController.UpdateSubscriptionRecord(subscription);
    }

    var newDate = subscription.NextPaymentAttemptDate == null ? "null" : subscription.NextPaymentAttemptDate.ToString();
    Console.WriteLine($"Next payment date has been updated to {newDate}");
}
#endregion Cancellation



//// ---------------------------------------------------- + Insurance

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



