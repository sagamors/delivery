using System;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using Errs;
using FluentAssertions;
using Xunit;

namespace DeliveryApp.UnitTests.Domain.OrderAggregate;

public class OrderShould
{
    [Fact]
    public void BeCorrectWhenParamsAreCorrectOnCreated()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var volume = Volume.MustCreate(5);
        var location = Location.MustCreate(1, 1);

        // Act
        var result = Order.Create(orderId, volume, location);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(orderId);
        result.Value.Volume.Should().Be(volume);
        result.Value.Location.Should().Be(location);
        result.Value.Status.Should().Be(OrderStatus.Created);
    }

    [Fact]
    public void ReturnErrorWhenOrderIdIsEmptyOnCreated()
    {
        // Arrange

        // Act
        var result = Order.Create(Guid.Empty, Volume.MustCreate(5), Location.MustCreate(1, 1));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(GeneralErrors.ValueIsRequired("orderId"));
    }

    [Fact]
    public void ReturnErrorWhenVolumeIsNullOnCreated()
    {
        // Arrange

        // Act
        var result = Order.Create(Guid.NewGuid(), null!, Location.MustCreate(1, 1));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(GeneralErrors.ValueIsRequired("volume"));
    }

    [Fact]
    public void ReturnErrorWhenLocationIsNullOnCreated()
    {
        // Arrange

        // Act
        var result = Order.Create(Guid.NewGuid(), Volume.MustCreate(5), null!);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(GeneralErrors.ValueIsRequired("location"));
    }

    [Fact]
    public void BeEqualWhenIdIsEqual()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var first = Order.Create(orderId, Volume.MustCreate(5), Location.MustCreate(1, 1)).Value;
        var second = Order.Create(orderId, Volume.MustCreate(7), Location.MustCreate(3, 3)).Value;

        // Act
        var result = first.Equals(second);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void BeNotEqualWhenIdIsNotEqual()
    {
        // Arrange
        var first = Order.Create(Guid.NewGuid(), Volume.MustCreate(5), Location.MustCreate(1, 1)).Value;
        var second = Order.Create(Guid.NewGuid(), Volume.MustCreate(5), Location.MustCreate(1, 1)).Value;

        // Act
        var result = first.Equals(second);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void AssignWhenCourierIsInSameCell()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), Volume.MustCreate(5), Location.MustCreate(1, 1)).Value;
        var courier = Courier.Create("Test Courier", Location.Create(1, 1).Value).Value;

        // Act
        var result = order.Assign(courier.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Assigned);
    }

    [Fact]
    public void AssignWhenCourierIsOneCellAway()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), Volume.MustCreate(5), Location.MustCreate(1, 1)).Value;
        var courier = Courier.Create("Test Courier", Location.Create(2, 1).Value).Value;

        // Act
        var result = order.Assign(courier.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Assigned);
    }

    [Fact]
    public void ReturnErrorWhenAlreadyAssignedOnAssign()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), Volume.MustCreate(5), Location.MustCreate(1, 1)).Value;
        var courier = Courier.Create("Test Courier", Location.Create(2, 1).Value).Value;
        order.Assign(courier.Id);

        // Act
        var result = order.Assign(courier.Id);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Order.Errors.NotCreated());
    }

    [Fact]
    public void CompleteWhenCorrect()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), Volume.MustCreate(5), Location.MustCreate(1, 1)).Value;
        var courier = Courier.Create("Test Courier", Location.Create(1, 1).Value).Value;
        order.Assign(courier.Id);

        // Act
        var result = order.Complete();

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Completed);
    }
    
    [Fact]
    public void ReturnErrorWhenAlreadyCompletedOnComplete()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), Volume.MustCreate(5), Location.MustCreate(1, 1)).Value;
        var courier = Courier.Create("Test Courier", Location.Create(1, 1).Value).Value;
        order.Assign(courier.Id);
        order.Complete();

        // Act
        var result = order.Complete();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Order.Errors.AlreadyCompleted());
    }

    [Fact]
    public void ReturnErrorWhenNotAssignedOnComplete()
    {
        // Arrange
        var order = Order.Create(Guid.NewGuid(), Volume.MustCreate(5), Location.MustCreate(1, 1)).Value;
        var courier = Courier.Create("Test Courier", Location.Create(1, 1).Value).Value;
        
        // Act
        var result = order.Complete();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Order.Errors.NotAssigned());
    }
}