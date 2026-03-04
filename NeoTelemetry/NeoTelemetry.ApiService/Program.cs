using Neo.Application.Solver.Equation;
using Neo.CompositionRoot;
using NeoTelemetry.ApiService.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add Neo equation solver with telemetry
builder.Services.AddNeoEquationSolverWithTelemetry(options =>
{
    options.EnableCaching = true;
    options.CacheTtl = TimeSpan.FromMinutes(30);
    options.DefaultAlgorithm = SolvingAlgorithm.LU;
    options.ValidationTolerance = 1e-10;
});

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Map equation solver endpoints
app.MapEquationEndpoints();

app.MapDefaultEndpoints();

app.Run();
