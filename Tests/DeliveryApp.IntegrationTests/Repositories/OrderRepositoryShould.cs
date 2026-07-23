using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using DeliveryApp.Infrastructure.Adapters.Postgres;
using DeliveryApp.Infrastructure.Adapters.Postgres.Repositories;
using FluentAssertions;
using Xunit;

namespace DeliveryApp.IntegrationTests.Repositories;

public class OrderRepositoryShould : IntegrationTestBase
{
    [Fact]
    public async Task CanAddOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = Order.MustCreate(orderId, Volume.MustCreate(1), Location.Max);

        // Act
        var orderRepository = new OrderRepository(DbContext);
        var unitOfWork = new UnitOfWork(DbContext);

        await orderRepository.AddAsync(order);
        await unitOfWork.SaveChangesAsync();

        // Assert
        var entity = await orderRepository.GetAsync(order.Id, CancellationToken.None);
        entity.Should().BeEquivalentTo(order);
    }
    
    [Fact]
    public async Task CanUpdateOrder()
    {
        // Arrange
        var courier = Guid.NewGuid();

        var orderId = Guid.NewGuid();
        var order = Order.MustCreate(orderId, Volume.MustCreate(2), Location.Max);

        var orderRepository = new OrderRepository(DbContext);
        await orderRepository.AddAsync(order);

        var unitOfWork = new UnitOfWork(DbContext);
        await unitOfWork.SaveChangesAsync();

        // Act
        order.Assign(courier);
        orderRepository.Update(order);
        await unitOfWork.SaveChangesAsync();

        // Assert
        var getOrderResult = await orderRepository.GetAsync(order.Id, CancellationToken.None);
        getOrderResult.HasValue.Should().BeTrue();
        var orderFromDb = getOrderResult.Value;
        order.Should().BeEquivalentTo(orderFromDb);
    }

    [Fact]
    public async Task CanGetOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = Order.MustCreate(orderId, Volume.MustCreate(2), Location.Min);

        // Act
        var orderRepository = new OrderRepository(DbContext);
        await orderRepository.AddAsync(order);

        var unitOfWork = new UnitOfWork(DbContext);
        await unitOfWork.SaveChangesAsync();

        // Assert
        var getOrderResult = await orderRepository.GetAsync(order.Id, CancellationToken.None);
        getOrderResult.HasValue.Should().BeTrue();
        var orderFromDb = getOrderResult.Value;
        order.Should().BeEquivalentTo(orderFromDb);
    }

    [Fact]
    public async Task CanGetFirstByCreatedStatus()
    {
        // Arrange
        var courierId = Guid.NewGuid();

        var order1Id = Guid.NewGuid();
        var order1 = Order.MustCreate(order1Id, Volume.MustCreate(3), Location.Min);
        order1.Assign(courierId);

        var order2Id = Guid.NewGuid();
        var order2 = Order.MustCreate(order2Id,  Volume.MustCreate(7), Location.Min);

        var orderRepository = new OrderRepository(DbContext);
        await orderRepository.AddAsync(order1);
        await orderRepository.AddAsync(order2);

        var unitOfWork = new UnitOfWork(DbContext);
        await unitOfWork.SaveChangesAsync();

        // Act
        var result = await orderRepository.GetFirstByCreatedStatusAsync(CancellationToken.None);

        // Assert
        result.HasValue.Should().BeTrue();
        order2.Should().BeEquivalentTo(result.Value);
    }

    
    [Fact]
    public async Task GetAllByAssignedStatus()
    {
        //Arrange
        var courier1 = Courier.MustCreate( "Pedestrian", Location.MustCreate(1, 1));
        var courier2 = Courier.MustCreate( "Moto", Location.MustCreate(1, 1));

        var order1Id = Guid.NewGuid();
        var order1 = Order.MustCreate(order1Id, Volume.MustCreate(5), Location.Min);
        order1.Assign(Guid.NewGuid());

        var order2Id = Guid.NewGuid();
        var order2 = Order.MustCreate(order2Id, Volume.MustCreate(5), Location.Min);
        order2.Assign(Guid.NewGuid());
        
        var orderRepository = new OrderRepository(DbContext);
        await orderRepository.AddAsync(order1);
        await orderRepository.AddAsync(order2);

        var unitOfWork = new UnitOfWork(DbContext);
        await unitOfWork.SaveChangesAsync();

        //Act
        var result = await orderRepository.GetAllByAssignedStatusAsync(CancellationToken.None);

        // Assert
        result.Count().Should().Be(2);

        result.SingleOrDefault(x => x.Id == order1Id).Should().NotBeNull();
        
        result.SingleOrDefault(x => x.Id == order2Id).Should().NotBeNull();
    }
}
