using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Api.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static void SetupLogging(this WebApplicationBuilder builder)
    {
        builder.Logging.AddOpenTelemetry();

        var otel = builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService("workouts"))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation())
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddEntityFrameworkCoreInstrumentation());

        var otelEndpointUri = builder.Configuration.GetSection("OpenTelemetry")["Uri"];
        if (!string.IsNullOrWhiteSpace(otelEndpointUri))
        {
            otel.UseOtlpExporter(OpenTelemetry.Exporter.OtlpExportProtocol.Grpc, new Uri(otelEndpointUri));
        }
    }
}
