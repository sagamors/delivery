using CntFixtures;
using DeliveryApp.Infrastructure.Adapters.Postgres;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DeliveryApp.IntegrationTests;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    private readonly PostgresFixture _postgres = new();

    private ApplicationDbContext? _dbContext;

    protected string ConnectionString => _postgres.ConnectionString;

    protected ApplicationDbContext DbContext =>
        _dbContext ?? throw new InvalidOperationException(
            "DbContext is not initialized. Ensure InitializeAsync was called."
        );

    public async Task InitializeAsync()
    {
        // 1. Поднимаем Postgres через fixture
        await _postgres.InitializeAsync();

        // 2. Создаём DbContext поверх fixture
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(
                _postgres.ConnectionString,
                x => x.MigrationsAssembly("DeliveryApp.Infrastructure"))
            .Options;

        _dbContext = new ApplicationDbContext(options);

        // 3. Гарантируем схему
        await _dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        if (_dbContext != null)
        {
            await _dbContext.DisposeAsync();
        }

        await _postgres.DisposeAsync();
    }
}