// Program setup
using IClock;
using TestZeroPaymentService.Controllers;
using TestZeroPaymentService.Models;
using TestZeroRecordService.Controllers;

Console.WriteLine("Running TestZeroPaymentService");

var paymentsController = new PaymentController();
var recordController = new RecordController();


var daysFromTodayToAttempt = 22;

#region RecurringPayments
// Set system date to whatever is relevant for testing
var tc = new TestClock(DateTime.UtcNow.AddDays(daysFromTodayToAttempt));
// Get all payments from the sub db where the NextPaymentDate matches
var duePayments = paymentsController.GetDuePayments(tc);

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
            Price = duePayments[i].Products.Sum(x => x.Price),
            PaymentMethodNonce = "fake-valid-nonce",
        });
    }
    // Process payments
    var paymentResults = paymentsController.ProcessDuePayments(duePaymentRequests);
}
#endregion RecurringPayments