using DeliveryApp.Core.Application.UseCases.Queries.GetAllCouriers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using OpenApi.Controllers;
using OpenApi.Models;

namespace DeliveryApp.Api.Adapters.Http;

public class GetCouriersController(IMediator mediator) : GetCouriersApiController
{
    public override async Task<IActionResult> GetCouriers()
    {
        var getAllCouriersQuery = new GetAllCouriersQuery();
        var sendResult = await mediator.Send(getAllCouriersQuery);
        
        var response = sendResult.Couriers.Select<CourierDto, Courier>(c => new Courier
        {
            Id = c.Id,
            Name = c.Name,
            Location = new Location
            {
                X = c.Location.X,
                Y = c.Location.Y
            }
        }).ToList();
        
        return Ok(response);
    }
}