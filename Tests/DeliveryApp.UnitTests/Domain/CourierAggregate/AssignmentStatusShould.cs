using DeliveryApp.Core.Domain.Model.CourierAggregate;
using FluentAssertions;
using Xunit;

namespace DeliveryApp.UnitTests.Domain.CourierAggregate;

public class AssignmentStatusShould
{
    [Fact]
    public void BeEqualWhenNameIsEqual()
    {
        // Arrange
        var first = AssignmentStatus.Assigned;
        var second = AssignmentStatus.Assigned;

        // Act
        var result = first == second;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void BeNotEqualWhenNameIsNotEqual()
    {
        // Arrange
        var first = AssignmentStatus.Assigned;
        var second = AssignmentStatus.Completed;

        // Act
        var result = first == second;

        // Assert
        result.Should().BeFalse();
    }
}
