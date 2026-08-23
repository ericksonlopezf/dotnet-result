// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Immutable;

namespace EricksonLopez.Result.Serialization.Generators;

/// <summary>
/// Represents a JsonSerializerContext class that has Result&lt;T&gt; types registered via
/// [JsonSerializable] attributes. Implemented as a readonly record struct so that incremental
/// generator equality checks work correctly — a mutable struct with List&lt;T&gt; would risk
/// hash instability if the list were modified after use as a dictionary key.
/// </summary>
internal readonly record struct ContextInfo(
    string ClassName,
    string FullyQualifiedClassName,
    string? Namespace,
    ImmutableArray<ResultTypeInfo> ResultValueTypes) : IEquatable<ContextInfo>
{
    public bool Equals(ContextInfo other)
    {
        if (ClassName != other.ClassName || Namespace != other.Namespace)
            return false;
        if (ResultValueTypes.Length != other.ResultValueTypes.Length)
            return false;
        for (int i = 0; i < ResultValueTypes.Length; i++)
        {
            if (ResultValueTypes[i] != other.ResultValueTypes[i])
                return false;
        }
        return true;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17;
            hash = hash * 31 + ClassName.GetHashCode();
            hash = hash * 31 + (Namespace?.GetHashCode() ?? 0);
            foreach (var t in ResultValueTypes)
                hash = hash * 31 + t.FullyQualifiedName.GetHashCode();
            return hash;
        }
    }
}
