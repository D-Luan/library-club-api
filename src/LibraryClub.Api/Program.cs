using LibraryClub.Api.Data;
using LibraryClub.Api.Repositories;
using DbUp;
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

builder.Services.AddSingleton<ISqlConnectionFactory>(new SqlConnectionFactory(connectionString));
builder.Services.AddScoped<IReaderRepository, ReaderRepository>();
builder.Services.AddScoped<IReaderService, ReaderService>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateReaderRequestValidator>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

var app = builder.Build();

EnsureDatabase.For.SqlDatabase(connectionString);

var scriptsPath = Path.Combine(AppContext.BaseDirectory, "Scripts");

var upgrader = DeployChanges.To
    .SqlDatabase(connectionString)
    .WithScriptsFromFileSystem(scriptsPath)
    .LogToConsole()
    .Build();

var result = upgrader.PerformUpgrade();
if (!result.Successful)
{
    throw new InvalidOperationException("Database migration failed.", result.Error);
}

if (app.Environment.IsDevelopment())
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