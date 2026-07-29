using CSharpFunctionalExtensions;
using Ddd;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using DeliveryApp.Core.Ports;
using Errs;
using MediatR;

namespace DeliveryApp.Core.Application.UseCases.Commands;

public class CreateOrderHandler(IUnitOfWork unitOfWork, IOrderRepository orderRepository)
    : IRequestHandler<CreateOrderCommand, UnitResult<Error>>
{
    public async Task<UnitResult<Error>> Handle(CreateOrderCommand message, CancellationToken cancellationToken)
    {
        var existOrder = await orderRepository.GetAsync(message.OrderId, cancellationToken);
        if (existOrder.HasValue)
        {
            return UnitResult.Failure<Error>(Errors.AlreadyExist());
        }

        var location = Location.CreateRandom();
        var orderCreateResult = Order.Create(message.OrderId, message.Volume, location);
        if (orderCreateResult.IsFailure)
        {
            return orderCreateResult;
        }

        var order = orderCreateResult.Value;

        await orderRepository.AddAsync(order);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UnitResult.Success<Error>();
    }

    public static class Errors
    {
        public static Error AlreadyExist() => new(
            "order.already.exist", "Заказ уже существует");
    }
}