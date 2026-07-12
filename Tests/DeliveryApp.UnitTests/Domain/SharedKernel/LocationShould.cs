using System.Collections.Generic;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using FluentAssertions;
using Xunit;

namespace DeliveryApp.UnitTests.Domain.SharedKernel;

public class LocationShould
{ 
    [Fact]
    public void BeCorrectWhenParamsAreCorrectOnCreated()
    {
        // Arrange

        // Act
        var result = Location.Create(5,8);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.X.Should().Be(5);
        result.Value.Y.Should().Be(8);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(11)]
    public void ReturnErrorWhenXAreNotCorrectOnCreated(int val)
    {
        // Arrange

        // Act
        var result = Location.Create(val, 1);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(11)]
    public void ReturnErrorWhenYAreNotCorrectOnCreated(int val)
    {
        // Arrange

        // Act
        var result = Location.Create(1, val);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public void BeEqualWhenAllPropertiesIsEqual()
    {
        // Arrange
        var first = Location.MustCreate(1, 2);
        var second =  Location.MustCreate(1, 2);

        // Act
        var result = first == second;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void BeNotEqualWhenOneOfPropertiesIsNotEqual()
    {
        // Arrange
        var first = Location.MustCreate(1, 2);
        var second =  Location.MustCreate(1, 6);

        // Act
        var result = first == second;

        // Assert
        result.Should().BeFalse();
    }
    
    [Theory]
    [MemberData(nameof(Data))]
    public void CalculateDistance(Location l1, Location l2, int distance)
    {
        //Arrange

        //Act
        var steps = l1.DistanceTo(l2);

        //Assert
        steps.Should().Be(distance);
    }
    
    public static IEnumerable<object[]> Data =>
        new List<object[]>
        {
            new object[] { Location.Min, Location.Min, 0 },
            new object[] { Location.Max, Location.Max, 0 },
            new object[] { Location.Max, Location.Min, 18 },
            new object[] { Location.Min, Location.Max, 18 },
            new object[] { Location.MustCreate(3,2), Location.MustCreate(6,3), 4 },
            new object[] { Location.MustCreate(8,9), Location.MustCreate(1,2), 14 },
        };
}