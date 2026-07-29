using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using Errs;
using MediatR;

namespace DeliveryApp.Core.Application.UseCases.Commands.MoveCourier;

public class MoveCourierCommand: IRequest<UnitResult<Error>>
{
    private MoveCourierCommand(Guid courierId, Location location)
    {
        CourierId = courierId;
        Location = location;
    }

    /// <summary>
    ///     Factory Method
    /// </summary>
    /// <param name="courierId">Идентификатор курьера</param>
    /// <param name="location">Локация</param>
    /// <returns>Результат</returns>
    public static Result<MoveCourierCommand, Error> Create(Guid courierId, Location location)
    {
        if (courierId == Guid.Empty) return GeneralErrors.ValueIsRequired(nameof(courierId));
        if (location == null) return GeneralErrors.ValueIsRequired(nameof(location));

        return new MoveCourierCommand(courierId, location);
    }
    
    /// <summary>
    ///     Идентификатор курьера
    /// </summary>
    public Guid CourierId { get; }
    
    /// <summary>
    ///     Локация
    /// </summary>
    public Location Location { get; }
}