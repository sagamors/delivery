using DeliveryApp.Core.Domain.Model.OrderAggregate.DomainEvents;
using DeliveryApp.Core.Ports;
using MediatR;

namespace DeliveryApp.Core.Application.EventHandlers;

public class OrderCompletedDomainEventHandler(IOrderEventsProducer producer) : INotificationHandler<OrderCompletedDomainEvent>
{
    private readonly IOrderEventsProducer _producer = producer;

    public Task Handle(OrderCompletedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        return _producer.PublishAsync(domainEvent, cancellationToken);
    }
}