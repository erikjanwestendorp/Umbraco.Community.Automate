using System.Security.Cryptography;
using System.Text;
using Umbraco.Automate.Core.Triggers;
using Umbraco.Cms.Core.HealthChecks;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Community.Automate.Extensions.Triggers;

[Trigger(
    AutomateExtensionsConstants.Triggers.HealthCheckCompleted.Alias,
    AutomateExtensionsConstants.Triggers.HealthCheckCompleted.Name,
    Description = AutomateExtensionsConstants.Triggers.HealthCheckCompleted.Description,
    Group = AutomateExtensionsConstants.Groups.Community,
    Icon = AutomateExtensionsConstants.Icons.Health)]
public sealed class HealthCheckCompletedTrigger(
    TriggerInfrastructure infrastructure,
    HealthCheckCollection healthChecks)
    : NotificationTriggerBase<object, HealthCheckCompletedTriggerOutput, HealthCheckCompletedNotification>(infrastructure)
{
    private static readonly StatusResultType[] StatusBuckets =
    [
        StatusResultType.Success,
        StatusResultType.Warning,
        StatusResultType.Error,
        StatusResultType.Info
    ];

    private readonly IReadOnlyDictionary<string, string?> _healthCheckGroups =
        healthChecks.ToDictionary(
            healthCheck => healthCheck.Name,
            healthCheck => healthCheck.Group,
            StringComparer.OrdinalIgnoreCase);

    public override IEnumerable<TriggerEvent> MapEvent(HealthCheckCompletedNotification notification)
    {
        var groupedResults =
            GetGroupedResults(notification.HealthCheckResults);

        var output =
            new HealthCheckCompletedTriggerOutput
            {
                AllChecksSuccessful = notification.HealthCheckResults.AllChecksSuccessful,
                TotalChecks = groupedResults.Count,
                SuccessfulChecks = CountChecks(notification.HealthCheckResults, StatusResultType.Success),
                InfoChecks = CountChecks(notification.HealthCheckResults, StatusResultType.Info),
                WarningChecks = CountChecks(notification.HealthCheckResults, StatusResultType.Warning),
                FailedChecks = CountChecks(notification.HealthCheckResults, StatusResultType.Error),
                Results = groupedResults
                    .SelectMany(
                        group => group.Value.Select(
                            status => new HealthCheckCompletedTriggerResult
                            {
                                Name = group.Key,
                                Group = _healthCheckGroups.GetValueOrDefault(group.Key),
                                Status = status.ResultType.ToString(),
                                Message = status.Message,
                                Description = status.Description,
                                ReadMoreLink = status.ReadMoreLink
                            }))
                    .ToArray()
            };

        yield return new TriggerEvent<HealthCheckCompletedTriggerOutput>
        {
            TriggerAlias = Alias,
            InitiatorType = "system",
            IdempotencyKey = GenerateIdempotencyKey(output),
            Output = output
        };
    }

    private static Dictionary<string, List<HealthCheckStatus>> GetGroupedResults(HealthCheckResults results)
    {
        var groupedResults =
            new Dictionary<string, List<HealthCheckStatus>>(
                StringComparer.OrdinalIgnoreCase);

        foreach (var statusBucket in StatusBuckets)
        {
            foreach (var group in results.GetResultsForStatus(statusBucket) ?? [])
            {
                if (!groupedResults.TryGetValue(group.Key, out var statuses))
                {
                    statuses = [];
                    groupedResults[group.Key] = statuses;
                }

                statuses.AddRange(group.Value);
            }
        }

        return groupedResults;
    }

    private static int CountChecks(HealthCheckResults results, StatusResultType statusResultType)
    => results.GetResultsForStatus(statusResultType)?.Count ?? 0;

    private static string GenerateIdempotencyKey(HealthCheckCompletedTriggerOutput output)
    {
        var builder = new StringBuilder();

        builder.Append(output.AllChecksSuccessful)
            .Append('|')
            .Append(output.TotalChecks)
            .Append('|')
            .Append(output.SuccessfulChecks)
            .Append('|')
            .Append(output.InfoChecks)
            .Append('|')
            .Append(output.WarningChecks)
            .Append('|')
            .Append(output.FailedChecks);

        foreach (var result in output.Results
                     .OrderBy(result => result.Name, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(result => result.Status, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(result => result.Message, StringComparer.Ordinal))
        {
            builder.Append('|')
                .Append(result.Name)
                .Append('|')
                .Append(result.Group)
                .Append('|')
                .Append(result.Status)
                .Append('|')
                .Append(result.Message)
                .Append('|')
                .Append(result.Description)
                .Append('|')
                .Append(result.ReadMoreLink);
        }

        return Convert.ToHexString(
            SHA256.HashData(
                Encoding.UTF8.GetBytes(
                    builder.ToString())));
    }
}
