using ChatAPI;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services.AddSingleton<AISearch>();
builder.Services.AddSingleton<BlobAPI>();

// Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
// builder.Services
//     .AddApplicationInsightsTelemetryWorkerService()
//     .ConfigureFunctionsApplicationInsights();

builder.Build().Run();
