using System.Text;
using Dapper;
using FluentAssertions;
using Npgsql;
using Xunit;

namespace DeliveryApp.ComponentTests;

public class CreateOrderMethodTest : ComponentTestBase
{
    [Fact]
    public async Task CreateOrderAndReturn20OkWhenParamsAreCorrect()
    {
        // Arrange
        var orderId = Guid.Parse("f4aedb25-2952-4720-a5f4-018dfb7d9714");
        var payload = $"{{\"id\":\"{orderId}\",\"address\":{{\"country\":\"Russia\",\"city\":\"Spb\",\"street\":\"123 Main St\",\"house\":\"3\",\"apartment\":\"4\"}},\"volume\":3}}";
        var content = new StringContent(payload, Encoding.UTF8, "application/json");

        // Act
        var response = await Client.PostAsync("/api/v1/orders", content);
        response.EnsureSuccessStatusCode();

        // Assert
        await using var connection = new NpgsqlConnection(Postgres.ConnectionString);
        var rows = (await connection.QueryAsync<dynamic>(
            "SELECT * FROM public.orders"
        )).ToList();

        rows.Should().HaveCount(1);
        ((Guid)rows.Single().id).Should().Be(orderId);
    }
}