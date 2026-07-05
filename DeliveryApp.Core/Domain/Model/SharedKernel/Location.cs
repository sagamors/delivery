using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;
using Errs;
using Errs.Extensions;

namespace DeliveryApp.Core.Domain.Model.SharedKernel;

public sealed class Location : ValueObject
{
    /// <summary>
    /// Минимальная координата
    /// </summary>
    public static readonly Location Min = new Location(1,1);
    
    /// <summary>
    /// Максимальная координата
    /// </summary>
    public static readonly Location Max = new Location(10,10);

    /// <summary>
    /// Координата по горизонтали
    /// </summary>
    public int X { get; }

    /// <summary>
    /// Координата по вертикали
    /// </summary>
    public int Y { get; }

    /// <summary>
    /// Создает объект.
    /// </summary>
    /// <param name="x">Координата по X</param>
    /// <param name="y">Координата по Y</param>
    /// <returns></returns>
    public static Result<Location, Error> Create(int x, int y)
    {
        if (x < Min.X || x > Max.X)
            return GeneralErrors.ValueMustBeBetween(nameof(x), x, Min.X, Max.X);
        
        if (y < Min.Y || y > Max.Y)
            return GeneralErrors.ValueMustBeBetween(nameof(y), y, Min.Y, Max.Y);
        
        return new Location(x, y);
    }
    
    /// <summary>
    /// Factory Method. Создаёт объект в контексте, где нарушение инвариантов невозможно и является исключительной ситуацией
    /// </summary>
    /// <param name="x">Координата по X</param>
    /// <param name="y">Координата по Y</param>
    /// <returns>Вес</returns>
    public static Location MustCreate(int x, int y)
    {
        return Create(x, y).GetValueOrThrow();
    }

    public int CalculateDistance(Location location)
    {
        ArgumentNullException.ThrowIfNull(location);
        if (this == location) return 0;

        var x = Math.Abs(X - location.X);
        var y = Math.Abs(Y - location.Y);

        return x + y;
    }
    
    [ExcludeFromCodeCoverage]
    private Location(int x, int y)
    {
        X = x;
        Y = y;
    }
    
    [ExcludeFromCodeCoverage]
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return X;
        yield return Y;
    }
}