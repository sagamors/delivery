using CntFixtures;
using Xunit;

namespace DeliveryApp.ComponentTests;

public abstract class ComponentTestBase : IAsyncLifetime
{
    protected readonly KafkaFixture Kafka = new();

    protected readonly PostgresFixture Postgres = new();
    protected HttpClient Client = null!;
    protected TestWebApplicationFactory Factory = null!;

    public async Task InitializeAsync()
    {
        await Postgres.InitializeAsync();
        await Kafka.InitializeAsync();

        Environment.SetEnvironmentVariable(
            "CONNECTION_STRING", Postgres.ConnectionString);
        Environment.SetEnvironmentVariable(
            "MESSAGE_BROKER_HOST", Kafka.BootstrapServers);

        Environment.SetEnvironmentVariable("DISCOUNT_SERVICE_GRPC_HOST", "http://localhost:5003");
        Environment.SetEnvironmentVariable("STOCK_EVENTS_TOPIC", "stock.events");
        Environment.SetEnvironmentVariable("BASKET_EVENTS_TOPIC", "basket.events");

        Factory = new TestWebApplicationFactory(Postgres.ConnectionString);
        Client = Factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();
        await Factory.DisposeAsync();

        await Kafka.DisposeAsync();
        await Postgres.DisposeAsync();
    }
}