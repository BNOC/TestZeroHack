Setup Steps:

1. Open the RecordController in the TestZeroRecordService and update the path to your mocked DB files
they sit under TestZeroHack.Database, there are 2 files, Subscriptions and Transactions

2. If there are no records in the Mocked DBs, run the TestZeroHack app first
      - This will create an initial Subscription and transaction record.

3. You're setup.


Recurring Payments steps:

1. Open the TestZeroPaymentService.Program file and update the daysFromTodayToAttempt variable equal to what you expect
      - Look in the Subscriptions.Json file and manually calculate the difference between the NextPaymentDate and today.

2. Run the TestZeroPaymentService app and confirm that the console displays the correct amount of expected duePayments.

3. Check the Subscriptions.Json file again after it's ran to confirm the updated dates.