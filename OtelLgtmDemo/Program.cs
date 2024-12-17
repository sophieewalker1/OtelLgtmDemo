using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics.Metrics;

var builder = WebApplication.CreateBuilder(args);

// OpenTelemetry Resource configuration
var resourceBuilder = ResourceBuilder.CreateDefault()
    .AddService("OpenTelemetryDemo");

// Add OpenTelemetry services
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("OpenTelemetryDemo"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter());

// Logging configuration
builder.Logging.AddOpenTelemetry(options =>
{
    options.SetResourceBuilder(resourceBuilder);
    options.AddOtlpExporter();
});

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Custom meter and instrument for demonstration
var meter = new Meter("OpenTelemetryDemo.Metrics");
var requestCounter = meter.CreateCounter<long>("demo_requests_total", description: "Total number of requests");

app.Use(async (context, next) =>
{
    requestCounter.Add(1);
    await next.Invoke();
});

app.Run();