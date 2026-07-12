using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using Errs;

namespace DeliveryApp.Core.Domain.Model.CourierAggregate;

public sealed class Assignment : Entity<Guid>
{
    public Guid OrderId { get; private set; }

    public Volume Volume { get; private set; }

    public Location Location { get; private set; }

    public AssignmentStatus Status { get; private set; }

    [ExcludeFromCodeCoverage]
    private Assignment()
    {
    }

    private Assignment(Guid orderId, Volume volume, Location location) : base(Guid.NewGuid())
    {
        OrderId = orderId;
        Volume = volume;
        Location = location;
        Status = AssignmentStatus.Assigned;
    }

    /// <summary>
    ///     Создаёт назначение заказа на курьера в статусе Assigned
    /// </summary>
    public static Result<Assignment, Error> Create(Guid orderId, Volume volume, Location location)
    {
        if (Guard.IsNullOrEmpty(orderId))
            return GeneralErrors.ValueIsRequired(nameof(orderId));

        if (volume is null)
            return GeneralErrors.ValueIsRequired(nameof(volume));

        if (location is null)
            return GeneralErrors.ValueIsRequired(nameof(location));

        return new Assignment(orderId, volume, location);
    }

    /// <summary>
    ///     Завершает назначение, если курьер находится в одной клетке или ближе от точки заказа
    /// </summary>
    public UnitResult<Error> Complete(Location courierLocation)
    {
        if (courierLocation is null)
            return GeneralErrors.ValueIsRequired(nameof(courierLocation));

        if (Status == AssignmentStatus.Completed)
            return Errors.AlreadyCompleted();

        if (Location.DistanceTo(courierLocation) > 1)
            return Errors.CourierTooFar();

        Status = AssignmentStatus.Completed;
        return UnitResult.Success<Error>();
    }

    public static class Errors
    {
        public static Error AlreadyCompleted() => new(
            "assignment.already.completed", "Назначение уже завершено");

        public static Error CourierTooFar() => new(
            "assignment.courier.too.far", "Курьер находится слишком далеко, чтобы завершить назначение");
    }
}
