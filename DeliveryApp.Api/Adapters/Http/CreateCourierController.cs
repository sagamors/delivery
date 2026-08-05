using DeliveryApp.Core.Application.UseCases.Commands.CreateCourier;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenApi.Controllers;
using OpenApi.Models;

namespace DeliveryApp.Api.Adapters.Http;

public class CreateCourierController(IMediator mediator) : CreateCourierApiController
{
    public override async Task<IActionResult> CreateCourier([FromBody] NewCourier newCourier)
    {
        var createCourierCommandResult = CreateCourierCommand.Create(newCourier.Name);
        if (createCourierCommandResult.IsFailure)
            return BadRequest(createCourierCommandResult.Error.ToApiError(StatusCodes.Status400BadRequest));

        var sendResult = await mediator.Send(createCourierCommandResult.Value);
        if (sendResult.IsFailure) return Conflict(sendResult.Error.ToApiError(StatusCodes.Status409Conflict));
        return StatusCode(StatusCodes.Status201Created, new CreateCourierResponse { CourierId = sendResult.Value });
    }
}