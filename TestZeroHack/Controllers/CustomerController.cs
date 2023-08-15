using Braintree;
using System.Web.Http;
using TestZeroHack.Models;
using TestZeroPaymentService.Models;

namespace TestZeroHack.Controllers
{
    public class CustomerController
    {
        private readonly BraintreeConfiguration _braintreeConfig;

        public CustomerController()
        {
            _braintreeConfig= new BraintreeConfiguration();
        } 

        public Customer? Setup(JourneyDetails details)
        {
            // Customer setup
            var request = new CustomerRequest
            {
                FirstName = details.FirstName,
                LastName = details.LastName,
                Email = details.Email,
            };
            Result<Customer> result = _braintreeConfig.GetGateway().Customer.Create(request);

            bool customerSuccess = result.IsSuccess();

            // Card setup
            var creditCardRequest = new CreditCardRequest
            {
                CustomerId = result.Target.Id,
                Number = "4111111111111111",
                ExpirationDate = "10/23",
                CVV = "123"
            };

            Result<CreditCard> creditCardResult = _braintreeConfig.GetGateway().CreditCard.Create(creditCardRequest);

            bool ccSuccess = creditCardResult.IsSuccess();


            // Check customer and card were setup and return the customer if they were
            var customerAndCardSetup = customerSuccess && ccSuccess;

            return customerAndCardSetup ? result.Target : null;
        }
    }
}
