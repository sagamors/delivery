
using DeliveryApp.Core.Domain.Model.OrderAggregate.DomainEvents;

namespace DeliveryApp.Core.Ports;

public interface IOrderEventsProducer
{
    Task PublishAsync(OrderAssignedDomainEvent notification,
        CancellationToken cancellationToken);

    Task PublishAsync(OrderCompletedDomainEvent notification,
        CancellationToken cancellationToken);
}