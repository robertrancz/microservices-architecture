using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Services.Common.Settings;

namespace Services.Common.MassTransit
{
    public static class Extensions
    {
        public static IServiceCollection AddMassTransitWithRabbitMq(this IServiceCollection services)
        {
            services.AddMassTransit(configure => {

                // Register consumers
                configure.AddConsumers(Assembly.GetEntryAssembly());

                configure.UsingRabbitMq((context, config) => {
                    var configuration = context.GetService<IConfiguration>();
                    var serviceSettings = configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
                    var rabbitMqSettings = configuration.GetSection(nameof(RabbitMqSettings)).Get<RabbitMqSettings>();
                    config.Host(rabbitMqSettings.Host);
                    config.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter(serviceSettings.ServiceName, includeNamespace: false));
                    config.UseMessageRetry(retryConfigurator => {
                        retryConfigurator.Interval(retryCount: 3, interval: TimeSpan.FromSeconds(5));
                    });
                });
            });

            return services;
        }
    }
}