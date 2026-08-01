using CSharpFunctionalExtensions;
using Ddd;
using DeliveryApp.Core.Ports;
using Errs;
using MediatR;

namespace DeliveryApp.Core.Application.UseCases.Commands.MoveCourier;

public class MoveCourierHandler(
    ICourierRepository courierRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<MoveCourierCommand, UnitResult<Error>>
{
    public async Task<UnitResult<Error>> Handle(MoveCourierCommand command, CancellationToken cancellationToken)
    {
        var courier = await courierRepository.GetAsync(command.CourierId, cancellationToken);
        if (courier.HasNoValue)
            return GeneralErrors.ValueIsInvalid(nameof(command.CourierId), courier);

        var moveResult = courier.Value.Move(command.Location);
        if (moveResult.IsFailure)
            return moveResult.Error;
        
        courierRepository.Update(courier.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return UnitResult.Success<Error>();
    }
}