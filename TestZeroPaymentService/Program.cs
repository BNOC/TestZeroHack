// Program setup
using IClock;
using TestZeroPaymentService.Controllers;
using TestZeroPaymentService.Models;
using TestZeroRecordService.Controllers;

Console.WriteLine("Running TestZeroPaymentService");

var paymentsController = new PaymentController();
var recordController = new RecordController();

paymentsController.RecurringPayments();