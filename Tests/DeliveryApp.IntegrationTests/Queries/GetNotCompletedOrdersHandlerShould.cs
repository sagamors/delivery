using DeliveryApp.Core;
using DeliveryApp.Core.Application.UseCases.Queries.GetNotCompletedOrders;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using DeliveryApp.Infrastructure.Adapters.Postgres;
using DeliveryApp.Infrastructure.Adapters.Postgres.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace DeliveryApp.IntegrationTests.Queries;

public class GetNotCompletedOrdersHandlerShould : IntegrationTestBase
{
    [Fact]
    public async Task ReturnOnlyOrdersThatAreNotCompleted()
    {
        // Arrange
        var notCompletedOrderId = Guid.NewGuid();
        var notCompletedOrder = Order.MustCreate(notCompletedOrderId, Volume.MustCreate(5), Location.Min);

        var completedOrderId = Guid.NewGuid();
        var completedOrder = Order.MustCreate(completedOrderId, Volume.MustCreate(3), Location.Max);
        completedOrder.Assign(Guid.NewGuid());
        completedOrder.Complete();

        var orderRepository = new OrderRepository(DbContext);
        await orderRepository.AddAsync(notCompletedOrder);
        await orderRepository.AddAsync(completedOrder);

        var unitOfWork = new UnitOfWork(DbContext);
        await unitOfWork.SaveChangesAsync();

        var handler = new GetNotCompletedOrdersHandler(Options.Create(new Settings { ConnectionString = ConnectionString }));

        // Act
        var result = await handler.Handle(new GetNotCompletedOrdersQuery(), CancellationToken.None);

        // Assert
        result.Orders.Should().ContainSingle(o => o.Id == notCompletedOrderId);
        result.Orders.Should().NotContain(o => o.Id == completedOrderId);

        var dto = result.Orders.Single(o => o.Id == notCompletedOrderId);
        dto.LocationDto.X.Should().Be(Location.Min.X);
        dto.LocationDto.Y.Should().Be(Location.Min.Y);
    }

    [Fact]
    public async Task ReturnNoneWhenAllOrdersAreCompleted()
    {
        // Arrange
        var completedOrderId = Guid.NewGuid();
        var completedOrder = Order.MustCreate(completedOrderId, Volume.MustCreate(2), Location.Min);
        completedOrder.Assign(Guid.NewGuid());
        completedOrder.Complete();

        var orderRepository = new OrderRepository(DbContext);
        await orderRepository.AddAsync(completedOrder);

        var unitOfWork = new UnitOfWork(DbContext);
        await unitOfWork.SaveChangesAsync();

        var handler = new GetNotCompletedOrdersHandler(Options.Create(new Settings { ConnectionString = ConnectionString }));

        // Act
        var result = await handler.Handle(new GetNotCompletedOrdersQuery(), CancellationToken.None);

        // Assert
        result.Orders.Should().BeEmpty();
    }
}
