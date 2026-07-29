namespace DeliveryApp.Core.Application.UseCases.Queries.GetNotCompletedOrders;

public class GetNotCompletedOrdersResult
{
    public List<OrderDto> Orders { get; } = new();
    
    public GetNotCompletedOrdersResult(List<OrderDto> orders)
    {
        Orders.AddRange(orders);
    }
    
    private GetNotCompletedOrdersResult()
    {
    }
    
    public static GetNotCompletedOrdersResult None => new GetNotCompletedOrdersResult();
}