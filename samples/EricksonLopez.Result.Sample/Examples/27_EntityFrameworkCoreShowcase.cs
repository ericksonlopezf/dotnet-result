// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EricksonLopez.Result;
using EricksonLopez.Result.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EricksonLopez.Result.Sample.Examples;

public class ShowcaseUser
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class ShowcaseDbContext : DbContext
{
    public ShowcaseDbContext(DbContextOptions<ShowcaseDbContext> options) : base(options) { }

    public DbSet<ShowcaseUser> Users => Set<ShowcaseUser>();
}

public static class EntityFrameworkCoreShowcase
{
    public static async Task RunAsync()
    {
        Console.WriteLine("\n========================================================");
        Console.WriteLine(" 27. ENTITY FRAMEWORK CORE INTEGRATION SHOWCASE");
        Console.WriteLine("========================================================");

        var options = new DbContextOptionsBuilder<ShowcaseDbContext>()
            .UseInMemoryDatabase(databaseName: $"ShowcaseDb_{Guid.NewGuid()}")
            .Options;

        using var context = new ShowcaseDbContext(options);

        // Seed data
        context.Users.AddRange(
            new ShowcaseUser { Id = 1, Username = "alice", Role = "Admin" },
            new ShowcaseUser { Id = 2, Username = "bob", Role = "User" },
            new ShowcaseUser { Id = 3, Username = "charlie", Role = "User" }
        );

        // 1. SaveChangesAsyncToResult
        Console.WriteLine("\n[1] SaveChangesAsyncToResult — exception-safe commit:");
        Result<int> saveResult = await context.SaveChangesAsyncToResult();
        Console.WriteLine($"  Save succeeded: IsSuccess={saveResult.IsSuccess}, RecordsWritten={saveResult.Value}");

        // 2. FirstOrDefaultToResultAsync
        Console.WriteLine("\n[2] FirstOrDefaultToResultAsync — query with domain error fallback:");
        Result<ShowcaseUser> userFound = await context.Users
            .FirstOrDefaultToResultAsync(
                u => u.Username == "alice",
                notFoundError: Error.NotFound("User.NotFound", "User 'alice' does not exist."));

        Console.WriteLine($"  Found user: IsSuccess={userFound.IsSuccess}, Username={userFound.Value.Username}, Role={userFound.Value.Role}");

        Result<ShowcaseUser> userNotFound = await context.Users
            .FirstOrDefaultToResultAsync(
                u => u.Username == "david",
                notFoundError: Error.NotFound("User.NotFound", "User 'david' does not exist."));

        Console.WriteLine($"  Missing user: IsFailure={userNotFound.IsFailure}, Code=[{userNotFound.Error.Code}], Type={userNotFound.Error.Type}");

        // 3. SingleOrDefaultToResultAsync
        Console.WriteLine("\n[3] SingleOrDefaultToResultAsync — single entity verification:");
        Result<ShowcaseUser> singleUser = await context.Users
            .SingleOrDefaultToResultAsync(
                u => u.Id == 2,
                notFoundError: Error.NotFound("User.NotFound", "User #2 not found."));
        Console.WriteLine($"  Single user: IsSuccess={singleUser.IsSuccess}, Name={singleUser.Value.Username}");

        // SingleOrDefault detecting multiple elements translates to Conflict
        Result<ShowcaseUser> multipleConflict = await context.Users
            .SingleOrDefaultToResultAsync(
                u => u.Role == "User",
                notFoundError: Error.NotFound("User.NotFound", "No users found."));
        Console.WriteLine($"  Multiple entities conflict: IsFailure={multipleConflict.IsFailure}, Code=[{multipleConflict.Error.Code}], Type={multipleConflict.Error.Type}");

        // 4. ToListToResultAsync
        Console.WriteLine("\n[4] ToListToResultAsync — materialize query as Result<List<T>>:");
        Result<List<ShowcaseUser>> allAdmins = await context.Users
            .Where(u => u.Role == "Admin")
            .ToListToResultAsync();

        Console.WriteLine($"  Admins list count: {allAdmins.Value.Count}, First Admin={allAdmins.Value[0].Username}");

        // 5. EntityFrameworkErrorCodes reference
        Console.WriteLine("\n[5] Well-known EntityFrameworkErrorCodes constants:");
        Console.WriteLine($"  ConcurrencyConflict: {EntityFrameworkErrorCodes.ConcurrencyConflict}");
        Console.WriteLine($"  UpdateFailed:        {EntityFrameworkErrorCodes.UpdateFailed}");
        Console.WriteLine($"  Timeout:             {EntityFrameworkErrorCodes.Timeout}");
        Console.WriteLine($"  MultipleEntities:    {EntityFrameworkErrorCodes.MultipleEntitiesFound}");
    }
}
