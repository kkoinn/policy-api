using Microsoft.Extensions.Options;

public static class ServicesExtensions
{
    extension(IServiceCollection services)
    {
        public void AddPolicyApiServices()
        {
            services.AddSingleton<IEventPublisher, ServiceBusPublisher>();
            services.AddHttpClient<IGuidewireService, GuidewireService>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<GuidewireOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
            });
        }
    }
}
