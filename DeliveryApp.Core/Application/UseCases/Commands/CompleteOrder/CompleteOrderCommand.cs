using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using Errs;
using MediatR;

namespace DeliveryApp.Core.Application.UseCases.Commands.CompleteOrder;

/// <summary>
///     Завершить заказ
/// </summary>
public class CompleteOrderCommand : IRequest<UnitResult<Error>>
{
    private CompleteOrderCommand(Guid orderId, Guid сourierId)
    {
        OrderId = orderId;
        CourierId = сourierId;
    }
    
    /// <summary>
    ///     Factory Method
    /// </summary>
    /// <param name="orderId">Идентификатор заказа</param>
    /// <param name="сourierId"> Идентификатор курьера</param>
    /// <returns>Результат</returns>
    public static Result<CompleteOrderCommand, Error> Create(Guid orderId, Guid сourierId )
    {
        if (orderId == Guid.Empty) return GeneralErrors.ValueIsRequired(nameof(orderId));
        if (сourierId == Guid.Empty) return GeneralErrors.ValueIsRequired(nameof(сourierId));
        
        return new CompleteOrderCommand(orderId, сourierId);
    }

    /// <summary>
    ///     Идентификатор заказа
    /// </summary>
    public Guid OrderId { get; }

    /// <summary>
    ///     Идентификатор курьера
    /// </summary>
    public Guid CourierId { get; }
}