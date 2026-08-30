using DeliveryApp.Core.Domain.Model.OrderAggregate.DomainEvents;
using DeliveryApp.Core.Ports;
using MediatR;

namespace DeliveryApp.Core.Application.EventHandlers;

public class OrderAssignedDomainEventHandler(IOrderEventsProducer producer) : INotificationHandler<OrderAssignedDomainEvent>
{
    private readonly IOrderEventsProducer _producer = producer;

    public Task Handle(OrderAssignedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        return _producer.PublishAsync(domainEvent, cancellationToken);
    }
}
