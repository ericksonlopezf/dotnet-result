// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;

namespace EricksonLopez.Result.MassTransit;

/// <summary>
/// Represents a structured message fault contract published or returned across message transports
/// when a consumer operation yields a domain <see cref="Error"/>.
/// </summary>
public sealed class ResultFault
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResultFault"/> class.
    /// </summary>
    public ResultFault()
    {
        Code = string.Empty;
        Description = string.Empty;
        Type = string.Empty;
        Severity = string.Empty;
        Retryability = string.Empty;
        Metadata = new Dictionary<string, object>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResultFault"/> class from an <see cref="Error"/>.
    /// </summary>
    /// <param name="error">The domain error to map.</param>
    public ResultFault(Error error)
    {
        if (error is null) throw new ArgumentNullException(nameof(error));

        Code = error.Code;
        Description = error.Description;
        Type = error.Type.ToString();
        Severity = error.Severity.ToString();
        Retryability = error.Retryability.ToString();
        TraceId = error.TraceId;
        CorrelationId = error.CorrelationId;
        DescriptionKey = error.DescriptionKey;

        var meta = new Dictionary<string, object>(error.Metadata.Count);
        foreach (var kvp in error.Metadata)
        {
            meta[kvp.Key] = kvp.Value;
        }
        Metadata = meta;
    }

    /// <summary>
    /// Gets or sets the unique error code.
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// Gets or sets the error description.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the error type name.
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Gets or sets the severity classification name.
    /// </summary>
    public string Severity { get; set; }

    /// <summary>
    /// Gets or sets the retryability classification name.
    /// </summary>
    public string Retryability { get; set; }

    /// <summary>
    /// Gets or sets the ambient trace ID.
    /// </summary>
    public string? TraceId { get; set; }

    /// <summary>
    /// Gets or sets the correlation ID.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Gets or sets the localization key.
    /// </summary>
    public string? DescriptionKey { get; set; }

    /// <summary>
    /// Gets or sets the structured metadata dictionary.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata { get; set; }

    /// <summary>
    /// Creates a <see cref="ResultFault"/> instance from a domain <see cref="Error"/>.
    /// </summary>
    /// <param name="error">The source error.</param>
    /// <returns>A mapped fault message.</returns>
    public static ResultFault FromError(Error error) => new(error);
}
