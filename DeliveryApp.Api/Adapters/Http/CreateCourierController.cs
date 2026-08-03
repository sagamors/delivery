using DeliveryApp.Core.Application.UseCases.Commands.CreateCourier;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using OpenApi.Controllers;
using OpenApi.Models;

namespace DeliveryApp.Api.Adapters.Http;

public class CreateCourierController(IMediator mediator) : CreateCourierApiController
{
    public override async Task<IActionResult> CreateCourier([FromBody] NewCourier newCourier)
    {
        var createCourierCommandResult = CreateCourierCommand.Create(newCourier.Name);
        if (createCourierCommandResult.IsFailure) return BadRequest(createCourierCommandResult.Error);

        var sendResult = await mediator.Send(createCourierCommandResult.Value);
        if (sendResult.IsFailure) return Conflict(sendResult.Error);
        return Ok();
    }
}