using LibraryClub.Api.Data;
using LibraryClub.Api.Repositories;
using LibraryClub.Api.Services;
using FluentValidation;
using LibraryClub.Api.Validators;
using LibraryClub.Api.Middlewares;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");
}

var applicationInsightsConnectionString =
     builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]
     ?? builder.Configuration["ApplicationInsights:ConnectionString"];

if (!builder.Environment.IsEnvironment("Testing") &&
    !string.IsNullOrWhiteSpace(applicationInsightsConnectionString))
{
    builder.Services.AddApplicationInsightsTelemetry(options =>
    {
        options.ConnectionString = applicationInsightsConnectionString;
    });
}

builder.Services.AddSingleton<ISqlConnectionFactory>(new SqlConnectionFactory(connectionString));
builder.Services.AddScoped<IReaderRepository, ReaderRepository>();
builder.Services.AddScoped<IReadingClubRepository, ReadingClubRepository>();
builder.Services.AddScoped<IReaderService, ReaderService>();
builder.Services.AddScoped<IReadingClubService, ReadingClubService>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateReaderRequestValidator>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

var app = builder.Build();

var scriptsPath = Path.Combine(AppContext.BaseDirectory, "Scripts");

DatabaseMigrator.Migrate(connectionString, scriptsPath);

if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "LibraryClub API v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseExceptionHandler();

app.MapControllers();

app.Run();

public partial class Program { }