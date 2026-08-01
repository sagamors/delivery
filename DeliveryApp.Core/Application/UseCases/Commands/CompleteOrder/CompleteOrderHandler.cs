using CSharpFunctionalExtensions;
using Ddd;
using DeliveryApp.Core.Ports;
using Errs;
using MediatR;

namespace DeliveryApp.Core.Application.UseCases.Commands.CompleteOrder;

public class CompleteOrderHandler(IUnitOfWork unitOfWork, IOrderRepository orderRepository, ICourierRepository courierRepository)
    : IRequestHandler<CompleteOrderCommand, UnitResult<Error>>
{
    public async Task<UnitResult<Error>> Handle(CompleteOrderCommand message, CancellationToken cancellationToken)
    {
        var orderResult = await orderRepository.GetAsync(message.OrderId, cancellationToken);
        if (orderResult.HasNoValue)
        {
            return UnitResult.Failure<Error>(Errors.OrderNotExist());
        }
        
        var order = orderResult.Value;
        var courierResult = await courierRepository.GetAsync(message.CourierId, cancellationToken);
        if (courierResult.HasNoValue)
        {
            return UnitResult.Failure<Error>(Errors.CourierNotExist());
        }
        
        var courier = courierResult.Value;

        var courierCompleteResult = courier.CompleteOrder(order.Id);
        if (courierCompleteResult.IsFailure)
        {
            return courierCompleteResult;
        }

        var orderCompleteResult = order.Complete();
        if (orderCompleteResult.IsFailure)
        {
            return orderCompleteResult;
        }

        orderRepository.Update(order);
        courierRepository.Update(courier);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UnitResult.Success<Error>();
    }

    public static class Errors
    {
        public static Error OrderNotExist() => new(
            "order.not.exist", "Заказ не найден");
        
        public static Error CourierNotExist() => new(
            "courier.not.exist", "Курьер не найден");
    }
}