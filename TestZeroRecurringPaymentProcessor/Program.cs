// Program setup
using IClock;
using TestZeroPaymentService.Controllers;
using TestZeroPaymentService.Models;

var duePaymentsController = new PaymentController();

var daysFromTodayToAttempt = 23;

#region RecurringPayments
// Set system date to whatever is relevant for testing
var tc = new TestClock(DateTime.UtcNow.AddDays(daysFromTodayToAttempt));
// Get all payments from the sub db where the NextPaymentDate matches
var duePayments = duePaymentsController.GetDuePayments(tc);

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
duePaymentsController.ProcessDuePayments(duePaymentRequests);
// Update sub records with new dates

#endregion RecurringPayments