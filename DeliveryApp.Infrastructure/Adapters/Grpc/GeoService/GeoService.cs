using Clients.Geo;
using CSharpFunctionalExtensions;
using DeliveryApp.Core.Ports;
using Errs;
using DomainLocation = DeliveryApp.Core.Domain.Model.SharedKernel.Location;

namespace DeliveryApp.Infrastructure.Adapters.Grpc.GeoService;

public class GeoService(Geo.GeoClient client) : IGeoService
{
    public async Task<Result<DomainLocation, Error>> GetGeolocationAsync(string street, CancellationToken cancellationToken)
    {
        var reply = await client.GetGeolocationAsync(
            new GetGeolocationRequest { Street = street },
            cancellationToken: cancellationToken);

        return DomainLocation.Create(reply.Location.X, reply.Location.Y);
    }
}
