using System;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using Errs;
using FluentAssertions;
using Xunit;

namespace DeliveryApp.UnitTests.Domain.CourierAggregate;

public class AssignmentShould
{
    [Fact]
    public void BeCorrectWhenParamsAreCorrectOnCreated()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var volume = Volume.MustCreate(5);
        var location = Location.MustCreate(1, 1);

        // Act
        var result = Assignment.Create(orderId, volume, location);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.OrderId.Should().Be(orderId);
        result.Value.Volume.Should().Be(volume);
        result.Value.Location.Should().Be(location);
        result.Value.Status.Should().Be(AssignmentStatus.Assigned);
    }

    [Fact]
    public void ReturnErrorWhenOrderIdIsEmptyOnCreated()
    {
        // Arrange

        // Act
        var result = Assignment.Create(Guid.Empty, Volume.MustCreate(5), Location.MustCreate(1, 1));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(GeneralErrors.ValueIsRequired("orderId"));
    }

    [Fact]
    public void ReturnErrorWhenVolumeIsNullOnCreated()
    {
        // Arrange

        // Act
        var result = Assignment.Create(Guid.NewGuid(), null!, Location.MustCreate(1, 1));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(GeneralErrors.ValueIsRequired("volume"));
    }

    [Fact]
    public void ReturnErrorWhenLocationIsNullOnCreated()
    {
        // Arrange

        // Act
        var result = Assignment.Create(Guid.NewGuid(), Volume.MustCreate(5), null!);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(GeneralErrors.ValueIsRequired("location"));
    }

    [Fact]
    public void BeEqualWhenIdIsEqual()
    {
        // Arrange
        var first = Assignment.Create(Guid.NewGuid(), Volume.MustCreate(5), Location.MustCreate(1, 1)).Value;
        var second = Assignment.Create(Guid.NewGuid(), Volume.MustCreate(7), Location.MustCreate(3, 3)).Value;
        typeof(Assignment).GetProperty(nameof(Assignment.Id))!.SetValue(second, first.Id);

        // Act
        var result = first.Equals(second);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void BeNotEqualWhenIdIsNotEqual()
    {
        // Arrange
        var first = Assignment.Create(Guid.NewGuid(), Volume.MustCreate(5), Location.MustCreate(1, 1)).Value;
        var second = Assignment.Create(Guid.NewGuid(), Volume.MustCreate(5), Location.MustCreate(1, 1)).Value;

        // Act
        var result = first.Equals(second);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CompleteWhenCourierIsInSameCell()
    {
        // Arrange
        var assignment = Assignment.Create(Guid.NewGuid(), Volume.MustCreate(5), Location.MustCreate(5, 5)).Value;

        // Act
        var result = assignment.Complete(Location.MustCreate(5, 5));

        // Assert
        result.IsSuccess.Should().BeTrue();
        assignment.Status.Should().Be(AssignmentStatus.Completed);
    }

    [Fact]
    public void CompleteWhenCourierIsOneCellAway()
    {
        // Arrange
        var assignment = Assignment.Create(Guid.NewGuid(), Volume.MustCreate(5), Location.MustCreate(5, 5)).Value;

        // Act
        var result = assignment.Complete(Location.MustCreate(5, 6));

        // Assert
        result.IsSuccess.Should().BeTrue();
        assignment.Status.Should().Be(AssignmentStatus.Completed);
    }

    [Fact]
    public void ReturnErrorWhenCourierIsTooFarOnComplete()
    {
        // Arrange
        var assignment = Assignment.Create(Guid.NewGuid(), Volume.MustCreate(5), Location.MustCreate(1, 1)).Value;

        // Act
        var result = assignment.Complete(Location.MustCreate(10, 10));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Assignment.Errors.CourierTooFar());
        assignment.Status.Should().Be(AssignmentStatus.Assigned);
    }

    [Fact]
    public void ReturnErrorWhenAlreadyCompletedOnComplete()
    {
        // Arrange
        var assignment = Assignment.Create(Guid.NewGuid(), Volume.MustCreate(5), Location.MustCreate(5, 5)).Value;
        assignment.Complete(Location.MustCreate(5, 5));

        // Act
        var result = assignment.Complete(Location.MustCreate(5, 5));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Assignment.Errors.AlreadyCompleted());
    }

    [Fact]
    public void ReturnErrorWhenCourierLocationIsNullOnComplete()
    {
        // Arrange
        var assignment = Assignment.Create(Guid.NewGuid(), Volume.MustCreate(5), Location.MustCreate(5, 5)).Value;

        // Act
        var result = assignment.Complete(null!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(GeneralErrors.ValueIsRequired("courierLocation"));
    }
}
