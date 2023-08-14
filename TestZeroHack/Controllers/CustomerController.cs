using Braintree;
using System.Web.Http;
using TestZeroHack.Models;

namespace TestZeroHack.Controllers
{
    public class CustomerController
    {
        private readonly BraintreeConfiguration _braintreeConfig;

        public CustomerController()
        {
            _braintreeConfig= new BraintreeConfiguration();
        }

        public Customer Setup()
        {
            var request = new CustomerRequest
            {
                FirstName = "Sean",
                LastName = "Keenan",
                Email = "SK@example.com",
            };
            Result<Customer> result = _braintreeConfig.GetGateway().Customer.Create(request);

            bool success = result.IsSuccess();
            // true

            string customerId = result.Target.Id;
            // e.g. 594019

            var creditCardRequest = new CreditCardRequest
            {
                CustomerId = result.Target.Id,
                Number = "4111111111111111",
                ExpirationDate = "10/23",
                CVV = "123"
            };

            CreditCard creditCardToken = _braintreeConfig.GetGateway().CreditCard.Create(creditCardRequest).Target;

            return result.Target;
        }

    }
}
