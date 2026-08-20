namespace Umbraco.Community.Automate.Extensions.Triggers;

public sealed class HealthCheckCompletedTriggerOutput
{
    public bool AllChecksSuccessful { get; init; }
    public int TotalChecks { get; init; }
    public int SuccessfulChecks { get; init; }
    public int InfoChecks { get; init; }
    public int WarningChecks { get; init; }
    public int FailedChecks { get; init; }
    public HealthCheckCompletedTriggerResult[] Results { get; init; } = [];
}
