using Ddd;

namespace DeliveryApp.Core.Domain.Model.OrderAggregate.DomainEvents;

public sealed record OrderCompletedDomainEvent(Order Order): DomainEvent;