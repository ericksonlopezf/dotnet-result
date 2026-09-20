// Copyright © Erickson Lopez. MIT License.
using System;

namespace EricksonLopez.Result.DomainErrors.Generators;

/// <summary>
/// Represents a strongly-typed parameter declaration for a generated domain error factory method.
/// </summary>
public sealed class DomainErrorParameterModel : IEquatable<DomainErrorParameterModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DomainErrorParameterModel"/> class.
    /// </summary>
    /// <param name="name">The name of the parameter.</param>
    /// <param name="type">The type of the parameter (e.g. <c>string</c>, <c>System.Guid</c>).</param>
    public DomainErrorParameterModel(string name, string type)
    {
        Name = name ?? string.Empty;
        Type = type ?? "string";
    }

    /// <summary>
    /// Gets the parameter name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the parameter type name.
    /// </summary>
    public string Type { get; }

    /// <inheritdoc/>
    public bool Equals(DomainErrorParameterModel? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return string.Equals(Name, other.Name, StringComparison.Ordinal) &&
               string.Equals(Type, other.Type, StringComparison.Ordinal);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as DomainErrorParameterModel);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        unchecked
        {
            return (StringComparer.Ordinal.GetHashCode(Name) * 397) ^ StringComparer.Ordinal.GetHashCode(Type);
        }
    }
}
