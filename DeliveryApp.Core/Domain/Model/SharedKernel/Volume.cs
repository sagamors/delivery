using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;
using Errs;
using Errs.Extensions;

namespace DeliveryApp.Core.Domain.Model.SharedKernel;

public sealed class Volume : ValueObject, IComparable<Volume>
{
    /// <summary>
    ///     Объём
    /// </summary>
    public int Value { get; }

    /// <summary>
    ///     Создаёт объект
    /// </summary>
    /// <param name="value">Объём</param>
    public static Result<Volume, Error> Create(int value)
    {
        if (value <= 0)
            return GeneralErrors.ValueMustBeGreaterThan(nameof(value), value, 0);

        return new Volume(value);
    }

    public static Volume operator +(Volume left, Volume right) => new(left.Value + right.Value);

    public int CompareTo(Volume other)
    {
        return Value.CompareTo(other.Value);
    }

    public static bool operator >(Volume left, Volume right) => left.CompareTo(right) > 0;
    public static bool operator <(Volume left, Volume right) => left.CompareTo(right) < 0;
    public static bool operator >=(Volume left, Volume right) => left.CompareTo(right) >= 0;
    public static bool operator <=(Volume left, Volume right) => left.CompareTo(right) <= 0;

    /// <summary>
    ///     Factory Method. Создаёт объект в контексте, где нарушение инвариантов невозможно и является исключительной ситуацией
    /// </summary>
    /// <param name="value">Объём</param>
    public static Volume MustCreate(int value)
    {
        return Create(value).GetValueOrThrow();
    }

    [ExcludeFromCodeCoverage]
    private Volume(int value)
    {
        Value = value;
    }

    [ExcludeFromCodeCoverage]
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
