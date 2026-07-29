using Dapper;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using MediatR;
using Microsoft.Extensions.Options;
using Npgsql;

namespace DeliveryApp.Core.Application.UseCases.Queries.GetNotCompletedOrders;

public class GetNotCompletedOrdersHandler : IRequestHandler<GetNotCompletedOrdersQuery, GetNotCompletedOrdersResult>
{
    private readonly string _connectionString;

    public GetNotCompletedOrdersHandler(IOptions<Settings> settings)
    {
        _connectionString = !string.IsNullOrWhiteSpace(settings.Value.ConnectionString)
            ? settings.Value.ConnectionString
            : throw new ArgumentNullException(nameof(settings));
    }

    public async Task<GetNotCompletedOrdersResult> Handle(GetNotCompletedOrdersQuery message, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var rows = await connection.QueryAsync<OrderRow>(
            @"SELECT id, location_x AS LocationX, location_y AS LocationY FROM public.orders where status IN (@created, @assigned);"
            , new { created = OrderStatus.Created.Name, assigned = OrderStatus.Assigned.Name });

        var dbOrders = rows.ToList();

        if (dbOrders.Count == 0)
            return GetNotCompletedOrdersResult.None;

        var orders = dbOrders.Select(Mapper.MapToOrderDto).ToList();

        return new GetNotCompletedOrdersResult(orders);
    }

    private sealed class OrderRow
    {
        public Guid Id { get; set; }
        public int LocationX { get; set; }
        public int LocationY { get; set; }
    }

    private static class Mapper
    {
        public static OrderDto MapToOrderDto(OrderRow row) => new()
        {
            Id = row.Id,
            LocationDto = new LocationDto { X = row.LocationX, Y = row.LocationY }
        };
    }
}