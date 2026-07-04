using ECommercePlatform.Identity;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using PaymentService.Application.Payments.Command;
using PaymentService.Application.Payments.Queries;
using PaymentService.Contracts.Requests;
using PaymentService.Contracts.Responses;

namespace PaymentService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController
        (IMediator mediator) : ControllerBase
    {
        [Authorize(Roles = $"{Roles.Admin},{Roles.Customer}")]
        [HttpGet("{orderId:guid}")]
        public async Task<IActionResult> GetPaymentForOrder([FromRoute] Guid orderId, CancellationToken cancellationToken)
        {
            GetPaymentByOrderId query = new GetPaymentByOrderId(orderId);

            OrderPaymentDto? orderPaymentDto = await mediator.Send(query, cancellationToken);

            if (orderPaymentDto is null)
                return NotFound();

            OrderPaymentResponse orderPaymentResponse = new OrderPaymentResponse(
                orderPaymentDto.Id,
                orderPaymentDto.OrderId,
                orderPaymentDto.Amount.Amount,
                orderPaymentDto.Amount.Currency,
                orderPaymentDto.Status.ToString(),
                orderPaymentDto.PaymentMethod.ToString(),
                orderPaymentDto.ProcessedAt);

            return Ok(orderPaymentResponse);
        }

        [Authorize(Roles = $"{Roles.Admin},{Roles.Customer}")]
        [HttpPost("pay")]
        public async Task<IActionResult> PayWithCard([FromBody] PayWithCardRequest request, CancellationToken cancellationToken)
        {
            PayWithCardCommand command = new PayWithCardCommand(
                request.PaymentId,
                request.CardNumber,
                request.CardHolder,
                request.Expiry,
                request.Cvv);

            await mediator.Send(command, cancellationToken);

            return Accepted();
        }
    }
}
