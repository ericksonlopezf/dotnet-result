// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Immutable;

namespace EricksonLopez.Result.DomainErrors.Generators;

/// <summary>
/// Represents the parsed declaration of a domain error class containing one or more domain errors.
/// </summary>
public sealed class DomainErrorClassModel : IEquatable<DomainErrorClassModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DomainErrorClassModel"/> class.
    /// </summary>
    /// <param name="namespaceName">The containing C# namespace.</param>
    /// <param name="className">The static class name to generate.</param>
    /// <param name="errors">The collection of domain errors.</param>
    public DomainErrorClassModel(
        string namespaceName,
        string className,
        ImmutableArray<DomainErrorModel> errors)
    {
        NamespaceName = namespaceName ?? "Domain.Errors";
        ClassName = className ?? "DomainErrors";
        Errors = errors.IsDefault ? ImmutableArray<DomainErrorModel>.Empty : errors;
    }

    /// <summary>
    /// Gets the namespace name.
    /// </summary>
    public string NamespaceName { get; }

    /// <summary>
    /// Gets the class name.
    /// </summary>
    public string ClassName { get; }

    /// <summary>
    /// Gets the domain error declarations.
    /// </summary>
    public ImmutableArray<DomainErrorModel> Errors { get; }

    /// <inheritdoc/>
    public bool Equals(DomainErrorClassModel? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (!string.Equals(NamespaceName, other.NamespaceName, StringComparison.Ordinal) ||
            !string.Equals(ClassName, other.ClassName, StringComparison.Ordinal))
        {
            return false;
        }

        if (Errors.Length != other.Errors.Length) return false;
        for (int i = 0; i < Errors.Length; i++)
        {
            if (!Errors[i].Equals(other.Errors[i])) return false;
        }

        return true;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as DomainErrorClassModel);

    /// <inheritdoc/>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    // Stryker disable all : Hash code contract
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = StringComparer.Ordinal.GetHashCode(NamespaceName);
            hash = (hash * 397) ^ StringComparer.Ordinal.GetHashCode(ClassName);
            hash = (hash * 397) ^ Errors.Length;
            return hash;
        }
    }
    // Stryker restore all
}
