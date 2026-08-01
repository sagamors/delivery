using CSharpFunctionalExtensions;
using Errs;
using MediatR;

namespace DeliveryApp.Core.Application.UseCases.Commands.AssignOrder;

public class AssignOrderCommand : IRequest<UnitResult<Error>>;
