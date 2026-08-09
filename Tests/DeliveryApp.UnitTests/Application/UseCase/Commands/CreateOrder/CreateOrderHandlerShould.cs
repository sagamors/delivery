using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Ddd;
using DeliveryApp.Core.Application.UseCases.Commands.CreateOrder;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using DeliveryApp.Core.Ports;
using Errs;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace DeliveryApp.UnitTests.Application.UseCase.Commands.CreateOrder;

public class CreateOrderHandlerShould
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderRepository _orderRepository;
    private readonly IGeoService _geoService;
    private readonly CreateOrderHandler _handler;

    public CreateOrderHandlerShould()
    {
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _orderRepository = Substitute.For<IOrderRepository>();
        _geoService = Substitute.For<IGeoService>();
        _handler = new CreateOrderHandler(_unitOfWork, _orderRepository, _geoService);
    }

    [Fact]
    public async Task  CreateOrderSuccessfullyWhenOrderDoesNotExist()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        const string country = "Test Country";
        const string city = "Test City";
        const string street = "Test Street";
        const string house = "1";
        const string apart = "1";
        var volume = Volume.MustCreate(5);
        var location = Location.MustCreate(3, 4);

        var command = CreateOrderCommand.Create(orderId, country: country, city, street,  house, apart, volume.Value).Value;

        _orderRepository.GetAsync(orderId, CancellationToken.None).Returns(Task.FromResult<Maybe<Order>>(null));
        _geoService.GetGeolocationAsync(street, CancellationToken.None).Returns(Task.FromResult<Result<Location, Error>>(location));
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(true));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _orderRepository.Received(1).GetAsync(orderId, CancellationToken.None);
        await _geoService.Received(1).GetGeolocationAsync(street, CancellationToken.None);
        await _orderRepository.Received(1).AddAsync(Arg.Is<Order>(o =>
            o.Id == orderId &&
            o.Volume == volume &&
            o.Location == location &&
            o.Status == OrderStatus.Created));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateOrderSuccessfullyWhenOrderExists()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        const string country = "Test Country";
        const string city = "Test City";
        const string street = "Test Street";
        const string house = "1";
        const string apart = "1";
        var volume = Volume.MustCreate(5);

        var command = CreateOrderCommand.Create(orderId, country: country, city, street,  house, apart, volume.Value).Value;

        var existingOrder = Order.Create(orderId, volume, Location.CreateRandom()).Value;
        _orderRepository.GetAsync(orderId, CancellationToken.None).Returns(Task.FromResult<Maybe<Order>>(existingOrder));
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(true));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        await _orderRepository.Received(1).GetAsync(orderId, CancellationToken.None);
        await _geoService.DidNotReceive().GetGeolocationAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _orderRepository.DidNotReceive().AddAsync(Arg.Any<Order>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}