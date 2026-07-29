using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using Errs;
using MediatR;

namespace DeliveryApp.Core.Application.UseCases.Commands;

/// <summary>
///     Создать заказ
/// </summary>
public class CreateOrderCommand : IRequest<UnitResult<Error>>
{
    private CreateOrderCommand(Guid orderId, string country, string city, string street,
        string house, string apartment, Volume volume)
    {
        OrderId = orderId;
        Country = country;
        City = city;
        Street = street;
        House = house;
        Apartment = apartment;
        Volume = volume;
    }
    
    /// <summary>
    ///     Factory Method
    /// </summary>
    /// <param name="orderId">Идентификатор заказа</param>
    /// <param name="country">Страна</param>
    /// <param name="city">Город</param>
    /// <param name="street">Улица</param>
    /// <param name="house">Дом</param>
    /// <param name="apartment">Квартира</param>
    /// <param name="volume">Объем</param>
    /// <returns>Результат</returns>
    public static Result<CreateOrderCommand, Error> Create(Guid orderId, string country, string city, string street,
        string house, string apartment, Volume volume)
    {
        if (orderId == Guid.Empty) return GeneralErrors.ValueIsRequired(nameof(orderId));
        if (string.IsNullOrWhiteSpace(street)) return GeneralErrors.ValueIsRequired(nameof(street));
        if (volume == null) return GeneralErrors.ValueIsRequired(nameof(volume));

        return new CreateOrderCommand(orderId, country, city, street, house, apartment, volume);
    }

    /// <summary>
    ///     Идентификатор заказа
    /// </summary>
    public Guid OrderId { get; }

    /// <summary>
    ///     Страна
    /// </summary>
    public string Country { get; private set; }

    /// <summary>
    ///     Город
    /// </summary>
    public string City { get; private set; }

    /// <summary>
    ///     Улица
    /// </summary>
    public string Street { get; private set; }

    /// <summary>
    ///     Дом
    /// </summary>
    public string House { get; private set; }

    /// <summary>
    ///     Квартира
    /// </summary>
    public string Apartment { get; private set; }
    
    /// <summary>
    ///     Объем
    /// </summary>
    public Volume Volume { get; }
}