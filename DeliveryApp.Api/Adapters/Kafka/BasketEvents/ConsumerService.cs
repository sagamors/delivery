using Confluent.Kafka;
using DeliveryApp.Core;
using DeliveryApp.Core.Application.UseCases.Commands.CreateOrder;
using MediatR;
using Microsoft.Extensions.Options;
using Queues.Basket.Events;

namespace DeliveryApp.Api.Adapters.Kafka.BasketEvents;

public class ConsumerService : BackgroundService
{
    private readonly IConsumer<Ignore, byte[]> _consumer;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ConsumerService> _logger;
    private readonly string _topic;

    public ConsumerService(IServiceScopeFactory scopeFactory, IOptions<Settings> settings, ILogger<ConsumerService> logger)
    {
        ArgumentException.ThrowIfNullOrEmpty(settings.Value.MessageBrokerHost);
        ArgumentException.ThrowIfNullOrEmpty(settings.Value.BasketEventsTopic);
        
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = settings.Value.MessageBrokerHost,
            GroupId = "DeliveryConsumerGroup",
            EnableAutoOffsetStore = false,
            EnableAutoCommit = true,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnablePartitionEof = true
        };
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _consumer = new ConsumerBuilder<Ignore, byte[]>(consumerConfig).Build();
        _topic = settings.Value.BasketEventsTopic;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _consumer.Subscribe(_topic);
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var consumeResult = _consumer.Consume(cancellationToken);

                if (consumeResult.IsPartitionEOF)
                    continue;

                using var scope = _scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                BasketConfirmedIntegrationEvent evt;

                try
                {
                    evt = BasketConfirmedIntegrationEvent.Parser.ParseFrom(consumeResult.Message.Value);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Deserialization error: {Message}", ex.Message);
                    continue;
                }

                if (!Guid.TryParse(evt.BasketId, out var basketGuid))
                {
                    _logger.LogError("Invalid BasketId format: {BasketId}", evt.BasketId);
                    continue;
                }

                var commandResult = CreateOrderCommand.Create(
                    basketGuid,
                    evt.Address.Country,
                    evt.Address.City,
                    evt.Address.Street,
                    evt.Address.House,
                    evt.Address.Apartment,
                    evt.Volume);

                if (commandResult.IsFailure)
                {
                    _logger.LogError("Error creating order command {BasketId} {code}", basketGuid,
                        commandResult.Error.Code);
                    continue;
                }

                var result = await mediator.Send(commandResult.Value, cancellationToken);

                if (result.IsFailure)
                {
                    _logger.LogError("Error executing create order command {BasketId} {code}", basketGuid,
                        result.Error.Code);
                    continue;
                }

                _consumer.StoreOffset(consumeResult);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Processing cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Unhandled exception in Kafka consumer loop, stopping host");
            throw;
        }
        finally
        {
            _consumer.Close();
        }
    }
}
