using CSharpFunctionalExtensions;
using Ddd;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using DeliveryApp.Core.Ports;
using Errs;
using MediatR;

namespace DeliveryApp.Core.Application.UseCases.Commands.CreateCourier;

public class CreateCourierHandler(IUnitOfWork unitOfWork, ICourierRepository courierRepository)
    : IRequestHandler<CreateCourierCommand, Result<Guid, Error>>
{
    public async Task<Result<Guid, Error>> Handle(CreateCourierCommand message, CancellationToken cancellationToken)
    {
        var location = Location.CreateRandom();
        var courierCreateResult = Courier.Create(message.Name, location);
        if (courierCreateResult.IsFailure)
        {
            return courierCreateResult.Error;
        }

        var courier = courierCreateResult.Value;

        await courierRepository.AddAsync(courier);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return courier.Id;
    }
}
