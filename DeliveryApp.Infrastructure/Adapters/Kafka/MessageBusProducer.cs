using System.Text;
using Confluent.Kafka;
using DeliveryApp.Core;
using DeliveryApp.Core.Domain.Model.OrderAggregate.DomainEvents;
using DeliveryApp.Core.Ports;
using Google.Protobuf;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Queues.Order.Events;

namespace DeliveryApp.Infrastructure.Adapters.Kafka;

public sealed class OrderEventsProducer : IOrderEventsProducer
{
    private readonly IProducer<string, byte[]> _producer;
    private readonly string _topicName;

    public OrderEventsProducer(IProducer<string, byte[]> producer, IOptions<Settings> options)
    {
        _producer = producer ?? throw new ArgumentNullException(nameof(producer));

        if (string.IsNullOrWhiteSpace(options.Value.OrderEventsTopic))
            throw new ArgumentException(nameof(options.Value.OrderEventsTopic));
        _topicName = options.Value.OrderEventsTopic;
    }

    public async Task PublishAsync(OrderAssignedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        var orderAssignedIntegrationEvent = new OrderAssignedIntegrationEvent()
        {
            OrderId = notification.OrderId.ToString(),
        };

        await Produce(key: notification.EventId.ToString(), orderAssignedIntegrationEvent, notification, cancellationToken);
    }

    public async Task PublishAsync(OrderCompletedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        var orderCompletedIntegrationEvent = new OrderCompletedIntegrationEvent()
        {
            OrderId = notification.OrderId.ToString()
        };

        await Produce(key: notification.EventId.ToString(), orderCompletedIntegrationEvent, notification, cancellationToken);
    }

    private async Task Produce<TIntegrationEvent, TDomainEvent>(
        string key,
        TIntegrationEvent integrationEvent,
        TDomainEvent domainEvent,
        CancellationToken cancellationToken)
        where TIntegrationEvent : IMessage
    {
        var message = new Message<string, byte[]>
        {
            Key = key,
            Value = integrationEvent.ToByteArray(),
            Headers = new Headers
        {
            { "event-id", Encoding.UTF8.GetBytes(key) },
            { "event-type", Encoding.UTF8.GetBytes(integrationEvent.GetType().Name) },
            { "occurred-at", Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString("O")) },
            { "content-type", "application/x-protobuf"u8.ToArray() },
            { "debug-json", Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(domainEvent)) }
        }
        };

        await _producer.ProduceAsync(_topicName, message, cancellationToken);
    }
}