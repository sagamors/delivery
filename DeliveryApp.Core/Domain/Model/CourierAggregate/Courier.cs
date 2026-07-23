using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;
using Ddd;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using Errs;
using Errs.Extensions;

namespace DeliveryApp.Core.Domain.Model.CourierAggregate;

public class Courier : Aggregate<Guid>
{
    private readonly List<Assignment> _assignments = [];

    public string Name { get; private set; }

    public Location Location { get; private set; }

    public static Volume MaxVolume { get; private set; } = Volume.MustCreate(20);

    public IReadOnlyCollection<Assignment> Assignments { get; }

    [ExcludeFromCodeCoverage]
    private Courier()
    {
        Assignments = new ReadOnlyCollection<Assignment>(_assignments);
    }

    private Courier(string name, Location location) : base(Guid.NewGuid())
    {
        Name = name;
        Location = location;
        Assignments = new ReadOnlyCollection<Assignment>(_assignments);
    }

    /// <summary>
    ///     Создаёт курьера
    /// </summary>
    public static Result<Courier, Error> Create(string name, Location location)
    {
        if (Guard.IsNullOrEmpty(name))
            return GeneralErrors.ValueIsRequired(nameof(name));

        if (location is null)
            return GeneralErrors.ValueIsRequired(nameof(location));

        return new Courier(name, location);
    }
    
    /// <summary>
    ///     Factory Method. Создаёт объект в контексте, где нарушение инвариантов невозможно и является исключительной ситуацией
    /// </summary>
    public static Courier MustCreate(string name, Location location)
    {
        return Create(name, location).GetValueOrThrow();
    }

    public Result<bool, Error> CanTakeOrder(Order order)
    {
        if (order is null)
            return GeneralErrors.ValueIsRequired(nameof(order));

        var totalVolume = _assignments.Aggregate(order.Volume, (acc, ass) => acc + ass.Volume);
        if (totalVolume > MaxVolume)
        {
            return Errors.CanNotTakeOrder();
        }

        return true;
    }


    public UnitResult<Error> TakeOrder(Order order)
    {
        var canTakeOrder = CanTakeOrder(order);
        if (canTakeOrder.IsFailure)
        {
            return canTakeOrder.Error;
        }

        var creationResult = Assignment.Create(order.Id, order.Volume, order.Location);

        if (creationResult.IsFailure)
        {
            return creationResult.Error;
        }

        _assignments.Add(creationResult.Value);

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> CompleteOrder(Order order)
    {
        var assignment = Assignments.FirstOrDefault(ass => ass.OrderId == order.Id);

        if (assignment == null)
        {
            return Errors.AssignmentNotFound();
        }

        var completeResult = assignment.Complete(Location);
        if (completeResult.IsFailure)
        {
            return completeResult.Error;
        }

        _assignments.Remove(assignment);
        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> Move(Location target)
    {
        if (target is null)
            return GeneralErrors.ValueIsRequired(nameof(target));

        var distance = Location.DistanceTo(target);
        if (distance > 1)
        {
            return Errors.CantMoveThatFar();
        }

        Location = target;
        return UnitResult.Success<Error>();
    }

    public static class Errors
    {
        public static Error CanNotTakeOrder() => new("courier.can.not.take", "Нельзя взять заказ");

        public static Error AssignmentNotFound() => new("courier.assignment.not.found", "Назначение не найдено");

        public static Error CantMoveThatFar() => new("courier.cant.move.that.far", "Курьер не может переместиться так далеко");
    }
}