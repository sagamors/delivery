using OpenApi.Models;
using DomainError = Errs.Error;

namespace DeliveryApp.Api.Adapters.Http;

internal static class ErrorMappingExtensions
{
    public static Error ToApiError(this DomainError error, int statusCode)
    {
        return new Error
        {
            Code = statusCode,
            Message = error.Message
        };
    }
}
