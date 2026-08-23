// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;

namespace EricksonLopez.Result;

/// <summary>
/// Provides <see cref="IEqualityComparer{T}"/> implementations for <see cref="Error"/>.
/// </summary>
/// <remarks>
/// <para>
/// By default, <see cref="Error.Equals(Error?)"/> performs shallow equality based on
/// <see cref="Error.Code"/>, <see cref="Error.Description"/>, <see cref="Error.Type"/>,
/// <see cref="Error.Severity"/>, and <see cref="Error.Retryability"/>.
/// Use <see cref="Strict"/> when you need full structural equality including
/// <see cref="Error.TraceId"/>, <see cref="Error.CorrelationId"/>, <see cref="Error.DescriptionKey"/>,
/// <see cref="Error.InnerErrors"/>, and <see cref="Error.Metadata"/>.
/// </para>
/// <para>
/// <b>Usage with collections:</b>
/// <code>
/// var set = new HashSet&lt;Error&gt;(ErrorEqualityComparer.Strict);
/// var dict = new Dictionary&lt;Error, int&gt;(ErrorEqualityComparer.Strict);
/// </code>
/// </para>
/// </remarks>
public static class ErrorEqualityComparer
{
    /// <summary>
    /// Gets an <see cref="IEqualityComparer{T}"/> that uses <see cref="Error.Equals(Error?)"/>
    /// for shallow (Code, Description, Type, Severity, Retryability) equality.
    /// </summary>
    public static IEqualityComparer<Error> Default { get; } = new DefaultErrorComparer();

    /// <summary>
    /// Gets an <see cref="IEqualityComparer{T}"/> that uses <see cref="Error.StrictEquals"/>
    /// for deep structural equality including all fields (TraceId, CorrelationId,
    /// DescriptionKey, InnerErrors, Metadata) beyond those covered by shallow equality.
    /// </summary>
    public static IEqualityComparer<Error> Strict { get; } = new StrictErrorComparer();

    private sealed class DefaultErrorComparer : IEqualityComparer<Error>
    {
        public bool Equals(Error? x, Error? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;
            return x.Equals(y);
        }

        public int GetHashCode(Error obj) => obj.GetHashCode();
    }

    private sealed class StrictErrorComparer : IEqualityComparer<Error>
    {
        public bool Equals(Error? x, Error? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;
            return x.StrictEquals(y);
        }

        public int GetHashCode(Error obj)
        {
            // Include all fields in hash for strict equality. GetType() is omitted because
            // Error is sealed — the runtime type is always Error, so including GetType()
            // would add a constant that doesn't differentiate any two instances.
            var hashCode = new HashCode();
            hashCode.Add(obj.Code, System.StringComparer.Ordinal);
            hashCode.Add(obj.Description, System.StringComparer.Ordinal);
            hashCode.Add<ErrorType>(obj.Type);
            hashCode.Add<ErrorSeverity>(obj.Severity);
            hashCode.Add<ErrorRetryability>(obj.Retryability);
            hashCode.Add<string?>(obj.DescriptionKey, System.StringComparer.Ordinal);
            hashCode.Add<string?>(obj.TraceId, System.StringComparer.Ordinal);
            hashCode.Add<string?>(obj.CorrelationId, System.StringComparer.Ordinal);

            if (obj.HasInnerErrors)
            {
                // Hash count + each inner error's Code+Type so that errors with the same
                // number of inner errors but different content get distinct hashes.
                hashCode.Add<int>(obj.InnerErrors.Length);
                foreach (var inner in obj.InnerErrors)
                {
                    hashCode.Add(inner.Code, System.StringComparer.Ordinal);
                    hashCode.Add<ErrorType>(inner.Type);
                }
            }

            if (obj.HasMetadata)
            {
                // Hash count + each key so that errors with the same number of metadata
                // entries but different keys (or values) get distinct hashes.
                hashCode.Add<int>(obj.Metadata.Count);
                foreach (var kvp in obj.Metadata)
                {
                    hashCode.Add(kvp.Key, System.StringComparer.Ordinal);
                    // Value hash: use safe fallback that works for all object types.
                    int valHash = kvp.Value is null ? 0 : kvp.Value.GetHashCode();
                    hashCode.Add<int>(valHash);
                }
            }

            return hashCode.ToHashCode();
        }
    }
}


