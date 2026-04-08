using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

public static class OpenTelemetryExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        public void AddPolicyApiTelemetry()
        {
            builder.Logging.AddOpenTelemetry(logging =>
            {
                logging.IncludeFormattedMessage = true;
                logging.IncludeScopes = true;
            });

            builder.Services.AddOpenTelemetry()
                .ConfigureResource(r => r.AddService("PolicyApi"));
        }
    }
}
