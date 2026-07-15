using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace LibraryClub.Api.Extensions;

public static class HealthCheckExtensions
{
    public static Task WriteHealthCheckResponseAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        var response = new
        {
            Status = report.Status.ToString(),
            TotalDurationInMilliseconds = report.TotalDuration.TotalMilliseconds,
            Dependencies = report.Entries.Select(entry => new
            {
                Name = entry.Key,
                Status = entry.Value.Status.ToString(),
                Description = entry.Value.Description,
                DurationInMilliseconds = entry.Value.Duration.TotalMilliseconds
            })
        };

        return JsonSerializer.SerializeAsync(context.Response.Body, response);
    }
}