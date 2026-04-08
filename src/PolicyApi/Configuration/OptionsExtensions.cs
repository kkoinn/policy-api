public static class OptionsExtensions
{
    extension(IServiceCollection services)
    {
        public void AddPolicyApiOptions()
        {
            services.AddOptions<ServiceBusOptions>()
                .BindConfiguration(ServiceBusOptions.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddOptions<GuidewireOptions>()
                .BindConfiguration(GuidewireOptions.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddOptions<AppConfigurationOptions>()
                .BindConfiguration(AppConfigurationOptions.SectionName);
        }
    }
}
