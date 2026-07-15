using FluentValidation;
using LibraryClub.Api.Data;
using LibraryClub.Api.Extensions;
using LibraryClub.Api.Middlewares;
using LibraryClub.Api.Repositories;
using LibraryClub.Api.Services;
using LibraryClub.Api.Validators;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "LibraryClub.Api")
            .Enrich.WithProperty(
                "EnvironmentName",
                context.HostingEnvironment.EnvironmentName);
    });

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

    builder.Services.AddHealthChecks()
    .AddSqlServer(
        connectionString: connectionString,
        name: "SQL Server",
        tags: ["db", "sql", "sqlserver"]);

    builder.Services.AddSingleton<ISqlConnectionFactory>(new SqlConnectionFactory(connectionString));

    builder.Services.AddScoped<IReaderRepository, ReaderRepository>();
    builder.Services.AddScoped<IReadingClubRepository, ReadingClubRepository>();
    builder.Services.AddScoped<IClubSubscriptionRepository, ClubSubscriptionRepository>();

    builder.Services.AddScoped<IReaderService, ReaderService>();
    builder.Services.AddScoped<IReadingClubService, ReadingClubService>();
    builder.Services.AddScoped<IClubSubscriptionService, ClubSubscriptionService>();

    builder.Services.AddValidatorsFromAssemblyContaining<CreateReaderRequestValidator>();

    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    builder.Services.AddControllers();

    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen(options =>
    {
        var xmlFile = $"{typeof(Program).Assembly.GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }
    });

    var app = builder.Build();

    if (!app.Environment.IsEnvironment("Testing"))
    {
        var scriptsPath = Path.Combine(AppContext.BaseDirectory, "Scripts");

        DatabaseMigrator.Migrate(connectionString, scriptsPath);

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "LibraryClub API v1");
        });
    }

    app.UseExceptionHandler();

    app.UseHttpsRedirection();

    app.UseSerilogRequestLogging();

    app.UseAuthorization();

    app.MapControllers();

    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = HealthCheckExtensions.WriteHealthCheckResponseAsync
    });

    app.Run();
}
catch (Exception exception)
{
    Log.Fatal(exception, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program;