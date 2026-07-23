using CSharpFunctionalExtensions;
using Ddd;
using DeliveryApp.Core.Domain.Model.CourierAggregate;

namespace DeliveryApp.Core.Ports;

/// <summary>
///     Repository для Aggregate Courier
/// </summary>
public interface ICourierRepository : IRepository<Courier>
{
    /// <summary>
    ///     Добавить нового курьера
    /// </summary>
    /// <param name="courier">Курьер</param>
    Task AddAsync(Courier courier);

    /// <summary>
    ///     Обновить существующего курьера
    /// </summary>
    /// <param name="courier">Курьер</param>
    void Update(Courier courier);

    /// <summary>
    ///     Получить курьера по идентификатору
    /// </summary>
    /// <param name="сourierId">Идентификатор</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Курьер</returns>
    Task<Maybe<Courier>> GetAsync(Guid сourierId, CancellationToken cancellationToken);
    
    /// <summary>
    ///     Получить всех курьеров
    /// </summary>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Заказ</returns>
    Task<IReadOnlyCollection<Courier>> GetAllAsync(CancellationToken cancellationToken);
}