// Copyright © Erickson Lopez. MIT License.
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace EricksonLopez.Result.DomainErrors.Generators.Tests;

internal sealed class TestAdditionalText : AdditionalText
{
    private readonly SourceText _text;

    public TestAdditionalText(string path, string text)
    {
        Path = path;
        _text = SourceText.From(text);
    }

    public override string Path { get; }

    public override SourceText GetText(CancellationToken cancellationToken = default) => _text;
}
