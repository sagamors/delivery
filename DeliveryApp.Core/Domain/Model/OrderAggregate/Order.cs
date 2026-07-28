using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;
using Ddd;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using Errs;
using Errs.Extensions;

namespace DeliveryApp.Core.Domain.Model.OrderAggregate;

public class Order : Aggregate<Guid>
{
    public Volume Volume { get; private set; }

    public Location Location { get; private set; }

    public Guid? CourierId { get; private set; }

    public OrderStatus Status { get; private set; }

    [ExcludeFromCodeCoverage]
    private Order()
    {
        
    }
    
    private Order(Guid id, Volume volume, Location location) : base(id)
    {
        Volume = volume;
        Location = location;
        Status = OrderStatus.Created;
    }
    
    /// <summary>
    ///     Создаёт заказ в статусе Created
    /// </summary>
    public static Result<Order, Error> Create(Guid orderId, Volume volume, Location location)
    {
        if (Guard.IsNullOrEmpty(orderId))
            return GeneralErrors.ValueIsRequired(nameof(orderId));
        
        if (volume is null)
            return GeneralErrors.ValueIsRequired(nameof(volume));
        
        if (location is null)
            return GeneralErrors.ValueIsRequired(nameof(location));
        
        return new Order(orderId, volume, location);
    }
    
    /// <summary>
    ///     Factory Method. Создаёт объект в контексте, где нарушение инвариантов невозможно и является исключительной ситуацией
    /// </summary>
    public static Order MustCreate(Guid orderId, Volume volume, Location location)
    {
        return Create(orderId, volume, location).GetValueOrThrow();
    }

    public  UnitResult<Error> Assign(Guid courierId)
    {
        if (courierId == default)
            return GeneralErrors.ValueIsRequired(nameof(courierId));
        
        if (Status != OrderStatus.Created)
            return Errors.NotCreated();
        
        Status = OrderStatus.Assigned;
        CourierId = courierId;
        return UnitResult.Success<Error>();
    }
    
    public  UnitResult<Error> Complete()
    {
        if (Status == OrderStatus.Completed)
            return Errors.AlreadyCompleted();
        
        if (Status != OrderStatus.Assigned)
            return Errors.NotAssigned();
        
        Status = OrderStatus.Completed;

        return UnitResult.Success<Error>();
    }
    
    public static class Errors
    {
        public static Error AlreadyCompleted() => new(
            "order.already.completed", "Заказ уже завершен");
        
        public static Error NotCreated() => new(
            "order.not.created", "Заказ должен быть в статусе 'Создан'");
        
        public static Error NotAssigned() => new(
            "order.not.assigned", "Заказ должен быть в статусе 'Назначен'");
    }
}