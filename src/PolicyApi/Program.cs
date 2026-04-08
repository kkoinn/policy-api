using System.Text.Json.Nodes;
using Microsoft.FeatureManagement;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddAzureAppConfiguration();
builder.Services.AddOpenApi();
builder.Services.AddFeatureManagement();
builder.Services.AddPolicyApiOptions();
builder.Services.AddPolicyApiServices();
builder.AddPolicyApiTelemetry();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseGlobalExceptionHandler();
    app.UseAzureAppConfiguration();
}

app.UseStatusCodePages();
app.MapOpenApi();
app.MapScalarApiReference();
app.MapHealthChecks("/health");
app.UseHttpsRedirection();

var createPolicy = app.MapPost("api/v1/policies", CreatePolicyHandler.HandleAsync)
   .WithName("CreatePolicy")
   .WithSummary("Creates a new policy in Guidewire and publishes an event.");

if (app.Environment.IsDevelopment())
{
    createPolicy.AddOpenApiOperationTransformer((operation, context, ct) =>
    {
        if (operation.RequestBody?.Content?.TryGetValue("application/json", out var mediaType) is true)
        {
            mediaType.Example = JsonNode.Parse("""
                {
                  "customerNumber": "12345",
                  "status": "Active",
                  "startDate": "2026-01-01T00:00:00",
                  "endDate": "2027-01-01T00:00:00",
                  "premium": 1200.00
                }
                """);
        }
        return Task.CompletedTask;
    });
}

app.Run();
