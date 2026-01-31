using OpenTelemetry;
using OpenTelemetry.Exporter;
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
        var otelEndpointProtocol = builder.Configuration.GetSection("OpenTelemetry")["Protocol"];
        if (Uri.TryCreate(otelEndpointUri, UriKind.Absolute, out Uri? uri) && Enum.TryParse(otelEndpointProtocol, out OtlpExportProtocol protocol))
        {
            otel.UseOtlpExporter(protocol, uri);
        }
    }
}
