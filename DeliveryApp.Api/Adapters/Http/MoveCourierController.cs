using System.ComponentModel.DataAnnotations;
using DeliveryApp.Core.Application.UseCases.Commands.MoveCourier;
using DomainLocation = DeliveryApp.Core.Domain.Model.SharedKernel.Location;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using OpenApi.Controllers;
using OpenApi.Models;

namespace DeliveryApp.Api.Adapters.Http;

public class MoveCourierController(IMediator mediator) : MoveCourierApiController
{
    public override async Task<IActionResult> MoveCourier([FromRoute (Name = "courierId")][Required]Guid courierId, [FromBody]Location location)
    {
        var newLocationResult = DomainLocation.Create(location.X, location.Y);
        if (newLocationResult.IsFailure)
            return BadRequest(newLocationResult.Error.ToApiError(StatusCodes.Status400BadRequest));

        var moveCourierCommandResult = MoveCourierCommand.Create(courierId, newLocationResult.Value);
        if (moveCourierCommandResult.IsFailure)
            return BadRequest(moveCourierCommandResult.Error.ToApiError(StatusCodes.Status400BadRequest));

        var sendResult = await mediator.Send(moveCourierCommandResult.Value);
        if (sendResult.IsFailure) return Conflict(sendResult.Error.ToApiError(StatusCodes.Status409Conflict));
        return Ok();
    }
}