using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;

namespace DeliveryApp.Core.Domain.Model.CourierAggregate;

/// <summary>
///     Статус назначения
/// </summary>
public sealed class AssignmentStatus : ValueObject
{
    public static AssignmentStatus Assigned => new(nameof(Assigned).ToLowerInvariant());
    public static AssignmentStatus Completed => new(nameof(Completed).ToLowerInvariant());

    /// <summary>
    ///     Ctr
    /// </summary>
    [ExcludeFromCodeCoverage]
    private AssignmentStatus()
    {
    }

    /// <summary>
    ///     Ctr
    /// </summary>
    /// <param name="name">Название</param>
    private AssignmentStatus(string name) : this()
    {
        Name = name;
    }

    /// <summary>
    ///     Название
    /// </summary>
    public string Name { get; private set;}

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name;
    }
}