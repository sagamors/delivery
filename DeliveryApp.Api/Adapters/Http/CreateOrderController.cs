using DeliveryApp.Core.Application.UseCases.Commands.CreateOrder;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenApi.Controllers;
using OpenApi.Models;

namespace DeliveryApp.Api.Adapters.Http;

public class CreateOrderController(IMediator mediator) : CreateOrderApiController
{
    public override async Task<IActionResult> CreateOrder([FromBody] NewOrder newOrder)
    {
        var createOrderCommandResult = CreateOrderCommand.Create(
            newOrder.Id,
            newOrder.Address.Country, newOrder.Address.City, newOrder.Address.Street, newOrder.Address.House, newOrder.Address.Apartment,
            newOrder.Volume);

        if (createOrderCommandResult.IsFailure)
            return BadRequest(createOrderCommandResult.Error.ToApiError(StatusCodes.Status400BadRequest));

        var sendResult = await mediator.Send(createOrderCommandResult.Value);
        if (sendResult.IsFailure) return Conflict(sendResult.Error.ToApiError(StatusCodes.Status409Conflict));
        return StatusCode(StatusCodes.Status201Created, new CreateOrderResponse { OrderId = newOrder.Id });
    }
}