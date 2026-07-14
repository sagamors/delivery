using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;
using Errs;
using Errs.Extensions;

namespace DeliveryApp.Core.Domain.Model.SharedKernel;

public sealed class Volume : ValueObject
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
