using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Ports;
using Microsoft.EntityFrameworkCore;

namespace DeliveryApp.Infrastructure.Adapters.Postgres.Repositories;

public class CourierRepository(ApplicationDbContext dbContext) : ICourierRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    
    public async Task AddAsync(Courier courier)
    {
        await _dbContext.Couriers.AddAsync(courier);
    }

    public void Update(Courier courier)
    {
        _dbContext.Couriers.Update(courier);
    }

    public async Task<Maybe<Courier>> GetAsync(Guid сourierId, CancellationToken cancellationToken)
    {
        var order = await _dbContext
            .Couriers
            .Include(c => c.Assignments)
            .SingleOrDefaultAsync(o => o.Id == сourierId, cancellationToken);

        return order;
    }

    public async Task<IReadOnlyCollection<Courier>> GetAllAsync(CancellationToken cancellationToken)
    {
        var couriers = await _dbContext
            .Couriers
            .Include(c => c.Assignments)
            .ToArrayAsync(cancellationToken);
        return couriers;
    }
}