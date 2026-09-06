using Ddd;
using DeliveryApp.Infrastructure.Adapters.Postgres.Entities;
using MediatR;
using Newtonsoft.Json;

namespace DeliveryApp.Infrastructure.Adapters.Postgres;

public class UnitOfWork(ApplicationDbContext dbContext) : IUnitOfWork
{
    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Сохраняем доменные события в таблицу Outbox до коммита транзакции
        await SaveDomainEventsToOutboxAsync(cancellationToken);

        // Сохраняем всё одной транзакцией (агрегаты + outbox)
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task SaveDomainEventsToOutboxAsync(CancellationToken cancellationToken)
    {
        // Собираем доменные события из всех агрегатов, отслеживаемых контекстом
        var outboxMessages = dbContext.ChangeTracker
            .Entries<IAggregateRoot>()
            .Select(e => e.Entity)
            .SelectMany(aggregate =>
            {
                var domainEvents = aggregate.GetDomainEvents();
                aggregate.ClearDomainEvents(); // очищаем после извлечения
                return domainEvents;
            })
            .Select(domainEvent => new OutboxMessage
            {
                Id = domainEvent.EventId,
                OccurredOnUtc = DateTime.UtcNow,
                Type = domainEvent.GetType().Name,
                Payload = JsonConvert.SerializeObject(
                    domainEvent,
                    new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.All
                    })
            })
            .ToList();

        if (outboxMessages.Count > 0)
            await dbContext.Set<OutboxMessage>().AddRangeAsync(outboxMessages, cancellationToken);
    }
}