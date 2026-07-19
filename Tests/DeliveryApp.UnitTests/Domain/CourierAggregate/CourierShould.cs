using System;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using Errs;
using FluentAssertions;
using Xunit;

namespace DeliveryApp.UnitTests.Domain.CourierAggregate;

public class CourierShould
{
    [Fact]
    public void BeCorrectWhenParamsAreCorrectOnCreated()
    {
        // Arrange
        var name = "Test Courier";
        var location = Location.MustCreate(1, 1);

        // Act
        var result = Courier.Create(name, location);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(name);
        result.Value.Location.Should().Be(location);
        result.Value.MaxVolume.Should().Be(Volume.MustCreate(20));
        result.Value.Assignments.Should().BeEmpty();
    }

    [Fact]
    public void ReturnErrorWhenNameIsEmptyOnCreated()
    {
        // Arrange

        // Act
        var result = Courier.Create(string.Empty, Location.MustCreate(1, 1));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(GeneralErrors.ValueIsRequired("name"));
    }

    [Fact]
    public void ReturnErrorWhenLocationIsNullOnCreated()
    {
        // Arrange

        // Act
        var result = Courier.Create("Test Courier", null!);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(GeneralErrors.ValueIsRequired("location"));
    }

    [Fact]
    public void CanTakeOrderWhenVolumeFitsMax()
    {
        // Arrange
        var courier = Courier.Create("Test Courier", Location.MustCreate(1, 1)).Value;
        var order = Order.Create(Guid.NewGuid(), Volume.MustCreate(20), Location.MustCreate(1, 1)).Value;

        // Act
        var result = courier.CanTakeOrder(order);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public void ReturnErrorWhenOrderIsNullOnCanTakeOrder()
    {
        // Arrange
        var courier = Courier.Create("Test Courier", Location.MustCreate(1, 1)).Value;

        // Act
        var result = courier.CanTakeOrder(null!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(GeneralErrors.ValueIsRequired("order"));
    }

    [Fact]
    public void NotTakeOrderWhenVolumeExceedsMax()
    {
        // Arrange
        var courier = Courier.Create("Test Courier", Location.MustCreate(1, 1)).Value;
        var firstOrder = Order.Create(Guid.NewGuid(), Volume.MustCreate(20), Location.MustCreate(1, 1)).Value;
        var secondOrder = Order.Create(Guid.NewGuid(), Volume.MustCreate(1), Location.MustCreate(1, 1)).Value;
        courier.TakeOrder(firstOrder);

        // Act
        var result = courier.CanTakeOrder(secondOrder);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Courier.Errors.CanNotTakeOrder());
    }

    [Fact]
    public void TakeOrderWhenVolumeFitsMax()
    {
        // Arrange
        var courier = Courier.Create("Test Courier", Location.MustCreate(1, 1)).Value;
        var order = Order.Create(Guid.NewGuid(), Volume.MustCreate(5), Location.MustCreate(1, 1)).Value;

        // Act
        var result = courier.TakeOrder(order);

        // Assert
        result.IsSuccess.Should().BeTrue();
        courier.Assignments.Should().ContainSingle(a => a.OrderId == order.Id);
    }

    [Fact]
    public void ReturnErrorInsteadOfThrowingWhenVolumeExceedsMaxOnTakeOrder()
    {
        // Arrange
        var courier = Courier.Create("Test Courier", Location.MustCreate(1, 1)).Value;
        var firstOrder = Order.Create(Guid.NewGuid(), Volume.MustCreate(20), Location.MustCreate(1, 1)).Value;
        var secondOrder = Order.Create(Guid.NewGuid(), Volume.MustCreate(1), Location.MustCreate(1, 1)).Value;
        courier.TakeOrder(firstOrder);

        // Act
        var result = courier.TakeOrder(secondOrder);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Courier.Errors.CanNotTakeOrder());
        courier.Assignments.Should().ContainSingle();
    }

    [Fact]
    public void CompleteOrderWhenCourierIsCloseEnough()
    {
        // Arrange
        var courier = Courier.Create("Test Courier", Location.MustCreate(1, 1)).Value;
        var order = Order.Create(Guid.NewGuid(), Volume.MustCreate(5), Location.MustCreate(1, 1)).Value;
        courier.TakeOrder(order);

        // Act
        var result = courier.CompleteOrder(order);

        // Assert
        result.IsSuccess.Should().BeTrue();
        courier.Assignments.Should().BeEmpty();
    }

    [Fact]
    public void ReturnErrorWhenAssignmentNotFoundOnCompleteOrder()
    {
        // Arrange
        var courier = Courier.Create("Test Courier", Location.MustCreate(1, 1)).Value;
        var order = Order.Create(Guid.NewGuid(), Volume.MustCreate(5), Location.MustCreate(1, 1)).Value;

        // Act
        var result = courier.CompleteOrder(order);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Courier.Errors.AssignmentNotFound());
    }

    [Fact]
    public void MoveToTargetWhenOneStepAway()
    {
        // Arrange
        var courier = Courier.Create("Test Courier", Location.MustCreate(1, 1)).Value;
        var target = Location.MustCreate(1, 2);

        // Act
        var result = courier.Move(target);

        // Assert
        result.IsSuccess.Should().BeTrue();
        courier.Location.Should().Be(target);
    }

    [Fact]
    public void ReturnErrorWhenTargetIsTooFarOnMove()
    {
        // Arrange
        var courier = Courier.Create("Test Courier", Location.MustCreate(1, 1)).Value;
        var target = Location.MustCreate(10, 10);

        // Act
        var result = courier.Move(target);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Courier.Errors.CantMoveThatFar());
        courier.Location.Should().Be(Location.MustCreate(1, 1));
    }

    [Fact]
    public void ReturnErrorWhenTargetIsNullOnMove()
    {
        // Arrange
        var courier = Courier.Create("Test Courier", Location.MustCreate(1, 1)).Value;

        // Act
        var result = courier.Move(null!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(GeneralErrors.ValueIsRequired("target"));
    }
}
