// Copyright © Erickson Lopez. MIT License.

namespace EricksonLopez.Result.EntityFrameworkCore;

/// <summary>
/// Defines canonical, well-known domain error codes for Entity Framework Core persistence operations.
/// </summary>
public static class EntityFrameworkErrorCodes
{
    /// <summary>
    /// Error code emitted when an optimistic concurrency conflict occurs during database updates.
    /// </summary>
    public const string ConcurrencyConflict = "Database.ConcurrencyConflict";

    /// <summary>
    /// Error code emitted when a generic database update constraint or execution failure occurs.
    /// </summary>
    public const string UpdateFailed = "Database.UpdateFailed";

    /// <summary>
    /// Error code emitted when an expected entity does not exist in the database.
    /// </summary>
    public const string EntityNotFound = "Database.EntityNotFound";

    /// <summary>
    /// Error code emitted when a single entity was expected but multiple matching records were found.
    /// </summary>
    public const string MultipleEntitiesFound = "Database.MultipleEntitiesFound";

    /// <summary>
    /// Error code emitted when a database operation times out.
    /// </summary>
    public const string Timeout = "Database.Timeout";

    /// <summary>
    /// Error code emitted when an unexpected database exception occurs.
    /// </summary>
    public const string Unexpected = "Database.Unexpected";
}
