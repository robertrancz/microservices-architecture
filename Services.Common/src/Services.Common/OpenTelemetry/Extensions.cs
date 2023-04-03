using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Services.Common.Settings;
using OpenTelemetry.Trace;

namespace Services.Common.OpenTelemetry
{
    public static class Extensions
    {
        public static IServiceCollection AddTracing(this IServiceCollection services, IConfiguration config)
        {
            services.AddOpenTelemetry().WithTracing(builder => {

                var serviceSettings = config.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
                var jaegerSettings = config.GetSection(nameof(JaegerSettings)).Get<JaegerSettings>();

                builder.AddSource(serviceSettings.ServiceName)
                        .AddSource("MassTransit")
                        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceSettings.ServiceName))
                        .AddHttpClientInstrumentation()
                        .AddAspNetCoreInstrumentation()
                        .AddJaegerExporter(options =>
                        {
                            options.AgentHost = jaegerSettings.Host;
                            options.AgentPort = jaegerSettings.Port;
                        });
            });

            //services.AddConsumeObserver<IConsumeObserver>();

            return services;
        }

        public static IServiceCollection AddMetrics(this IServiceCollection services, IConfiguration config)
        {
            services.AddOpenTelemetry().WithMetrics(builder => {

                var serviceSettings = config.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();

                builder.AddMeter(serviceSettings.ServiceName)
                        .AddMeter("MassTransit")
                        //.AddRuntimeInstrumentation()
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceSettings.ServiceName))
                        .AddPrometheusExporter(/*options => {
                            options.StartHttpListener = true;
                            options.HttpListenerPrefixes = new[] { "http://localhost:9184/" };
                        }*/)
                        .AddOtlpExporter(options => { options.Endpoint = new Uri("http://otel-collector:4317"); });
            });
            
            return services;
        }
    }
}