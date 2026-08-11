using Ddd;
using MediatR;

namespace DeliveryApp.Infrastructure.Adapters.Postgres;

public class UnitOfWork(ApplicationDbContext dbContext, IMediator mediator) : IUnitOfWork
{
    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
        await PublishDomainEventsAsync();
        return true;
    }

    private async Task PublishDomainEventsAsync()
    {
        // Получаем агрегаты, у которых есть доменные события
        var domainEntities = dbContext.ChangeTracker
            .Entries<IAggregateRoot>()
            .Where(e => e.Entity.GetDomainEvents().Any())
            .ToList();

        // Извлекаем все события
        var domainEvents = domainEntities
            .SelectMany(e => e.Entity.GetDomainEvents())
            .ToList();

        // Очищаем их после извлечения
        domainEntities.ForEach(e => e.Entity.ClearDomainEvents());

        // Публикуем через MediatR
        foreach (var domainEvent in domainEvents)
        {
            await mediator.Publish(domainEvent);
        }
    }
}