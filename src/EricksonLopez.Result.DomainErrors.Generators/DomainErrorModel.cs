// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Immutable;

namespace EricksonLopez.Result.DomainErrors.Generators;

/// <summary>
/// Represents the declaration of a single domain error item within a domain error schema file.
/// </summary>
public sealed class DomainErrorModel : IEquatable<DomainErrorModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DomainErrorModel"/> class.
    /// </summary>
    /// <param name="name">The method name of the error factory.</param>
    /// <param name="code">The unique error code identifier.</param>
    /// <param name="description">The default human-readable error description template.</param>
    /// <param name="type">The domain error type classification.</param>
    /// <param name="severity">The optional error severity level.</param>
    /// <param name="retryability">The optional error retryability classification.</param>
    /// <param name="descriptionKey">The optional localization key.</param>
    /// <param name="parameters">The parameter list for parameterization.</param>
    public DomainErrorModel(
        string name,
        string code,
        string description,
        string? type,
        string? severity,
        string? retryability,
        string? descriptionKey,
        ImmutableArray<DomainErrorParameterModel> parameters)
    {
        Name = name ?? string.Empty;
        Code = code ?? string.Empty;
        Description = description ?? string.Empty;
        Type = string.IsNullOrWhiteSpace(type) ? "Failure" : type!;
        Severity = severity;
        Retryability = retryability;
        DescriptionKey = descriptionKey;
        Parameters = parameters.IsDefault ? ImmutableArray<DomainErrorParameterModel>.Empty : parameters;
    }

    /// <summary>
    /// Gets the factory method name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the unique error code.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Gets the description format string.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the error type (e.g. Failure, NotFound, Validation, Conflict, Unauthorized, Forbidden, Unavailable, Unexpected).
    /// </summary>
    public string Type { get; }

    /// <summary>
    /// Gets the error severity level, if specified.
    /// </summary>
    public string? Severity { get; }

    /// <summary>
    /// Gets the retryability classification, if specified.
    /// </summary>
    public string? Retryability { get; }

    /// <summary>
    /// Gets the localization key, if specified.
    /// </summary>
    public string? DescriptionKey { get; }

    /// <summary>
    /// Gets the method parameters.
    /// </summary>
    public ImmutableArray<DomainErrorParameterModel> Parameters { get; }

    /// <inheritdoc/>
    public bool Equals(DomainErrorModel? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (!string.Equals(Name, other.Name, StringComparison.Ordinal) ||
            !string.Equals(Code, other.Code, StringComparison.Ordinal) ||
            !string.Equals(Description, other.Description, StringComparison.Ordinal) ||
            !string.Equals(Type, other.Type, StringComparison.Ordinal) ||
            !string.Equals(Severity, other.Severity, StringComparison.Ordinal) ||
            !string.Equals(Retryability, other.Retryability, StringComparison.Ordinal) ||
            !string.Equals(DescriptionKey, other.DescriptionKey, StringComparison.Ordinal))
        {
            return false;
        }

        if (Parameters.Length != other.Parameters.Length) return false;
        for (int i = 0; i < Parameters.Length; i++)
        {
            if (!Parameters[i].Equals(other.Parameters[i])) return false;
        }

        return true;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as DomainErrorModel);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = StringComparer.Ordinal.GetHashCode(Name);
            hash = (hash * 397) ^ StringComparer.Ordinal.GetHashCode(Code);
            hash = (hash * 397) ^ StringComparer.Ordinal.GetHashCode(Description);
            hash = (hash * 397) ^ StringComparer.Ordinal.GetHashCode(Type);
            if (Severity is not null) hash = (hash * 397) ^ StringComparer.Ordinal.GetHashCode(Severity);
            if (Retryability is not null) hash = (hash * 397) ^ StringComparer.Ordinal.GetHashCode(Retryability);
            if (DescriptionKey is not null) hash = (hash * 397) ^ StringComparer.Ordinal.GetHashCode(DescriptionKey);
            hash = (hash * 397) ^ Parameters.Length;
            return hash;
        }
    }
}
