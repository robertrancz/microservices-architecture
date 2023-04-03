using Catalog.Service.Entities;
using Catalog.Service.Metrics;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using Services.Common.Logging;
using Services.Common.MassTransit;
using Services.Common.MongoDB;
using Services.Common.OpenTelemetry;
using Services.Common.Settings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<OtelMetrics>();

builder.Services.AddSeqLogging(builder.Configuration)
                .AddTracing(builder.Configuration)
                .AddMetrics(builder.Configuration);

builder.Services.AddMongo()
                .AddMongoRepository<Item>("Items");

builder.Services.AddMassTransitWithRabbitMq();

builder.Services.AddControllers(options => 
    {
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
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Catalog v1"));
}

app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
