using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using Errs;

namespace DeliveryApp.Core.Domain.Services;

public class OrderDispatcher : IOrderDispatcher
{
    public Result<Courier, Error> Dispatch(Order order, IEnumerable<Courier> couriers)
    {
        if (order is null)
            return GeneralErrors.ValueIsRequired(nameof(order));

        if (couriers is null)
            return GeneralErrors.ValueIsRequired(nameof(couriers));

        if (order.Status != OrderStatus.Created)
            return Order.Errors.NotCreated();

        var eligibleCouriers = couriers.Where(c => c.CanTakeOrder(order).IsSuccess).ToList();
        if (eligibleCouriers.Count == 0)
            return Errors.NoSuitableCourier();

        var winner = eligibleCouriers.Select(c => new { Courier = c, Distance = c.Location.DistanceTo(order.Location) }).MinBy(d => d.Distance).Courier;

        var takeResult = winner.TakeOrder(order);
        if (takeResult.IsFailure)
            return takeResult.Error;

        var assignResult = order.Assign(winner.Id);
        if (assignResult.IsFailure)
            return assignResult.Error;

        return winner;
    }

    public static class Errors
    {
        public static Error NoSuitableCourier() => new(
            "order.dispatch.no.suitable.courier", "Нет подходящего курьера для заказа");
    }
}
