using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using DeliveryApp.Infrastructure.Adapters.Postgres;
using DeliveryApp.Infrastructure.Adapters.Postgres.Repositories;
using FluentAssertions;
using Xunit;

namespace DeliveryApp.IntegrationTests.Repositories;

public class CourierRepositoryShould : IntegrationTestBase
{
    [Fact]
    public async Task CanAddCourier()
    {
        // Arrange
        var courier = Courier.MustCreate("Pedestrian", Location.Min);

        // Act
        var courierRepository = new CourierRepository(DbContext);
        var unitOfWork = new UnitOfWork(DbContext);
        await courierRepository.AddAsync(courier);
        await unitOfWork.SaveChangesAsync();

        // Assert
        var entity = await courierRepository.GetAsync(courier.Id, CancellationToken.None);
        entity.Should().BeEquivalentTo(courier);
    }

    [Fact]
    public async Task CanUpdateCourier()
    {
        // Arrange
        var order = Order.MustCreate(Guid.NewGuid(), Volume.MustCreate(5), Location.Min);

        var courier = Courier.MustCreate("Pedestrian", Location.Min);

        var courierRepository = new CourierRepository(DbContext);
        var unitOfWork = new UnitOfWork(DbContext);
        await courierRepository.AddAsync(courier);
        await unitOfWork.SaveChangesAsync();

        // Act
        var courierStartWorkResult = courier.TakeOrder(order);
        courierStartWorkResult.IsSuccess.Should().BeTrue();
        courierRepository.Update(courier);
        await unitOfWork.SaveChangesAsync();

        // Assert
        var result = await courierRepository.GetAsync(courier.Id, CancellationToken.None);
        result.Should().BeEquivalentTo(courier);
    }

    [Fact]
    public async Task CanGet()
    {
        // Arrange
        var courier = Courier.MustCreate("Pedestrian", Location.Min);

        // Act
        var courierRepository = new CourierRepository(DbContext);
        var unitOfWork = new UnitOfWork(DbContext);
        await courierRepository.AddAsync(courier);
        await unitOfWork.SaveChangesAsync();

        // Assert
        var result = await courierRepository.GetAsync(courier.Id,  CancellationToken.None);
        result.Should().BeEquivalentTo(courier);
    }

    [Fact]
    public async Task CanGetAll()
    {
        // Arrange
        var courier1 = Courier.MustCreate("Pedestrian", Location.Min);
        var courier2 = Courier.MustCreate("Moto", Location.Min);

        // Act
        var courierRepository = new CourierRepository(DbContext);
        var unitOfWork = new UnitOfWork(DbContext);
        await courierRepository.AddAsync(courier1);
        await courierRepository.AddAsync(courier2);
        await unitOfWork.SaveChangesAsync();

        // Assert
        var result = await courierRepository.GetAllAsync(CancellationToken.None);

        // Assert
        result.Count().Should().Be(2);

        result.SingleOrDefault(x => x.Id == courier1.Id).Should().BeEquivalentTo(courier1);
        
        result.SingleOrDefault(x => x.Id == courier2.Id).Should().BeEquivalentTo(courier2);
    }
}