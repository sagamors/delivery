using Dapper;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using MediatR;
using Microsoft.Extensions.Options;
using Npgsql;

namespace DeliveryApp.Core.Application.UseCases.Queries.GetNotCompletedOrders;

public class GetNotCompletedOrdersHandler(IOptions<Settings> settings) : IRequestHandler<GetNotCompletedOrdersQuery, GetNotCompletedOrdersResult>
{
    private readonly string _connectionString = !string.IsNullOrWhiteSpace(settings.Value.ConnectionString)
        ? settings.Value.ConnectionString
        : throw new ArgumentNullException(nameof(settings));

    public async Task<GetNotCompletedOrdersResult> Handle(GetNotCompletedOrdersQuery message, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var result = await connection.QueryAsync<dynamic>(
            @"SELECT id, courier_id, location_x, location_y FROM public.orders where status!=@status;"
            , new { status = OrderStatus.Completed.Name });

        var dbOrders = result.ToList();
        
        if (dbOrders.Count == 0)
            return GetNotCompletedOrdersResult.None;

        var orders = dbOrders.Select(Mapper.MapToOrderDto).ToList();

        return new GetNotCompletedOrdersResult(orders);
    }

    private static class Mapper
    {
        public static OrderDto MapToOrderDto(dynamic result)
        {
            var location = new LocationDto { X = result.location_x, Y = result.location_y };
            var order = new OrderDto { Id = result.id, LocationDto = location };
            return order;
        }
    }
}