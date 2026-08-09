using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using Errs;

namespace DeliveryApp.Core.Ports;

/// <summary>
///     Сервис получения геолокации по адресу
/// </summary>
public interface IGeoService
{
    /// <summary>
    ///     Получить геолокацию по адресу
    /// </summary>
    /// <param name="street">Адрес (улица)</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Локация</returns>
    Task<Result<Location, Error>> GetGeolocationAsync(string street, CancellationToken cancellationToken);
}
