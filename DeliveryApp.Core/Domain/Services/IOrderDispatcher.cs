using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using Errs;

namespace DeliveryApp.Core.Domain.Services;

public interface IOrderDispatcher
{
    Result<Courier, Error> Dispatch(Order order, IEnumerable<Courier> couriers);
}
