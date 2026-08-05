using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using DeliveryApp.Api.Adapters.Http;
using DeliveryApp.Core.Application.UseCases.Commands.CreateOrder;
using Errs;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using OpenApi.Models;
using Xunit;
using Error = Errs.Error;

namespace DeliveryApp.UnitTests.Ui.Http;

public class CreateOrderControllerShould
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();

    [Fact]
    public async Task CreateOrderCorrectly()
    {
        // Arrange
        _mediator.Send(Arg.Any<CreateOrderCommand>())
            .Returns(UnitResult.Success<Error>());

        // Act
        var basketController = new CreateOrderController(_mediator);
        var newOrder = new NewOrder()
        {
            Id = Guid.NewGuid(),
            Address = new Address()
            {
                Country = "Russia",
                Apartment = "4",
                City = "Spb",
                House = "3",
                Street = "123 Main St"
            },
            Volume = 3
        };

        var result = await basketController.CreateOrder(newOrder);

        // Assert
        var objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(StatusCodes.Status201Created);
        objectResult.Value.Should().BeOfType<CreateOrderResponse>()
            .Which.OrderId.Should().Be(newOrder.Id);
    }

    [Fact]
    public async Task ReturnConflictWhenCreateNotCorrectly()
    {
        // Arrange
        _mediator.Send(Arg.Any<CreateOrderCommand>())
            .Returns(UnitResult.Failure(GeneralErrors.ValueIsRequired("orderId")));

        // Act
        var basketController = new CreateOrderController(_mediator);
        var newOrder = new NewOrder()
        {
            Id = Guid.NewGuid(),
            Address = new Address()
            {
                Country = "Russia",
                Apartment = "4",
                City = "Spb",
                House = "3",
                Street = "123 Main St"
            },
            Volume = 3
        };
        
        var result = await basketController.CreateOrder(newOrder);
        // Assert
        result.Should().BeOfType<ConflictObjectResult>();
    }
}