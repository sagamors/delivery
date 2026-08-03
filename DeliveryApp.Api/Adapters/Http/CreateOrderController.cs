using DeliveryApp.Core.Application.UseCases.Commands.CreateOrder;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using OpenApi.Controllers;
using OpenApi.Models;

namespace DeliveryApp.Api.Adapters.Http;

public class CreateOrderController(IMediator mediator) : CreateOrderApiController
{
    public override async Task<IActionResult> CreateOrder([FromBody] NewOrder newOrder)
    {
        var volumeResult = Volume.Create(newOrder.Volume);
        if (volumeResult.IsFailure) return BadRequest(volumeResult.Error);
        
        var createOrderCommandResult = CreateOrderCommand.Create(
            newOrder.Id,
            newOrder.Address.Country, newOrder.Address.City, newOrder.Address.Street, newOrder.Address.House, newOrder.Address.Apartment,
            volumeResult.Value);
        
        if (createOrderCommandResult.IsFailure) return BadRequest(createOrderCommandResult.Error);

        var sendResult = await mediator.Send(createOrderCommandResult.Value);
        if (sendResult.IsFailure) return Conflict(sendResult.Error);
        return Ok();
    }
}