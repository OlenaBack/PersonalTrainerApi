using FluentAssertions;
using PersonalTrainer.Api.Common;
using Xunit;

namespace PersonalTrainer.Api.Tests.Unit.Common;

public class ResultTests
{
    [Fact]
    public void Success_HasNoError()
    {
        var result = Result<string>.Success("value");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("value");
    }

    [Fact]
    public void Failure_ExposesError()
    {
        var error = Error.NotFound("Test.NotFound", "not found");

        var result = Result<string>.Failure(error);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Value_OnFailure_Throws()
    {
        var result = Result<string>.Failure(Error.Conflict("Test.Conflict", "conflict"));

        var act = () => result.Value;

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ImplicitConversion_FromError_CreatesFailure()
    {
        Result<int> result = Error.Validation("Test.Invalid", "invalid");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ImplicitConversion_FromValue_CreatesSuccess()
    {
        Result<int> result = 42;

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }
}
