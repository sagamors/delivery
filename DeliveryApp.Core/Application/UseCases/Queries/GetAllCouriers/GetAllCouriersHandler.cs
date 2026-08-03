using Dapper;
using MediatR;
using Microsoft.Extensions.Options;
using Npgsql;

namespace DeliveryApp.Core.Application.UseCases.Queries.GetAllCouriers;

public class GetAllCouriersHandler(IOptions<Settings> settings) : IRequestHandler<GetAllCouriersQuery, GetAllCouriersResult>
{
    private readonly string _connectionString = !string.IsNullOrWhiteSpace(settings.Value.ConnectionString)
            ? settings.Value.ConnectionString
            : throw new ArgumentNullException(nameof(settings));

    public async Task<GetAllCouriersResult> Handle(GetAllCouriersQuery message, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var rows = await connection.QueryAsync<CourierRow>(
            @"SELECT id, name, location_x AS LocationX, location_y AS LocationY FROM public.couriers");

        var dbCouriers = rows.ToList();

        if (dbCouriers.Count == 0)
            return GetAllCouriersResult.None;

        var couriers = dbCouriers.Select(Mapper.MapToCourier).ToList();

        return new GetAllCouriersResult { Couriers = couriers };
    }

    private sealed class CourierRow
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public int LocationX { get; set; }
        public int LocationY { get; set; }
    }

    private static class Mapper
    {
        public static CourierDto MapToCourier(CourierRow row) => new()
        {
            Id = row.Id,
            Name = row.Name,
            Location = new LocationDto { X = row.LocationX, Y = row.LocationY }
        };
    }
}