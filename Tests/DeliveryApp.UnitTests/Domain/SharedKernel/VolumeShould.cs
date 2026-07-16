using DeliveryApp.Core.Domain.Model.SharedKernel;
using FluentAssertions;
using Xunit;

namespace DeliveryApp.UnitTests.Domain.SharedKernel;

public class VolumeShould
{
    [Fact]
    public void BeCorrectWhenParamsAreCorrectOnCreated()
    {
        // Arrange

        // Act
        var result = Volume.Create(5);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(5);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ReturnErrorWhenValueIsNotCorrectOnCreated(int val)
    {
        // Arrange

        // Act
        var result = Volume.Create(val);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void BeEqualWhenValueIsEqual()
    {
        // Arrange
        var first = Volume.MustCreate(5);
        var second = Volume.MustCreate(5);

        // Act
        var result = first == second;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void BeNotEqualWhenValueIsNotEqual()
    {
        // Arrange
        var first = Volume.MustCreate(5);
        var second = Volume.MustCreate(7);

        // Act
        var result = first == second;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ReturnSumWhenAdded()
    {
        // Arrange
        var first = Volume.MustCreate(5);
        var second = Volume.MustCreate(7);

        // Act
        var result = first + second;

        // Assert
        result.Should().Be(Volume.MustCreate(12));
    }

    [Fact]
    public void BeGreaterWhenValueIsBigger()
    {
        // Arrange
        var bigger = Volume.MustCreate(10);
        var smaller = Volume.MustCreate(5);

        // Act & Assert
        (bigger > smaller).Should().BeTrue();
        (smaller < bigger).Should().BeTrue();
        (bigger >= smaller).Should().BeTrue();
        (smaller <= bigger).Should().BeTrue();
        (bigger < smaller).Should().BeFalse();
        (smaller > bigger).Should().BeFalse();
    }
}
