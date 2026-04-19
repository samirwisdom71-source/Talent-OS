using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace TalentSystem.IntegrationTests;

public sealed class TalentSystemApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("IdentitySeed:RunOnStartup", "false");
        builder.UseSetting(
            "Jwt:SigningKey",
            "INTEGRATION_TESTS_ONLY_KEY________________32");
    }
}
