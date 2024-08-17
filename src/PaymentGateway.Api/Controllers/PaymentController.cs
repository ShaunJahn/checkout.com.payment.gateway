using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using PaymentGateway.Api.Application.Commands;
using PaymentGateway.Api.Contracts;

namespace PaymentGateway.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class PaymentController(IMediator mediator) : ControllerBase
    {
        [HttpPost]
        [ProducesResponseType(typeof(PaymentCreatedResponse), StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ProcessPaymentAsync(ClientPaymentRequest request)
        {
            var paymentRequest = new PaymentRequestCommand
            {
                CardNumber = request.CardNumber,
                ExpiryMonth = request.ExpiryMonth,
                ExpiryYear = request.ExpiryYear,
                Currency = request.Currency,
                Amount = request.Amount,
                Cvv = request.Cvv
            };
            var response = await mediator.Send(paymentRequest);
            return Ok(response);
        }
    }
}
