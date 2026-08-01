namespace DeliveryApp.Core.Application.UseCases.Queries.GetAllCouriers;

public class GetAllCouriersResult
{
    public IReadOnlyList<CourierDto> Couriers { get; set; } = Array.Empty<CourierDto>();

    public static GetAllCouriersResult None => new();
}