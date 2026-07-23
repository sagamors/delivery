using CSharpFunctionalExtensions;
using Ddd;
using DeliveryApp.Core.Domain.Model.OrderAggregate;

namespace DeliveryApp.Core.Ports;

/// <summary>
///     Repository для Aggregate Order
/// </summary>
public interface IOrderRepository : IRepository<Order>
{
    /// <summary>
    ///     Добавить новый заказ
    /// </summary>
    /// <param name="order">Заказ</param>
    Task AddAsync(Order order);

    /// <summary>
    ///     Обновить существующий заказ
    /// </summary>
    /// <param name="order">Заказ</param>
    void Update(Order order);

    /// <summary>
    ///     Получить заказ по идентификатору
    /// </summary>
    /// <param name="orderId">Идентификатор</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Заказ</returns>
    Task<Maybe<Order>> GetAsync(Guid orderId, CancellationToken cancellationToken);
    
    /// <summary>
    ///     Получить один любой новый заказ (в статусе "Created")
    /// </summary>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Заказ</returns>
    Task<Maybe<Order>> GetFirstByCreatedStatusAsync(CancellationToken cancellationToken);
    
    /// <summary>
    ///     Получить все назначенные заказы (в статусе "Assigned")
    /// </summary>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>Заказ</returns>
    Task<IReadOnlyCollection<Order>> GetAllByAssignedStatusAsync(CancellationToken cancellationToken);
}