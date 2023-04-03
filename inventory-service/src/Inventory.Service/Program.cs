using Inventory.Service.Clients;
using Inventory.Service.Entities;
using Polly;
using Polly.Timeout;
using Services.Common.Logging;
using Services.Common.MassTransit;
using Services.Common.MongoDB;
using Services.Common.OpenTelemetry;

var builder = WebApplication.CreateBuilder(args);

ConfigurationManager config = builder.Configuration;

// Add services to the container.
builder.Services.AddSeqLogging(builder.Configuration)
                .AddTracing(builder.Configuration)
                .AddMetrics(builder.Configuration);
                
builder.Services.AddMongo()
                .AddMongoRepository<InventoryItem>("InventoryItems")
                .AddMongoRepository<CatalogItem>("CatalogItems");

// For async service communication
builder.Services.AddMassTransitWithRabbitMq();

// For sync service communication
AddCatalogHttpClient(builder, config);

builder.Services.AddControllers(options => {
    options.SuppressAsyncSuffixInActionNames = false;            
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

static void AddCatalogHttpClient(WebApplicationBuilder builder, ConfigurationManager config)
{
    var jitter = new Random();

    builder.Services.AddHttpClient<CatalogClient>(client =>
    {
        client.BaseAddress = new Uri(config["CatalogServiceBaseAddress"]);
    })
    .AddTransientHttpErrorPolicy(policyBuilder => policyBuilder.Or<TimeoutRejectedException>().WaitAndRetryAsync(
        retryCount: 5,
        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(jitter.Next(0, 1000)),
        onRetry: (outcome, timespan, retryAttempt) =>
        {
            var serviceProvider = builder.Services.BuildServiceProvider();
            serviceProvider.GetService<ILogger<CatalogClient>>()?
            .LogWarning($"Polly delaying service call for {timespan.TotalSeconds} seconds, then making retry no. {retryAttempt}");
        }
    ))
    .AddTransientHttpErrorPolicy(policyBuilder => policyBuilder.Or<TimeoutRejectedException>().CircuitBreakerAsync(
        handledEventsAllowedBeforeBreaking: 3,
        durationOfBreak: TimeSpan.FromSeconds(15),
        onBreak: (outcome, timespan) =>
        {
            var serviceProvider = builder.Services.BuildServiceProvider();
            serviceProvider.GetService<ILogger<CatalogClient>>()?
            .LogWarning($"Opening the circuit for {timespan.TotalSeconds} seconds...");
        },
        onReset: () =>
        {
            var serviceProvider = builder.Services.BuildServiceProvider();
            serviceProvider.GetService<ILogger<CatalogClient>>()?
            .LogWarning($"Closing the circuit...");
        }
    ))
    .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(1));
}