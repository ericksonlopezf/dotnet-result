// Copyright © Erickson Lopez. MIT License.
using System;
using Microsoft.CodeAnalysis;

namespace EricksonLopez.Result.Serialization.Generators;

internal readonly struct ResultTypeInfo : IEquatable<ResultTypeInfo>
{
    public readonly string FullyQualifiedName;
    public readonly string TypeInfoPropertyName;
    // Location of the [JsonSerializable] attribute for diagnostic reporting.
    // Only populated for non-generic Result markers (sentinel entries).
    public readonly Location? DiagnosticLocation;

    public ResultTypeInfo(string fullyQualifiedName, string typeInfoPropertyName, Location? diagnosticLocation = null)
    {
        FullyQualifiedName = fullyQualifiedName;
        TypeInfoPropertyName = typeInfoPropertyName;
        DiagnosticLocation = diagnosticLocation;
    }

    public bool Equals(ResultTypeInfo other)
        => FullyQualifiedName == other.FullyQualifiedName
        && TypeInfoPropertyName == other.TypeInfoPropertyName;

    public override bool Equals(object? obj) => obj is ResultTypeInfo other && Equals(other);
    public override int GetHashCode() => (FullyQualifiedName, TypeInfoPropertyName).GetHashCode();

    public static bool operator ==(ResultTypeInfo left, ResultTypeInfo right) => left.Equals(right);
    public static bool operator !=(ResultTypeInfo left, ResultTypeInfo right) => !left.Equals(right);
}
