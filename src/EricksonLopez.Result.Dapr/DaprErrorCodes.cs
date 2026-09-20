// Copyright © Erickson Lopez. MIT License.
namespace EricksonLopez.Result.Dapr;

/// <summary>
/// Provides standardized domain error codes for Dapr operations.
/// </summary>
public static class DaprErrorCodes
{
    /// <summary>
    /// Error code emitted when a requested state entry does not exist in the state store.
    /// </summary>
    public const string StateNotFound = "Dapr.StateNotFound";

    /// <summary>
    /// Error code emitted when saving or deleting state fails due to an ETag concurrency mismatch.
    /// </summary>
    public const string EtagMismatch = "Dapr.EtagMismatch";

    /// <summary>
    /// Error code emitted when communication with the Dapr sidecar fails or is unavailable.
    /// </summary>
    public const string SidecarUnavailable = "Dapr.SidecarUnavailable";

    /// <summary>
    /// Error code emitted when a generic Dapr operation fails.
    /// </summary>
    public const string OperationFailed = "Dapr.OperationFailed";
}
