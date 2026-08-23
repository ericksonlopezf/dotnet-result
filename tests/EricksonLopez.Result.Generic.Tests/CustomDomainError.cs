// Copyright © Erickson Lopez. MIT License.

namespace EricksonLopez.Result.Generic.Tests;

public sealed record CustomDomainError(string Reason, int ErrorCode)
{
    public override string ToString() => $"[DomainError:{ErrorCode}] {Reason}";
}
