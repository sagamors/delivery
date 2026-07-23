using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Ports;
using Microsoft.EntityFrameworkCore;

namespace DeliveryApp.Infrastructure.Adapters.Postgres.Repositories;

public class OrderRepository(ApplicationDbContext dbContext) : IOrderRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public async Task AddAsync(Order order)
    {
        await _dbContext.Orders.AddAsync(order);
    }

    public void Update(Order order)
    {
        _dbContext.Orders.Update(order);
    }

    public async Task<Maybe<Order>> GetAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await _dbContext
            .Orders
            .SingleOrDefaultAsync(o => o.Id == orderId, cancellationToken);
        return order;
    }

    public async Task<Maybe<Order>> GetFirstByCreatedStatusAsync(CancellationToken cancellationToken)
    {
        var order = await _dbContext
            .Orders
            .SingleOrDefaultAsync(o => o.Status == OrderStatus.Created, cancellationToken);
        return order;
    }

    public async Task<IReadOnlyCollection<Order>> GetAllByAssignedStatusAsync(CancellationToken cancellationToken)
    {
        var order = await _dbContext
            .Orders
            .Where(o => o.Status == OrderStatus.Assigned)
            .ToArrayAsync<Order>(cancellationToken);
        return order;
    }
}