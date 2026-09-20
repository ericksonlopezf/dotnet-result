// Copyright © Erickson Lopez. MIT License.
namespace EricksonLopez.Result.Dapr;

/// <summary>
/// Represents the payload returned to Dapr Pub/Sub endpoints when a message is dropped.
/// </summary>
/// <param name="Status">The disposition status for Dapr ("DROP").</param>
/// <param name="Code">The domain error code.</param>
/// <param name="Description">The error description.</param>
public sealed record DaprErrorResponse(string Status, string Code, string Description);
