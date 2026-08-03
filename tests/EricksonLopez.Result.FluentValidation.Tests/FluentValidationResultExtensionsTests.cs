using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using AwesomeAssertions;
using EricksonLopez.Result.FluentValidation;
using EricksonLopez.Result.Testing;
using Xunit;

namespace EricksonLopez.Result.FluentValidation.Tests;

public class TestModel
{
    public string Name { get; set; } = "";
    public int Age { get; set; }
}

public class TestModelValidator : AbstractValidator<TestModel>
{
    public TestModelValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithErrorCode("NAME_EMPTY").WithMessage("Name is required");
        RuleFor(x => x.Age).GreaterThan(18).WithErrorCode("AGE_TOO_YOUNG").WithMessage("Must be older than 18");
    }
}

public class FluentValidationResultExtensionsTests
{
    [Fact]
    public void ToResult_Valid_ReturnsSuccess()
    {
        var validationResult = new ValidationResult();
        var result = validationResult.ToValidationResult();
        result.ShouldBeSuccess();
    }

    [Fact]
    public void ToResult_Invalid_ReturnsFailureWithStructuredErrors()
    {
        var validationResult = new ValidationResult(new[]
        {
            new ValidationFailure("Name", "Name is required") { ErrorCode = "NAME_EMPTY", AttemptedValue = "" }
        });
        
        var result = validationResult.ToValidationResult();
        result.ShouldBeFailure();
        result.Error.Type.Should().Be(ErrorType.Validation);
        result.Error.Code.Should().Be("Validation.Failed");
        
        result.Error.InnerErrors.Length.Should().Be(1);
        var inner = result.Error.InnerErrors[0];
        inner.Type.Should().Be(ErrorType.Validation);
        inner.Code.Should().Be("NAME_EMPTY");
        inner.Description.Should().Be("Name is required");
        inner.Metadata["propertyName"].Should().Be("Name");
        inner.Metadata["attemptedValue"].Should().Be("");
    }

    [Fact]
    public void ToResultOfT_Valid_ReturnsSuccessWithValue()
    {
        var validationResult = new ValidationResult();
        var result = validationResult.ToValidationResult(10);
        result.ShouldBeSuccess().Should().Be(10);
    }

    [Fact]
    public void ToResultOfT_Invalid_ReturnsFailure()
    {
        var validationResult = new ValidationResult(new[] { new ValidationFailure("Prop", "Error") });
        var result = validationResult.ToValidationResult(10);
        result.ShouldBeFailure();
    }

    [Fact]
    public void Validate_Valid_ReturnsSuccess()
    {
        var validator = new TestModelValidator();
        var result = FluentValidationResultExtensions.ValidateToResult(validator, new TestModel { Name = "John", Age = 20 });
        result.ShouldBeSuccess();
    }

    [Fact]
    public void ValidateToResult_Valid_ReturnsSuccessWithValue()
    {
        var validator = new TestModelValidator();
        var model = new TestModel { Name = "John", Age = 20 };
        var result = validator.ValidateToResultWithValue(model);
        result.ShouldBeSuccess().Should().BeSameAs(model);
    }

    [Fact]
    public async Task ValidateAsync_Valid_ReturnsSuccess()
    {
        var validator = new TestModelValidator();
        var result = await FluentValidationResultExtensions.ValidateToResultAsync(validator, new TestModel { Name = "John", Age = 20 }, CancellationToken.None);
        result.ShouldBeSuccess();
    }
    
    [Fact]
    public async Task ValidateToResultAsync_Valid_ReturnsSuccessWithValue()
    {
        var validator = new TestModelValidator();
        var model = new TestModel { Name = "John", Age = 20 };
        var result = await validator.ValidateToResultWithValueAsync(model, CancellationToken.None);
        result.ShouldBeSuccess().Should().BeSameAs(model);
    }

    [Fact]
    public void EnsureValid_SuccessResultAndValid_ReturnsSuccess()
    {
        var validator = new TestModelValidator();
        var model = new TestModel { Name = "John", Age = 20 };
        var result = Result.Success(model);
        var ensure = result.EnsureValid(validator);
        ensure.ShouldBeSuccess().Should().BeSameAs(model);
    }

    [Fact]
    public void EnsureValid_SuccessResultAndInvalid_ReturnsFailure()
    {
        var validator = new TestModelValidator();
        var model = new TestModel { Name = "", Age = 20 };
        var result = Result.Success(model);
        var ensure = result.EnsureValid(validator);
        ensure.ShouldBeFailure().Code.Should().Be("Validation.Failed");
    }

    [Fact]
    public void EnsureValid_FailureResult_ReturnsFailure()
    {
        var validator = new TestModelValidator();
        var result = Result.Failure<TestModel>(Error.Conflict("C", "C"));
        var ensure = result.EnsureValid(validator);
        ensure.ShouldBeFailure().Code.Should().Be("C");
    }

    [Fact]
    public async Task EnsureValidAsync_SuccessResultAndValid_ReturnsSuccess()
    {
        var validator = new TestModelValidator();
        var model = new TestModel { Name = "John", Age = 20 };
        var task = Task.FromResult(Result.Success(model));
        var ensure = await task.EnsureValidAsync(validator);
        ensure.ShouldBeSuccess().Should().BeSameAs(model);
    }

    [Fact]
    public async Task EnsureValidAsync_SuccessResultAndInvalid_ReturnsFailure()
    {
        var validator = new TestModelValidator();
        var model = new TestModel { Name = "", Age = 20 };
        var task = Task.FromResult(Result.Success(model));
        var ensure = await task.EnsureValidAsync(validator);
        ensure.ShouldBeFailure().Code.Should().Be("Validation.Failed");
    }

    [Fact]
    public async Task EnsureValidAsync_FailureResult_ReturnsFailure()
    {
        var validator = new TestModelValidator();
        var task = Task.FromResult(Result.Failure<TestModel>(Error.Conflict("C", "C")));
        var ensure = await task.EnsureValidAsync(validator);
        ensure.ShouldBeFailure().Code.Should().Be("C");
    }

    [Fact]
    public void ToValidationResult_WithPropertyValuePlaceholder_ExcludesPropertyValueFromMetadata()
    {
        var failure = new ValidationFailure("Prop", "Error");
        failure.FormattedMessagePlaceholderValues = new System.Collections.Generic.Dictionary<string, object>
        {
            { "PropertyValue", "The Value" },
            { "OtherPlaceholder", "Other Value" }
        };

        var result = new ValidationResult(new[] { failure }).ToValidationResult();
        
        var innerError = result.Error.InnerErrors[0];
        AwesomeAssertions.AssertionExtensions.Should(innerError!.Metadata).NotContainKey("placeholder.PropertyValue");
        AwesomeAssertions.AssertionExtensions.Should(innerError.Metadata).ContainKey("placeholder.OtherPlaceholder");
    }
}


