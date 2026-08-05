namespace DeliveryApp.Core.Application.UseCases.Queries.GetNotCompletedOrders;

public class OrderDto
{
    public Guid Id { get; set; }
    
    public LocationDto Location { get; set; }
}