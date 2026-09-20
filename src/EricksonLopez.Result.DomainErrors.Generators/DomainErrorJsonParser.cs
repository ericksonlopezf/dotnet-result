// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace EricksonLopez.Result.DomainErrors.Generators;

/// <summary>
/// Provides zero-dependency JSON parsing for domain error declarations.
/// </summary>
internal static class DomainErrorJsonParser
{
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Parses a domain error JSON document into a structured <see cref="DomainErrorClassModel"/>.
    /// </summary>
    /// <param name="json">The JSON text content.</param>
    /// <returns>A populated model, or <c>null</c> if parsing fails.</returns>
    public static DomainErrorClassModel? TryParse(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            string ns = ExtractTopLevelString(json, "namespace") ?? "Domain.Errors";
            string className = ExtractTopLevelString(json, "className") ?? "DomainErrors";

            int errorsIndex = json.IndexOf("\"errors\"", StringComparison.OrdinalIgnoreCase);
            if (errorsIndex < 0)
            {
                return new DomainErrorClassModel(ns, className, ImmutableArray<DomainErrorModel>.Empty);
            }

            int arrayStart = json.IndexOf('[', errorsIndex);
            int arrayEnd = json.LastIndexOf(']');
            if (arrayStart < 0 || arrayEnd <= arrayStart)
            {
                return new DomainErrorClassModel(ns, className, ImmutableArray<DomainErrorModel>.Empty);
            }

            string errorsArrayContent = json.Substring(arrayStart + 1, arrayEnd - arrayStart - 1);
            var errorBlocks = SplitJsonObjects(errorsArrayContent);
            var errors = ImmutableArray.CreateBuilder<DomainErrorModel>(errorBlocks.Count);

            foreach (var block in errorBlocks)
            {
                var error = ParseErrorBlock(block);
                if (error is not null)
                {
                    errors.Add(error);
                }
            }

            return new DomainErrorClassModel(ns, className, errors.ToImmutable());
        }
        catch
        {
            return null;
        }
    }

    private static string? ExtractTopLevelString(string json, string propertyName)
    {
        var match = Regex.Match(json, $@"""{propertyName}""\s*:\s*""([^""]*)""", RegexOptions.IgnoreCase, RegexTimeout);
        return match.Success ? match.Groups[1].Value : null;
    }

    private static List<string> SplitJsonObjects(string arrayContent)
    {
        var results = new List<string>();
        int depth = 0;
        int startIndex = -1;

        for (int i = 0; i < arrayContent.Length; i++)
        {
            char c = arrayContent[i];
            if (c == '{')
            {
                if (depth == 0)
                {
                    startIndex = i;
                }
                depth++;
            }
            else if (c == '}')
            {
                depth--;
                if (depth == 0 && startIndex >= 0)
                {
                    results.Add(arrayContent.Substring(startIndex, i - startIndex + 1));
                    startIndex = -1;
                }
            }
        }

        return results;
    }

    private static DomainErrorModel? ParseErrorBlock(string block)
    {
        string? name = ExtractTopLevelString(block, "name");
        string? code = ExtractTopLevelString(block, "code");
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        string description = ExtractTopLevelString(block, "description") ?? string.Empty;
        string? type = ExtractTopLevelString(block, "type");
        string? severity = ExtractTopLevelString(block, "severity");
        string? retryability = ExtractTopLevelString(block, "retryability");
        string? descriptionKey = ExtractTopLevelString(block, "descriptionKey");
        var parameters = ParseParameters(block);

        return new DomainErrorModel(name!, code!, description, type, severity, retryability, descriptionKey, parameters);
    }

    private static ImmutableArray<DomainErrorParameterModel> ParseParameters(string block)
    {
        int paramIndex = block.IndexOf("\"parameters\"", StringComparison.OrdinalIgnoreCase);
        if (paramIndex < 0)
        {
            return ImmutableArray<DomainErrorParameterModel>.Empty;
        }

        int pArrayStart = block.IndexOf('[', paramIndex);
        if (pArrayStart < 0)
        {
            return ImmutableArray<DomainErrorParameterModel>.Empty;
        }

        int pArrayEnd = block.IndexOf(']', pArrayStart);
        if (pArrayEnd <= pArrayStart)
        {
            return ImmutableArray<DomainErrorParameterModel>.Empty;
        }

        string pContent = block.Substring(pArrayStart + 1, pArrayEnd - pArrayStart - 1);
        var pBlocks = SplitJsonObjects(pContent);
        var parameters = ImmutableArray.CreateBuilder<DomainErrorParameterModel>(pBlocks.Count);

        foreach (var pb in pBlocks)
        {
            string? pName = ExtractTopLevelString(pb, "name");
            string? pType = ExtractTopLevelString(pb, "type");
            if (!string.IsNullOrWhiteSpace(pName))
            {
                parameters.Add(new DomainErrorParameterModel(pName!, pType ?? "string"));
            }
        }

        return parameters.ToImmutable();
    }
}
