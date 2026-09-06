using Ddd;
using DeliveryApp.Infrastructure.Adapters.Postgres;
using DeliveryApp.Infrastructure.Adapters.Postgres.Entities;
using JsonNet.ContractResolvers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Quartz;

namespace DeliveryApp.Api.Adapters.BackgroundJobs;

[DisallowConcurrentExecution]
public class ProcessOutboxMessagesJob(ApplicationDbContext dbContext, IMediator mediator) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        // Получаем все DomainEvents, которые еще не были отправлены (где ProcessedOnUtc == null)
        var outboxMessages = await dbContext
            .Set<OutboxMessage>()
            .Where(m => m.ProcessedOnUtc == null)
            .OrderBy(o => o.OccurredOnUtc)
            .Take(20)
            .ToListAsync(context.CancellationToken);

        // Если такие есть, то перебираем их в цикле
        if (outboxMessages.Any())
        {
            foreach (var outboxMessage in outboxMessages)
            {
                // Настройки сериализатора
                var settings = new JsonSerializerSettings
                {
                    ContractResolver = new PrivateSetterContractResolver(),
                    TypeNameHandling = TypeNameHandling.All
                };

                // Десериализуем запись из OutboxMessages в DomainEvent
                var domainEvent = JsonConvert.DeserializeObject<DomainEvent>(outboxMessage.Payload, settings);

                // Отправляем
                await mediator.Publish(domainEvent, context.CancellationToken);

                // Если предыдущий метод не вернул ошибку, значит отправка была успешной
                // Ставим дату отправки, это будет признаком, что сообщение отправлять больше не нужно 
                outboxMessage.ProcessedOnUtc = DateTime.UtcNow;
            }

            // Сохраняем изменения
            await dbContext.SaveChangesAsync();
        }
    }
}