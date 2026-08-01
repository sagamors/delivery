using CSharpFunctionalExtensions;
using Ddd;
using DeliveryApp.Core.Domain.Services;
using DeliveryApp.Core.Ports;
using Errs;
using MediatR;

namespace DeliveryApp.Core.Application.UseCases.Commands.AssignOrder;

public class AssignOrderHandler(
    IOrderRepository orderRepository,
    ICourierRepository courierRepository,
    IOrderDispatcher dispatchService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AssignOrderCommand, UnitResult<Error>>
{
    public async Task<UnitResult<Error>> Handle(AssignOrderCommand command, CancellationToken cancellationToken)
    {
        var orderResult = await orderRepository.GetFirstByCreatedStatusAsync(cancellationToken);
        if (orderResult.HasNoValue) return Errors.NotAvailableOrders();

        var availableCouriers = await courierRepository.GetAllAsync(cancellationToken);
        if (availableCouriers.Count == 0) return Errors.NotAvailableCouriers();

        var order = orderResult.Value;
        var courierResult = dispatchService.Dispatch(order, availableCouriers);
        if (courierResult.IsFailure) return courierResult;

        var courier = courierResult.Value;
        courierRepository.Update(courier);
        orderRepository.Update(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return UnitResult.Success<Error>();
    }

    public static class Errors
    {
        public static Error NotAvailableOrders() => new Error("order.not.available", "Нет заказов");
        public static Error NotAvailableCouriers() => new Error("courier.not.available", "Нет курьеров");
    }
}
