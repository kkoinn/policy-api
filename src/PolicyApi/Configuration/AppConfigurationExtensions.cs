using Azure.Identity;

public static class AppConfigurationExtensions
{
    extension(WebApplicationBuilder builder)
    {
        public void AddAzureAppConfiguration()
        {
            var endpoint = builder.Configuration["AzureAppConfiguration:Endpoint"];

            if (string.IsNullOrEmpty(endpoint))
                return;

            var clientId = builder.Configuration["AZURE_CLIENT_ID"]
                ?? throw new InvalidOperationException("AZURE_CLIENT_ID is required for user-assigned managed identity.");

            builder.Configuration.AddAzureAppConfiguration(options =>
                options
                    .Connect(new Uri(endpoint), new ManagedIdentityCredential(ManagedIdentityId.FromUserAssignedClientId(clientId)))
                    .UseFeatureFlags(flags =>
                        flags.SetRefreshInterval(TimeSpan.FromSeconds(30))
                    )
            );

            builder.Services.AddAzureAppConfiguration();
        }
    }
}
