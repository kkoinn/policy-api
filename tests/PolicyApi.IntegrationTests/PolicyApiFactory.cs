using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

public class PolicyApiFactory : WebApplicationFactory<Program>
{
    public FakeEventPublisher EventPublisher { get; } = new();
    public FakeGuidewireService GuidewireService { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ServiceBus:ConnectionString"] = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=dGVzdA==",
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IEventPublisher>();
            services.RemoveAll<IGuidewireService>();
            services.RemoveAll<HttpClient>();
            services.AddSingleton<IEventPublisher>(EventPublisher);
            services.AddSingleton<IGuidewireService>(GuidewireService);
        });
    }
}
