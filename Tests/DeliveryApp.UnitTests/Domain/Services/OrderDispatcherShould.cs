using System;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using DeliveryApp.Core.Domain.Services;
using Errs;
using FluentAssertions;
using Xunit;

namespace DeliveryApp.UnitTests.Domain.Services;

public class OrderDispatcherShould
{
    [Fact]
    public void DispatchToTheOnlyEligibleCourier()
    {
        // Arrange
        var dispatcher = new OrderDispatcher();
        var order = Order.Create(Guid.NewGuid(), Volume.MustCreate(5), Location.MustCreate(3, 3)).Value;
        var courier = Courier.Create("Test Courier", Location.MustCreate(1, 1)).Value;

        // Act
        var result = dispatcher.Dispatch(order, new[] { courier });

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(courier);
        order.Status.Should().Be(OrderStatus.Assigned);
        order.CourierId.Should().Be(courier.Id);
        courier.Assignments.Should().ContainSingle(a => a.OrderId == order.Id);
    }

    [Fact]
    public void DispatchToTheClosestCourier()
    {
        // Arrange
        var dispatcher = new OrderDispatcher();
        var order = Order.Create(Guid.NewGuid(), Volume.MustCreate(5), Location.MustCreate(5, 5)).Value;
        var farCourier = Courier.Create("Far Courier", Location.MustCreate(1, 1)).Value;
        var nearCourier = Courier.Create("Near Courier", Location.MustCreate(5, 4)).Value;

        // Act
        var result = dispatcher.Dispatch(order, new[] { farCourier, nearCourier });

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(nearCourier);
    }

    [Fact]
    public void SkipOverfullCourierInFavorOfFartherOneWithCapacity()
    {
        // Arrange
        var dispatcher = new OrderDispatcher();
        var order = Order.Create(Guid.NewGuid(), Volume.MustCreate(5), Location.MustCreate(5, 5)).Value;

        var closeButFullCourier = Courier.Create("Close Courier", Location.MustCreate(5, 4)).Value;
        var bigOrder = Order.Create(Guid.NewGuid(), Volume.MustCreate(20), Location.MustCreate(5, 4)).Value;
        closeButFullCourier.TakeOrder(bigOrder);

        var farCourierWithCapacity = Courier.Create("Far Courier", Location.MustCreate(1, 1)).Value;

        // Act
        var result = dispatcher.Dispatch(order, new[] { closeButFullCourier, farCourierWithCapacity });

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(farCourierWithCapacity);
    }

    [Fact]
    public void ReturnErrorWhenAllCouriersAreOverfull()
    {
        // Arrange
        var dispatcher = new OrderDispatcher();
        var order = Order.Create(Guid.NewGuid(), Volume.MustCreate(5), Location.MustCreate(5, 5)).Value;
        var courier = Courier.Create("Test Courier", Location.MustCreate(5, 5)).Value;
        var bigOrder = Order.Create(Guid.NewGuid(), Volume.MustCreate(20), Location.MustCreate(5, 5)).Value;
        courier.TakeOrder(bigOrder);

        // Act
        var result = dispatcher.Dispatch(order, new[] { courier });

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(OrderDispatcher.Errors.NoSuitableCourier());
    }

    [Fact]
    public void ReturnErrorWhenNoCouriersProvided()
    {
        // Arrange
        var dispatcher = new OrderDispatcher();
        var order = Order.Create(Guid.NewGuid(), Volume.MustCreate(5), Location.MustCreate(5, 5)).Value;

        // Act
        var result = dispatcher.Dispatch(order, Array.Empty<Courier>());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(OrderDispatcher.Errors.NoSuitableCourier());
    }

    [Fact]
    public void ReturnErrorWhenOrderIsNotCreated()
    {
        // Arrange
        var dispatcher = new OrderDispatcher();
        var order = Order.Create(Guid.NewGuid(), Volume.MustCreate(5), Location.MustCreate(5, 5)).Value;
        var courier = Courier.Create("Test Courier", Location.MustCreate(5, 5)).Value;
        order.Assign(courier.Id);

        // Act
        var result = dispatcher.Dispatch(order, new[] { courier });

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Order.Errors.NotCreated());
        courier.Assignments.Should().BeEmpty();
    }

    [Fact]
    public void ReturnErrorWhenOrderIsNull()
    {
        // Arrange
        var dispatcher = new OrderDispatcher();
        var courier = Courier.Create("Test Courier", Location.MustCreate(5, 5)).Value;

        // Act
        var result = dispatcher.Dispatch(null!, new[] { courier });

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(GeneralErrors.ValueIsRequired("order"));
    }

    [Fact]
    public void ReturnErrorWhenCouriersIsNull()
    {
        // Arrange
        var dispatcher = new OrderDispatcher();
        var order = Order.Create(Guid.NewGuid(), Volume.MustCreate(5), Location.MustCreate(5, 5)).Value;

        // Act
        var result = dispatcher.Dispatch(order, null!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(GeneralErrors.ValueIsRequired("couriers"));
    }
}
