// Copyright © Erickson Lopez. MIT License.
using System;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EricksonLopez.Result.EntityFrameworkCore.Tests;

public class EntityFrameworkResultTests
{
    private static TestDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new TestDbContext(options);
    }

    [Fact]
    public async Task SaveChangesAsyncToResult_Returns_Success_When_EntriesSaved()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        context.Items.Add(new TestItem { Id = 1, Name = "Item 1" });

        var result = await context.SaveChangesAsyncToResult();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(1);
    }

    [Fact]
    public async Task FirstOrDefaultToResultAsync_Returns_Success_When_Found()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        context.Items.Add(new TestItem { Id = 1, Name = "Target" });
        await context.SaveChangesAsync();

        var result = await context.Items.FirstOrDefaultToResultAsync(
            x => x.Id == 1,
            Error.NotFound("Item.NotFound", "Not found"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Target");
    }

    [Fact]
    public async Task FirstOrDefaultToResultAsync_Returns_Failure_When_NotFound()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());

        var result = await context.Items.FirstOrDefaultToResultAsync(
            x => x.Id == 999,
            Error.NotFound("Item.NotFound", "Item 999 not found"));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Item.NotFound");
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task SingleOrDefaultToResultAsync_Returns_Conflict_When_Multiple_Found()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        context.Items.Add(new TestItem { Id = 1, Name = "Duplicate" });
        context.Items.Add(new TestItem { Id = 2, Name = "Duplicate" });
        await context.SaveChangesAsync();

        var result = await context.Items.SingleOrDefaultToResultAsync(
            x => x.Name == "Duplicate",
            Error.NotFound("Item.NotFound", "Not found"));

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Conflict);
        result.Error.Code.Should().Be(EntityFrameworkErrorCodes.MultipleEntitiesFound);
    }

    [Fact]
    public async Task ToListToResultAsync_Returns_Success_List()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        context.Items.Add(new TestItem { Id = 1, Name = "A" });
        context.Items.Add(new TestItem { Id = 2, Name = "B" });
        await context.SaveChangesAsync();

        var result = await context.Items.ToListToResultAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Count.Should().Be(2);
    }

    [Fact]
    public async Task SingleOrDefaultToResultAsync_Returns_Success_When_Found()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());
        context.Items.Add(new TestItem { Id = 1, Name = "Unique" });
        await context.SaveChangesAsync();

        var result = await context.Items.SingleOrDefaultToResultAsync(
            x => x.Name == "Unique",
            Error.NotFound("Item.NotFound", "Not found"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Unique");
    }

    [Fact]
    public async Task SingleOrDefaultToResultAsync_Returns_NotFound_When_Missing()
    {
        using var context = CreateContext(Guid.NewGuid().ToString());

        var result = await context.Items.SingleOrDefaultToResultAsync(
            x => x.Name == "Missing",
            Error.NotFound("Item.NotFound", "Not found"));

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task ExtensionMethods_Validate_Null_Arguments()
    {
        DbContext nullContext = null!;
        await Assert.ThrowsAsync<ArgumentNullException>(() => nullContext.SaveChangesAsyncToResult());

        IQueryable<TestItem> nullQueryable = null!;
        await Assert.ThrowsAsync<ArgumentNullException>(() => nullQueryable.FirstOrDefaultToResultAsync(x => true, Error.Failure("A", "B")));
        await Assert.ThrowsAsync<ArgumentNullException>(() => nullQueryable.SingleOrDefaultToResultAsync(x => true, Error.Failure("A", "B")));
        await Assert.ThrowsAsync<ArgumentNullException>(() => nullQueryable.ToListToResultAsync());

        using var context = CreateContext(Guid.NewGuid().ToString());
        await Assert.ThrowsAsync<ArgumentNullException>(() => context.Items.FirstOrDefaultToResultAsync(null!, Error.Failure("A", "B")));
        await Assert.ThrowsAsync<ArgumentNullException>(() => context.Items.SingleOrDefaultToResultAsync(null!, Error.Failure("A", "B")));
    }

    [Fact]
    public async Task SaveChangesAsyncToResult_Maps_Exceptions_Correctly()
    {
        using var concurrencyCtx = new FaultyDbContext(new DbUpdateConcurrencyException("concurrency error"));
        var resConcurrency = await concurrencyCtx.SaveChangesAsyncToResult();
        resConcurrency.IsFailure.Should().BeTrue();
        resConcurrency.Error.Type.Should().Be(ErrorType.Conflict);
        resConcurrency.Error.Code.Should().Be(EntityFrameworkErrorCodes.ConcurrencyConflict);

        using var updateCtx = new FaultyDbContext(new DbUpdateException("update error"));
        var resUpdate = await updateCtx.SaveChangesAsyncToResult();
        resUpdate.IsFailure.Should().BeTrue();
        resUpdate.Error.Type.Should().Be(ErrorType.Failure);
        resUpdate.Error.Code.Should().Be(EntityFrameworkErrorCodes.UpdateFailed);

        using var timeoutCtx = new FaultyDbContext(new TimeoutException("timeout error"));
        var resTimeout = await timeoutCtx.SaveChangesAsyncToResult();
        resTimeout.IsFailure.Should().BeTrue();
        resTimeout.Error.Type.Should().Be(ErrorType.Unavailable);
        resTimeout.Error.Code.Should().Be(EntityFrameworkErrorCodes.Timeout);
        resTimeout.Error.Retryability.Should().Be(ErrorRetryability.Transient);

        using var genericCtx = new FaultyDbContext(new InvalidOperationException("generic failure"));
        var resGeneric = await genericCtx.SaveChangesAsyncToResult();
        resGeneric.IsFailure.Should().BeTrue();
        resGeneric.Error.Type.Should().Be(ErrorType.Unexpected);
        resGeneric.Error.Code.Should().Be(EntityFrameworkErrorCodes.Unexpected);

        using var canceledCtx = new FaultyDbContext(new OperationCanceledException());
        await Assert.ThrowsAsync<OperationCanceledException>(() => canceledCtx.SaveChangesAsyncToResult());
    }

    private class FaultyDbContext : DbContext
    {
        private readonly Exception _exceptionToThrow;

        public FaultyDbContext(Exception exceptionToThrow)
            : base(new DbContextOptionsBuilder<FaultyDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options)
        {
            _exceptionToThrow = exceptionToThrow;
        }

        public override System.Threading.Tasks.Task<int> SaveChangesAsync(System.Threading.CancellationToken cancellationToken = default)
        {
            return System.Threading.Tasks.Task.FromException<int>(_exceptionToThrow);
        }
    }
}
