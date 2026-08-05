using DeliveryApp.Infrastructure.Adapters.Postgres;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DeliveryApp.ComponentTests;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public TestWebApplicationFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Убираем реальный контекст
            var descriptor = services.Single(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

            services.Remove(descriptor);

            // Регистрируем тестовый контекст
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(_connectionString));

            // Применяем миграции
            using var scope = services.BuildServiceProvider().CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.Migrate();
        });
    }
}