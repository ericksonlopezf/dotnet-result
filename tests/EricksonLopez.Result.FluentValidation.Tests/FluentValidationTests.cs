using System.Threading.Tasks;
using FluentValidation;
using EricksonLopez.Result;
using EricksonLopez.Result.FluentValidation;
using EricksonLopez.Result.Testing;
using Xunit;

namespace EricksonLopez.Result.FluentValidation.Tests;

public class FluentValidationTests
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
            RuleFor(x => x.Name).NotEmpty().WithErrorCode("NameRequired").WithMessage("Name must not be empty.");
            RuleFor(x => x.Age).GreaterThanOrEqualTo(18).WithMessage("Must be an adult.");
        }
    }

    [Fact]
    public void ToResult_ValidModel_ReturnsSuccess()
    {
        var validator = new TestModelValidator();
        var model = new TestModel { Name = "Erickson", Age = 25 };

        var validationResult = validator.Validate(model);
        var result = validationResult.ToValidationResult();

        result.ShouldBeSuccess();
    }

    [Fact]
    public void ToResult_InvalidModel_ReturnsValidationError()
    {
        var validator = new TestModelValidator();
        var model = new TestModel { Name = "", Age = 15 };

        var validationResult = validator.Validate(model);
        var result = validationResult.ToValidationResult();

        result.ShouldBeFailure()
              .ShouldHaveErrorType(ErrorType.Validation)
              .ShouldHaveSeverity(ErrorSeverity.Warning);

        Assert.Equal(2, result.Error.InnerErrors.Length);
    }

    [Fact]
    public void ToResultOfT_ValidModel_ReturnsSuccessWithValue()
    {
        var validator = new TestModelValidator();
        var model = new TestModel { Name = "Erickson", Age = 25 };

        var validationResult = validator.Validate(model);
        var result = validationResult.ToValidationResult(model);

        result.ShouldHaveValue(model);
    }

    [Fact]
    public async Task ValidateAsync_ValidModel_ReturnsSuccess()
    {
        var validator = new TestModelValidator();
        var model = new TestModel { Name = "Erickson", Age = 25 };

        var validationResult = await validator.ValidateAsync(model);
        var result = validationResult.ToValidationResult();
        result.ShouldBeSuccess();
    }

    [Fact]
    public void EnsureValid_InPipeline_AppliesValidation()
    {
        var validator = new TestModelValidator();
        var validModel = new TestModel { Name = "Erickson", Age = 25 };
        var invalidModel = new TestModel { Name = "", Age = 15 };

        Result.Success(validModel).EnsureValid(validator).ShouldBeSuccess();
        Result.Success(invalidModel).EnsureValid(validator).ShouldBeFailure();
    }
}

