using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace TalentSystem.IntegrationTests;

public sealed class ApiSmokeTests : IClassFixture<TalentSystemApiFactory>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ApiSmokeTests(TalentSystemApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Health_ShouldReturnOk()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/system/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
