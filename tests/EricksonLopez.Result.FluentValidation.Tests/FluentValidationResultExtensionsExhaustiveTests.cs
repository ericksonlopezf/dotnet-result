// Copyright © Erickson Lopez. MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EricksonLopez.Result;
using EricksonLopez.Result.FluentValidation;
using EricksonLopez.Result.Testing;
using FluentValidation;
using Xunit;

namespace EricksonLopez.Result.FluentValidation.Tests;

public class FluentValidationResultExtensionsExhaustiveTests
{
    private class TestModel
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    private class TestModelValidator : AbstractValidator<TestModel>
    {
        public TestModelValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithErrorCode("NameRequired").WithMessage("Name must not be empty.")
                .WithSeverity(Severity.Error);

            RuleFor(x => x.Age)
                .GreaterThanOrEqualTo(18).WithErrorCode("NotAnAdult").WithMessage("Must be an adult.")
                .WithSeverity(Severity.Warning);

            RuleFor(x => x.Age)
                .LessThan(100).WithErrorCode("TooOld").WithMessage("Too old")
                .WithSeverity(Severity.Info);

            RuleFor(x => x.Name)
                .Must(n => n != "UnknownSeverity").WithMessage("No explicit error code")
                .WithSeverity((Severity)99);
        }
    }

    private readonly TestModelValidator _validator = new();
    private readonly TestModel _valid = new() { Name = "Erickson", Age = 25 };
    private readonly TestModel _invalidAll = new() { Name = "", Age = 15 };
    private readonly TestModel _invalidInfo = new() { Name = "Erickson", Age = 100 };
    private readonly TestModel _invalidUnknownSeverity = new() { Name = "UnknownSeverity", Age = 25 };

    [Fact]
    public void Validate_Behavior_Valid()
    {
        var r1 = FluentValidationResultExtensions.ValidateToResult(_validator, _valid);
        r1.ShouldBeSuccess();

        var r2 = FluentValidationResultExtensions.ValidateToResultWithValue(_validator, _valid);
        r2.ShouldBeSuccess();
        Assert.Equal(_valid, r2.Value);
    }

    [Fact]
    public async Task ValidateAsync_Behavior_Valid()
    {
        var r1 = await FluentValidationResultExtensions.ValidateToResultAsync(_validator, _valid, CancellationToken.None);
        r1.ShouldBeSuccess();

        var r2 = await FluentValidationResultExtensions.ValidateToResultWithValueAsync(_validator, _valid, CancellationToken.None);
        r2.ShouldBeSuccess();
        Assert.Equal(_valid, r2.Value);
    }

    [Fact]
    public void Validate_Behavior_Invalid_Structure()
    {
        var r1 = FluentValidationResultExtensions.ValidateToResult(_validator, _invalidAll);
        r1.ShouldBeFailure();

        var error = r1.Error;
        Assert.Equal(ErrorType.Validation, error.Type);
        Assert.Equal("Validation.Failed", error.Code);
        Assert.False(error.InnerErrors.IsEmpty);
        Assert.Equal(2, error.InnerErrors.Length);

        var e1 = error.InnerErrors[0];
        Assert.Equal("NameRequired", e1.Code);
        Assert.Equal("Name must not be empty.", e1.Description);
        Assert.Equal(ErrorSeverity.Error, e1.Severity);
        Assert.Equal("Name", e1.Metadata!["propertyName"]);
        Assert.Equal("", e1.Metadata!["attemptedValue"]);

        var e2 = error.InnerErrors[1];
        Assert.Equal("NotAnAdult", e2.Code);
        Assert.Equal(ErrorSeverity.Warning, e2.Severity);
        Assert.Equal("Age", e2.Metadata!["propertyName"]);
        Assert.Equal(15, e2.Metadata!["attemptedValue"]);
    }

    [Fact]
    public void Validate_Behavior_Invalid_Severity_Info()
    {
        var r1 = FluentValidationResultExtensions.ValidateToResult(_validator, _invalidInfo);
        r1.ShouldBeFailure();
        Assert.Single(r1.Error.InnerErrors!);

        var e1 = r1.Error.InnerErrors![0];
        Assert.Equal("TooOld", e1.Code);
        Assert.Equal(ErrorSeverity.Info, e1.Severity);
    }

    [Fact]
    public void Validate_Behavior_Invalid_UnknownSeverity_And_EmptyErrorCode()
    {
        var r1 = FluentValidationResultExtensions.ValidateToResult(_validator, _invalidUnknownSeverity);
        r1.ShouldBeFailure();
        var e1 = r1.Error.InnerErrors![0];
        Assert.Equal("PredicateValidator", e1.Code); // Auto-generated code from PropertyName/Validator
        Assert.Equal(ErrorSeverity.Error, e1.Severity); // Fallback severity for (Severity)99
    }

    [Fact]
    public void Validate_Behavior_Manual_NullErrorCode_And_Placeholders()
    {
        var failure = new global::FluentValidation.Results.ValidationFailure("TestProp", "Test message")
        {
            ErrorCode = null,
            Severity = Severity.Error,
            FormattedMessagePlaceholderValues = new Dictionary<string, object>
            {
                { "TestKey", "TestValue" },
                { "NullKey", null! }
            }
        };
        var emptyPlaceholders = new global::FluentValidation.Results.ValidationFailure("TestProp2", "Test message")
        {
            ErrorCode = null,
            Severity = Severity.Error,
            FormattedMessagePlaceholderValues = new Dictionary<string, object>()
        };
        var validationResult = new global::FluentValidation.Results.ValidationResult(new[] { failure, emptyPlaceholders });

        var r1 = validationResult.ToValidationResult<string>("test");
        r1.ShouldBeFailure();
        var e1 = r1.Error.InnerErrors![0];
        Assert.Equal("Validation.TestProp", e1.Code);
        Assert.Equal("TestValue", e1.Metadata!["placeholder.TestKey"]);
        Assert.False(e1.Metadata.ContainsKey("placeholder.NullKey"));

        var e2 = r1.Error.InnerErrors![1];
        Assert.Equal("Validation.TestProp2", e2.Code);
        Assert.Single(e2.Metadata!);
        Assert.Equal("TestProp2", e2.Metadata!["propertyName"]);
    }

    [Fact]
    public async Task EnsureValidAsync_Behavior()
    {
        var successT = Task.FromResult(Result.Success(_valid));
        var failureT = Task.FromResult(Result.Failure<TestModel>(Error.Failure("A", "B")));

        // Success -> Valid => Success
        var r1 = await successT.EnsureValidAsync(_validator);
        r1.ShouldBeSuccess();
        Assert.Same(_valid, r1.Value);

        // Success -> Invalid => Failure
        var successInvalidT = Task.FromResult(Result.Success(_invalidAll));
        var r2 = await successInvalidT.EnsureValidAsync(_validator);
        r2.ShouldBeFailure();
        Assert.Equal(ErrorType.Validation, r2.Error.Type);

        // Failure -> Bypass => Same Error
        var r3 = await failureT.EnsureValidAsync(_validator);
        r3.ShouldBeFailure();
        Assert.Equal("A", r3.Error.Code); // bypassed
    }

    [Fact]
    public void EnsureValid_Behavior()
    {
        var success = Result.Success(_valid);
        var failure = Result.Failure<TestModel>(Error.Failure("A", "B"));

        // Success -> Valid => Success
        var r1 = success.EnsureValid(_validator);
        r1.ShouldBeSuccess();

        // Success -> Invalid => Failure
        var successInvalid = Result.Success(_invalidAll);
        var r2 = successInvalid.EnsureValid(_validator);
        r2.ShouldBeFailure();
        Assert.Equal(ErrorType.Validation, r2.Error.Type);

        // Failure -> Bypass => Same Error
        var r3 = failure.EnsureValid(_validator);
        r3.ShouldBeFailure();
        Assert.Equal("A", r3.Error.Code); // bypassed
    }
}






