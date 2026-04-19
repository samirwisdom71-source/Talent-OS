using TalentSystem.Shared.Results;

namespace TalentSystem.UnitTests;

public sealed class ResultTests
{
    [Fact]
    public void Ok_ShouldBeSuccessful()
    {
        var result = Result.Ok();

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Fail_ShouldContainErrors()
    {
        var result = Result.Fail("error");

        Assert.True(result.IsFailure);
        Assert.Single(result.Errors);
    }
}
