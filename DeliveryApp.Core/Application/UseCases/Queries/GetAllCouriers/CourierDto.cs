namespace DeliveryApp.Core.Application.UseCases.Queries.GetAllCouriers;

public class CourierDto
{
    /// <summary>
    ///     Идентификатор
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     Имя
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     Геопозиция (X,Y)
    /// </summary>
    public LocationDto Location { get; set; }
}