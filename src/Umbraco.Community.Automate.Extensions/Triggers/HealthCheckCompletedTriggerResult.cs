namespace Umbraco.Community.Automate.Extensions.Triggers;

public sealed class HealthCheckCompletedTriggerResult
{
    public string Name { get; init; } = string.Empty;
    public string? Group { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? ReadMoreLink { get; init; }
}
