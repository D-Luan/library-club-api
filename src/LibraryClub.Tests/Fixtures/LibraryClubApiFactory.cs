using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LibraryClub.Tests.Fixtures;

public sealed class LibraryClubApiFactory : WebApplicationFactory<Program>
{
    private const string ConnectionStringEnvironmentVariable =
        "ConnectionStrings__DefaultConnection";

    private readonly string? _previousConnectionString;

    public LibraryClubApiFactory(string connectionString)
    {
        _previousConnectionString = Environment.GetEnvironmentVariable(
            ConnectionStringEnvironmentVariable);

        Environment.SetEnvironmentVariable(
            ConnectionStringEnvironmentVariable,
            connectionString);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
    }

    protected override void Dispose(bool disposing)
    {
        Environment.SetEnvironmentVariable(
            ConnectionStringEnvironmentVariable,
            _previousConnectionString);

        base.Dispose(disposing);
    }
}