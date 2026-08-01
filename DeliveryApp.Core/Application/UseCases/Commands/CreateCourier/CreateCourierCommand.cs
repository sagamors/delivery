using CSharpFunctionalExtensions;
using Errs;
using MediatR;

namespace DeliveryApp.Core.Application.UseCases.Commands.CreateCourier;

/// <summary>
///     Создать курьера
/// </summary>
public class CreateCourierCommand : IRequest<UnitResult<Error>>
{
    private CreateCourierCommand(string name)
    {
        Name = name;
    }

    /// <summary>
    ///     Factory Method
    /// </summary>
    /// <param name="name">Имя курьера</param>
    /// <returns>Результат</returns>
    public static Result<CreateCourierCommand, Error> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return GeneralErrors.ValueIsRequired(nameof(name));

        return new CreateCourierCommand(name);
    }

    /// <summary>
    ///     Имя курьера
    /// </summary>
    public string Name { get; }
}
