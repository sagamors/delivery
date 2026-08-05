using System.ComponentModel.DataAnnotations;
using DeliveryApp.Core.Application.UseCases.Commands.CompleteOrder;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenApi.Controllers;

namespace DeliveryApp.Api.Adapters.Http;

public class CompleteOrderController(IMediator mediator) : CompleteOrderApiController
{
    public override async Task<IActionResult> CompleteOrder([FromRoute(Name = "courierId"), Required] Guid courierId, [FromRoute(Name = "orderId"), Required] Guid orderId)
    {
        var completeOrderCommandResult = CompleteOrderCommand.Create(orderId, courierId);
        if (completeOrderCommandResult.IsFailure)
            return BadRequest(completeOrderCommandResult.Error.ToApiError(StatusCodes.Status400BadRequest));

        var sendResult = await mediator.Send(completeOrderCommandResult.Value);
        if (sendResult.IsFailure) return Conflict(sendResult.Error.ToApiError(StatusCodes.Status409Conflict));
        return Ok();
    }
}