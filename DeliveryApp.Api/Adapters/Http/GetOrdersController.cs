using DeliveryApp.Core.Application.UseCases.Queries.GetNotCompletedOrders;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using OpenApi.Controllers;
using OpenApi.Models;

namespace DeliveryApp.Api.Adapters.Http;

public class GetOrdersController(IMediator mediator) : GetOrdersApiController
{
    public override async Task<IActionResult> GetOrders()
    {
        var getOrdersCommandResult = new GetNotCompletedOrdersQuery();
        var sendResult = await mediator.Send(getOrdersCommandResult);
        var orders = sendResult.Orders.Select<OrderDto, Order>(o => new Order
        {
            Id = o.Id,
            Location = new Location
            {
                X = o.Location.X,
                Y = o.Location.Y
            }
        }).ToList();
        
        return Ok(orders);
    }
}