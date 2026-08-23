// Copyright © Erickson Lopez. MIT License.
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using EricksonLopez.Result;
using EricksonLopez.Result.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using Xunit;

namespace EricksonLopez.Result.AspNetCore.Tests;

public class ResultHttpOptionsTests
{
    [Fact]
    public void ConfigureStatusCode_WhenBeforeFreeze_UpdatesMap()
    {
        var options = new ResultHttpOptions();
        options.ConfigureStatusCode(ErrorType.Domain, 418);

        options.StatusCodeMap[ErrorType.Domain].Should().Be(418);
    }

    [Fact]
    public void ConfigureStatusCode_WhenAfterFreeze_ThrowsInvalidOperationException()
    {
        var options = new ResultHttpOptions();

        var error = Error.Validation("C", "M");
        Result.Failure(error).ToHttpResult(options);

        Action ex_action = () => options.ConfigureStatusCode(ErrorType.Domain, 418);
        var ex = ex_action.Should().Throw<InvalidOperationException>().Which;
        ex.Message.Should().Contain("ResultHttpOptions cannot be modified after the first request has been processed.");
        ex.Message.Should().Contain("Call ConfigureStatusCode during application startup before any requests are handled.");
    }

    [Fact]
    public void ConfigureStatusCode_WhenAfterFreeze_Throws()
    {
        var options = new ResultHttpOptions();
        options.GetFrozenStatusCodeMap();
        Action ex_action = () => options.ConfigureStatusCode(ErrorType.Validation, 200);
        var ex = ex_action.Should().Throw<InvalidOperationException>().Which;
        ex.Message.Should().Contain("ResultHttpOptions cannot be modified after the first request has been processed.");
        ex.Message.Should().Contain("Call ConfigureStatusCode during application startup before any requests are handled.");
    }

    [Fact]
    public void ConfigureTitleOverride_WhenValidParameters_AddsTitle()
    {
        var options = new ResultHttpOptions();
        options.ConfigureTitleOverride(ErrorType.Validation, "Bad Request Title");
        var map = options.TitleOverrides;
        map[ErrorType.Validation].Should().Be("Bad Request Title");
    }

    [Fact]
    public void ConfigureTitleOverride_WhenAfterFreeze_Throws()
    {
        var options = new ResultHttpOptions();
        options.GetFrozenStatusCodeMap();
        Action ex_action = () => options.ConfigureTitleOverride(ErrorType.Validation, "Bad Request Title");
        var ex = ex_action.Should().Throw<InvalidOperationException>().Which;
        ex.Message.Should().Contain("ResultHttpOptions cannot be modified after the first request has been processed.");
        ex.Message.Should().Contain("Call ConfigureTitleOverride during application startup before any requests are handled.");
    }

    [Fact]
    public void IncludeDescriptionInDevelopment_WhenConfigured_SetsCorrectly()
    {
        var opt = new ResultHttpOptions();
        var mockEnv = NSubstitute.Substitute.For<Microsoft.Extensions.Hosting.IHostEnvironment>();

        mockEnv.EnvironmentName.Returns("Development");
        opt.IncludeDescriptionInDevelopment(mockEnv);
        opt.IncludeDescription.Should().BeTrue();

        mockEnv.EnvironmentName.Returns("Production");
        opt.IncludeDescriptionInDevelopment(mockEnv);
        opt.IncludeDescription.Should().BeFalse();
    }

    [Fact]
    public void GetTitleOverride_WhenPreFreeze_ReturnsFromMutableDict()
    {
        var opt = new ResultHttpOptions();
        opt.ConfigureTitleOverride(ErrorType.Failure, "Custom");

        var result = opt.GetTitleOverride(ErrorType.Failure);
        result.Should().Be("Custom");

        var resultNull = opt.GetTitleOverride(ErrorType.Conflict);
        resultNull.Should().BeNull();
    }

    [Fact]
    public void GetTitleOverride_WhenPostFreeze_ReturnsFromFrozenSnapshot()
    {
        var opt = new ResultHttpOptions();
        opt.ConfigureTitleOverride(ErrorType.Forbidden, "No Entry");
        opt.GetFrozenStatusCodeMap(); // trigger freeze

        opt.GetTitleOverride(ErrorType.Forbidden).Should().Be("No Entry");
        opt.GetTitleOverride(ErrorType.NotFound).Should().BeNull();
    }

    [Fact]
    public void GetTitleOverride_WhenPostFreeze_IsLockFree_DoesNotAcquireFreezeLock()
    {
        var options = new ResultHttpOptions();
        options.ConfigureTitleOverride(ErrorType.Forbidden, "No Entry");
        options.GetFrozenStatusCodeMap(); // trigger freeze

        var lockHeld = new ManualResetEventSlim(false);
        var releaseLock = new ManualResetEventSlim(false);
        var lockField = typeof(ResultHttpOptions).GetField("_freezeLock", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        var lockObj = lockField.GetValue(options)!;

        var thread = new Thread(() =>
        {
            lock (lockObj)
            {
                lockHeld.Set();
                releaseLock.Wait(5000);
            }
        });
        thread.Start();
        lockHeld.Wait(1000);

        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            // Must return immediately (< 500ms) from lock-free frozen snapshot without waiting for _freezeLock
            var title = options.GetTitleOverride(ErrorType.Forbidden);
            sw.Stop();
            sw.ElapsedMilliseconds.Should().BeLessThan(500);
            title.Should().Be("No Entry");
        }
        finally
        {
            releaseLock.Set();
            thread.Join(1000);
        }
    }

    [Fact]
    public void GetFrozenTitleOverrides_WhenCalled_ReturnsNullBeforeFreeze_AndSnapshotAfterFreeze()
    {
        var options = new ResultHttpOptions();
        options.GetFrozenTitleOverrides().Should().BeNull();

        options.ConfigureTitleOverride(ErrorType.Failure, "Custom Title");
        options.GetFrozenStatusCodeMap();

        var frozen = options.GetFrozenTitleOverrides();
        frozen.Should().NotBeNull();
        frozen![ErrorType.Failure].Should().Be("Custom Title");
    }

    [Fact]
    public void GetInternalFrozenStatusCodeMap_WhenCalled_ReturnsNullBeforeFreeze_AndSnapshotAfterFreeze()
    {
        var options = new ResultHttpOptions();
        options.GetInternalFrozenStatusCodeMap().Should().BeNull();

        options.GetFrozenStatusCodeMap();
        options.GetInternalFrozenStatusCodeMap().Should().NotBeNull();
    }

    [Fact]
    public void IsFrozen_WhenQueried_ReturnsInternalField()
    {
        var opt = new ResultHttpOptions();
        opt.IsFrozen.Should().BeFalse();

        opt.GetFrozenStatusCodeMap();
        opt.IsFrozen.Should().BeTrue();
    }

    [Fact]
    public void TitleOverrides_WhenQueried_ReturnsCachedWrapper_AndInvalidatesOnConfigure()
    {
        var options = new ResultHttpOptions();
        var map1 = options.TitleOverrides;
        var map2 = options.TitleOverrides;
        map2.Should().BeSameAs(map1);

        options.ConfigureTitleOverride(ErrorType.Failure, "Fail");
        var map3 = options.TitleOverrides;
        map3[ErrorType.Failure].Should().Be("Fail");
    }

    [Fact]
    public void StatusCodeMap_WhenConfigured_InvalidatesOnConfigure()
    {
        var options = new ResultHttpOptions();
        var map1 = options.StatusCodeMap;
        var map2 = options.StatusCodeMap;
        map2.Should().BeSameAs(map1);

        options.ConfigureStatusCode(ErrorType.Failure, 418);
        var map3 = options.StatusCodeMap;
        map3[ErrorType.Failure].Should().Be(418);
    }

    [Fact]
    public void ConfigureTitleOverride_WhenConcurrent_ReleasesLock()
    {
        var options = new ResultHttpOptions();
        options.ConfigureTitleOverride(ErrorType.Failure, "Custom");
        var t = new Thread(() => options.ConfigureTitleOverride(ErrorType.Conflict, "Conflict"));
        t.Start();
        bool completed = t.Join(1000);
        completed.Should().BeTrue();
    }

    [Fact]
    public void GetFrozenStatusCodeMap_WhenConcurrent_ReleasesLock()
    {
        var options = new ResultHttpOptions();
        options.GetFrozenStatusCodeMap();

        var exThrown = false;
        var t = new Thread(() =>
        {
            try { options.ConfigureStatusCode(ErrorType.Validation, 200); }
            catch (InvalidOperationException) { exThrown = true; }
        });
        t.Start();
        bool completed = t.Join(1000);
        completed.Should().BeTrue();
        exThrown.Should().BeTrue();
    }

    [Fact]
    public void GetTitleOverride_WhenConcurrent_ReleasesLock()
    {
        var options = new ResultHttpOptions();
        options.ConfigureTitleOverride(ErrorType.Failure, "Custom");
        options.GetTitleOverride(ErrorType.Failure);

        var t = new Thread(() => options.GetTitleOverride(ErrorType.Conflict));
        t.Start();
        bool completed = t.Join(1000);
        completed.Should().BeTrue();
    }

    [Fact]
    public async Task ConcurrentGetFrozenStatusCodeMapAndGetTitleOverride_WhenCalled_ThreadSafe()
    {
        var options = new ResultHttpOptions();
        options.ConfigureTitleOverride(ErrorType.Validation, "Custom Validation");

        var tasks = Enumerable.Range(0, 10).Select(_ => Task.Run(() =>
        {
            var frozenStatus = options.GetFrozenStatusCodeMap();
            var title = options.GetTitleOverride(ErrorType.Validation);
            frozenStatus.Should().NotBeNull();
            title.Should().Be("Custom Validation");
        })).ToArray();

        await Task.WhenAll(tasks);
    }

    [Fact]
    public void ToProblemDetails_WhenAllErrorTypes_CoversAllDescriptiveTitles()
    {
        var options = new ResultHttpOptions { TypeUriBase = "https://example.com/" };
        foreach (ErrorType type in Enum.GetValues<ErrorType>())
        {
            var result = Result.Failure(Error.Create("C", "D").WithType(type).Build());
            var pd = result.ToProblemDetails(options);
            ((ProblemHttpResult)pd).ProblemDetails.Title.Should().NotBeNullOrEmpty();
        }

        var options2 = new ResultHttpOptions { TypeUriBase = "https://example.com/" };
        options2.ConfigureStatusCode(ErrorType.Failure, 500);
        var pd2 = Result.Failure(Error.Failure("C", "D")).ToProblemDetails(options2);
        ((ProblemHttpResult)pd2).StatusCode.Should().Be(500);
    }

    [Fact]
    public void StatusCodeMap_WhenQueried_ReturnsCachedWrapper()
    {
        var options = new ResultHttpOptions();
        var map1 = options.StatusCodeMap;
        var map2 = options.StatusCodeMap;
        map2.Should().BeSameAs(map1);
    }

    [Fact]
    public void ConfigureStatusCode_WhenConcurrent_ReleasesLock()
    {
        var options = new ResultHttpOptions();
        options.ConfigureStatusCode(ErrorType.Failure, 400);
        var t = new Thread(() => options.ConfigureStatusCode(ErrorType.Conflict, 409));
        t.Start();
        bool completed = t.Join(1000);
        completed.Should().BeTrue();
    }

    [Fact]
    public void GetFrozenStatusCodeMap_WhenCalled_SetsIsFrozen()
    {
        var options = new ResultHttpOptions();
        options.GetFrozenStatusCodeMap();
        Action action = () => options.ConfigureStatusCode(ErrorType.Validation, 200);
        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void IncludeDescriptionInDevelopment_WhenNullEnvironment_ThrowsArgumentNullException()
    {
        var options = new ResultHttpOptions();
        Action action = () => options.IncludeDescriptionInDevelopment(null!);
        action.Should().Throw<ArgumentNullException>();
    }


    // Helper: triggers freeze by calling GetFrozenStatusCodeMap (internal method).
    private static ResultHttpOptions CreateFrozenOptions()
    {
        var options = new ResultHttpOptions();
        options.GetFrozenStatusCodeMap();
        return options;
    }

    // ─── Pre-freeze: all setters must accept values ────────────────────────────

    [Fact]
    public void DefaultSuccessStatusCode_WhenBeforeFreeze_Accepted()
    {
        var options = new ResultHttpOptions();
        options.DefaultSuccessStatusCode = StatusCodes.Status200OK;
        options.DefaultSuccessStatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public void IncludeTraceId_WhenBeforeFreeze_Accepted()
    {
        var options = new ResultHttpOptions();
        options.IncludeTraceId = true;
        options.IncludeTraceId.Should().BeTrue();
    }

    [Fact]
    public void IncludeDescription_WhenBeforeFreeze_Accepted()
    {
        var options = new ResultHttpOptions();
        options.IncludeDescription = true;
        options.IncludeDescription.Should().BeTrue();
    }

    [Fact]
    public void DefaultFallbackDescription_WhenBeforeFreeze_Accepted()
    {
        var options = new ResultHttpOptions();
        options.DefaultFallbackDescription = "Custom fallback";
        options.DefaultFallbackDescription.Should().Be("Custom fallback");
    }

    [Fact]
    public void TypeUriBase_WhenBeforeFreeze_Accepted()
    {
        var options = new ResultHttpOptions();
        options.TypeUriBase = "https://api.example.com/errors/";
        options.TypeUriBase.Should().Be("https://api.example.com/errors/");
    }

    // ─── Post-freeze: all setters must throw InvalidOperationException ─────────

    [Fact]
    public void DefaultSuccessStatusCode_WhenAfterFreeze_ThrowsInvalidOperationException()
    {
        // Arrange
        var options = CreateFrozenOptions();

        // Act & Assert
        // Regression: scalar properties were unprotected auto-properties (ARB F2-04-A).
        // Setting DefaultSuccessStatusCode after the first request would silently change
        // the success status code for ALL subsequent requests.
        Action ex_action = () => options.DefaultSuccessStatusCode = 200;
        var ex = ex_action.Should().Throw<InvalidOperationException>().Which;
        ex.Message.Should().Contain("DefaultSuccessStatusCode");
        ex.Message.Should().Contain("cannot be set after the first request");
        ex.Message.Should().Contain("Configure all options during application startup before any requests are handled.");
    }

    [Fact]
    public void IncludeTraceId_WhenAfterFreeze_ThrowsInvalidOperationException()
    {
        var options = CreateFrozenOptions();

        Action ex_action = () => options.IncludeTraceId = true;
        var ex = ex_action.Should().Throw<InvalidOperationException>().Which;
        ex.Message.Should().Contain("IncludeTraceId");
        ex.Message.Should().Contain("cannot be set after the first request");
        ex.Message.Should().Contain("Configure all options during application startup before any requests are handled.");
    }

    [Fact]
    public void IncludeDescription_WhenAfterFreeze_ThrowsInvalidOperationException()
    {
        var options = CreateFrozenOptions();

        // This is the highest-severity case: setting IncludeDescription = true after the first
        // request would expose error details to all subsequent API clients — an information
        // disclosure vulnerability (ARB finding F2-04-A / F6-02-A).
        Action ex_action = () => options.IncludeDescription = true;
        var ex = ex_action.Should().Throw<InvalidOperationException>().Which;
        ex.Message.Should().Contain("IncludeDescription");
        ex.Message.Should().Contain("cannot be set after the first request");
        ex.Message.Should().Contain("Configure all options during application startup before any requests are handled.");
    }

    [Fact]
    public void DefaultFallbackDescription_WhenAfterFreeze_ThrowsInvalidOperationException()
    {
        var options = CreateFrozenOptions();

        Action ex_action = () => options.DefaultFallbackDescription = "Hacked";
        var ex = ex_action.Should().Throw<InvalidOperationException>().Which;
        ex.Message.Should().Contain("DefaultFallbackDescription");
        ex.Message.Should().Contain("cannot be set after the first request");
        ex.Message.Should().Contain("Configure all options during application startup before any requests are handled.");
    }

    [Fact]
    public void TypeUriBase_WhenAfterFreeze_ThrowsInvalidOperationException()
    {
        var options = CreateFrozenOptions();

        Action ex_action = () => options.TypeUriBase = "https://evil.example.com/";
        var ex = ex_action.Should().Throw<InvalidOperationException>().Which;
        ex.Message.Should().Contain("TypeUriBase");
        ex.Message.Should().Contain("cannot be set after the first request");
        ex.Message.Should().Contain("Configure all options during application startup before any requests are handled.");
    }

    [Fact]
    public void IncludeDescriptionInDevelopment_WhenAfterFreeze_ThrowsInvalidOperationException()
    {
        var options = CreateFrozenOptions();
        var mockEnv = Substitute.For<IHostEnvironment>();
        mockEnv.EnvironmentName.Returns("Development");

        Action ex_action = () => options.IncludeDescriptionInDevelopment(mockEnv);
        var ex = ex_action.Should().Throw<InvalidOperationException>().Which;
        ex.Message.Should().Contain("IncludeDescriptionInDevelopment");
        ex.Message.Should().Contain("cannot be set after the first request");
        ex.Message.Should().Contain("Configure all options during application startup before any requests are handled.");
    }

    // ─── Post-freeze: getters must still work (read path is not frozen) ────────

    [Fact]
    public void AllScalarProperties_WhenAfterFreeze_GettersStillWork()
    {
        var options = new ResultHttpOptions
        {
            DefaultSuccessStatusCode = StatusCodes.Status201Created,
            IncludeTraceId = true,
            IncludeDescription = true,
            DefaultFallbackDescription = "Custom",
            TypeUriBase = "https://api.example.com/"
        };
        // Trigger freeze
        options.GetFrozenStatusCodeMap();

        // All getters must return the values set before freeze
        options.DefaultSuccessStatusCode.Should().Be(StatusCodes.Status201Created);
        options.IncludeTraceId.Should().BeTrue();
        options.IncludeDescription.Should().BeTrue();
        options.DefaultFallbackDescription.Should().Be("Custom");
        options.TypeUriBase.Should().Be("https://api.example.com/");
    }

    // ─── Default values ────────────────────────────────────────────────────────

    [Fact]
    public void DefaultValues_WhenQueried_AreCorrect()
    {
        var options = new ResultHttpOptions();

        options.DefaultSuccessStatusCode.Should().Be(StatusCodes.Status204NoContent);
        options.IncludeTraceId.Should().BeFalse();
        options.IncludeDescription.Should().BeFalse();
        options.DefaultFallbackDescription.Should().Be("An error occurred.");
        options.TypeUriBase.Should().Be("about:blank");
    }

    [Fact]
    public async Task GetFrozenStatusCodeMap_DoubleCheckLock_WhenAcquiredByOtherThread_Works()
    {
        var options = new ResultHttpOptions();
        var freezeLockField = typeof(ResultHttpOptions).GetField("_freezeLock", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var freezeLock = freezeLockField!.GetValue(options)!;

        // Thread 1 holds the lock and initializes the dictionary
        var t1 = Task.Run(() =>
        {
            Monitor.Enter(freezeLock);
            try
            {
                // Give Thread 2 time to reach the lock block
                Thread.Sleep(500);
                // GetFrozenStatusCodeMap calls Monitor.Enter. Since Monitor is reentrant, Thread 1 can do this.
                options.GetFrozenStatusCodeMap();
            }
            finally
            {
                Monitor.Exit(freezeLock);
            }
        });

        Thread.Sleep(50); // Ensure Thread 1 runs and holds lock before Thread 2 calls GetFrozenStatusCodeMap
        
        // Thread 2 will call GetFrozenStatusCodeMap().
        // Thread 2's first check (outer) is null, so it proceeds to Monitor.Enter.
        // But Thread 1 holds the lock! Thread 2 blocks.
        // Thread 1 sets _frozenStatusCodeMap and releases lock.
        // Thread 2 acquires lock, hits inner check: `if (_frozenStatusCodeMap != null) return _frozenStatusCodeMap;`
        // It should evaluate to true and return immediately.
        var map = options.GetFrozenStatusCodeMap();
        
        Assert.NotNull(map);
        await t1;
    }
}







