using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Api.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static void SetupLogging(this WebApplicationBuilder builder)
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        var otel = builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation())
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddEntityFrameworkCoreInstrumentation());

        var OtlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
        if (OtlpEndpoint != null)
        {
            otel.UseOtlpExporter();
        }
    }
}
