using Umbraco.Automate.Core.Settings;
using Umbraco.Automate.Core.Triggers;
using Umbraco.Cms.Core.HealthChecks;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Community.Automate.Extensions.Triggers;
using Xunit;

namespace Umbraco.Community.Automate.Extensions.Tests.Triggers;

public sealed class HealthCheckCompletedTriggerTests
{
    private readonly HealthCheckCompletedTrigger _trigger;

    public HealthCheckCompletedTriggerTests()
    {
        var infrastructure =
            new TriggerInfrastructure(
                new NullEditableModelResolver());

        var healthChecks =
            new HealthCheckCollection(
                () =>
                [
                    new SuccessHealthCheck(),
                    new WarningHealthCheck(),
                    new ErrorHealthCheck(),
                    new InfoHealthCheck()
                ]);

        _trigger =
            new HealthCheckCompletedTrigger(
                infrastructure,
                healthChecks);
    }

    [Fact]
    public async Task MapEvent_MapsHealthCheckResultsToExpectedOutput()
    {
        var notification =
            new HealthCheckCompletedNotification(
                await HealthCheckResults.Create(
                [
                    new SuccessHealthCheck(),
                    new WarningHealthCheck(),
                    new ErrorHealthCheck(),
                    new InfoHealthCheck()
                ]));

        var mappedEvents =
            _trigger.MapEvent(notification).ToArray();

        var mappedEvent =
            Assert.IsType<TriggerEvent<HealthCheckCompletedTriggerOutput>>(
                Assert.Single(mappedEvents));

        Assert.Equal(
            "community.extensions.healthCheckCompleted",
            mappedEvent.TriggerAlias);
        Assert.Equal("system", mappedEvent.InitiatorType);
        Assert.False(string.IsNullOrWhiteSpace(mappedEvent.IdempotencyKey));

        var output = mappedEvent.Output;

        Assert.False(output.AllChecksSuccessful);
        Assert.Equal(4, output.TotalChecks);
        Assert.Equal(1, output.SuccessfulChecks);
        Assert.Equal(1, output.InfoChecks);
        Assert.Equal(1, output.WarningChecks);
        Assert.Equal(1, output.FailedChecks);
        Assert.Equal(4, output.Results.Length);

        var successResult =
            Assert.Single(output.Results, result => result.Name == "Success check");
        Assert.Equal("Content", successResult.Group);
        Assert.Equal("Success", successResult.Status);
        Assert.Equal("Everything is healthy.", successResult.Message);

        var warningResult =
            Assert.Single(output.Results, result => result.Name == "Warning check");
        Assert.Equal("Members", warningResult.Group);
        Assert.Equal("Warning", warningResult.Status);
        Assert.Equal("Something needs attention.", warningResult.Message);

        var failedResult =
            Assert.Single(output.Results, result => result.Name == "Error check");
        Assert.Equal("Settings", failedResult.Group);
        Assert.Equal("Error", failedResult.Status);
        Assert.Equal("Something failed.", failedResult.Message);
        Assert.Equal("https://example.com/error", failedResult.ReadMoreLink);
    }

    [Fact]
    public async Task MapEvent_ProducesStableIdempotencyKeyForEquivalentResults()
    {
        var firstNotification =
            new HealthCheckCompletedNotification(
                await HealthCheckResults.Create(
                [
                    new SuccessHealthCheck(),
                    new WarningHealthCheck(),
                    new ErrorHealthCheck(),
                    new InfoHealthCheck()
                ]));

        var secondNotification =
            new HealthCheckCompletedNotification(
                await HealthCheckResults.Create(
                [
                    new InfoHealthCheck(),
                    new ErrorHealthCheck(),
                    new WarningHealthCheck(),
                    new SuccessHealthCheck()
                ]));

        var firstEvent =
            Assert.IsType<TriggerEvent<HealthCheckCompletedTriggerOutput>>(
                Assert.Single(_trigger.MapEvent(firstNotification)));
        var secondEvent =
            Assert.IsType<TriggerEvent<HealthCheckCompletedTriggerOutput>>(
                Assert.Single(_trigger.MapEvent(secondNotification)));

        Assert.Equal(firstEvent.IdempotencyKey, secondEvent.IdempotencyKey);
    }

    [Fact]
    public async Task MapEvent_MultiStatusCheckCountsDistinctChecksAndPreservesAllStatuses()
    {
        var healthChecks =
            new HealthCheckCollection(
                () =>
                [
                    new MultiStatusWarningHealthCheck()
                ]);

        var trigger =
            new HealthCheckCompletedTrigger(
                new TriggerInfrastructure(new NullEditableModelResolver()),
                healthChecks);

        var notification =
            new HealthCheckCompletedNotification(
                await HealthCheckResults.Create(
                [
                    new MultiStatusWarningHealthCheck()
                ]));

        var mappedEvent =
            Assert.IsType<TriggerEvent<HealthCheckCompletedTriggerOutput>>(
                Assert.Single(trigger.MapEvent(notification)));

        Assert.True(mappedEvent.Output.AllChecksSuccessful);
        Assert.Equal(1, mappedEvent.Output.TotalChecks);
        Assert.Equal(0, mappedEvent.Output.SuccessfulChecks);
        Assert.Equal(0, mappedEvent.Output.InfoChecks);
        Assert.Equal(1, mappedEvent.Output.WarningChecks);
        Assert.Equal(0, mappedEvent.Output.FailedChecks);
        Assert.Equal(2, mappedEvent.Output.Results.Length);
        Assert.Contains(mappedEvent.Output.Results, result => result.Status == "Success");
        Assert.Contains(mappedEvent.Output.Results, result => result.Status == "Warning");
    }

    private sealed class NullEditableModelResolver : IEditableModelResolver
    {
        public EditableModelSchema Resolve(Type type) => throw new NotSupportedException();

        public TModel? ResolveModel<TModel>(string modelId, object? data, EditableModelSchema? schema = null)
            where TModel : class, new()
            => throw new NotSupportedException();

        public object? ResolveModel(string modelId, Type type, object? data, EditableModelSchema? schema = null)
            => throw new NotSupportedException();
    }

    [HealthCheck("11111111-1111-1111-1111-111111111111", "Success check", Description = "Success", Group = "Content")]
    private sealed class SuccessHealthCheck : HealthCheck
    {
        public override Task<IEnumerable<HealthCheckStatus>> GetStatusAsync()
        => Task.FromResult<IEnumerable<HealthCheckStatus>>(
        [
            new HealthCheckStatus("Everything is healthy.")
            {
                ResultType = StatusResultType.Success
            }
        ]);
    }

    [HealthCheck("22222222-2222-2222-2222-222222222222", "Warning check", Description = "Warning", Group = "Members")]
    private sealed class WarningHealthCheck : HealthCheck
    {
        public override Task<IEnumerable<HealthCheckStatus>> GetStatusAsync()
        => Task.FromResult<IEnumerable<HealthCheckStatus>>(
        [
            new HealthCheckStatus("Something needs attention.")
            {
                ResultType = StatusResultType.Warning
            }
        ]);
    }

    [HealthCheck("33333333-3333-3333-3333-333333333333", "Error check", Description = "Error", Group = "Settings")]
    private sealed class ErrorHealthCheck : HealthCheck
    {
        public override Task<IEnumerable<HealthCheckStatus>> GetStatusAsync()
        => Task.FromResult<IEnumerable<HealthCheckStatus>>(
        [
            new HealthCheckStatus("Something failed.")
            {
                ResultType = StatusResultType.Error,
                Description = "Detailed failure",
                ReadMoreLink = "https://example.com/error"
            }
        ]);
    }

    [HealthCheck("44444444-4444-4444-4444-444444444444", "Info check", Description = "Info", Group = "Content")]
    private sealed class InfoHealthCheck : HealthCheck
    {
        public override Task<IEnumerable<HealthCheckStatus>> GetStatusAsync()
        => Task.FromResult<IEnumerable<HealthCheckStatus>>(
        [
            new HealthCheckStatus("Informational result.")
            {
                ResultType = StatusResultType.Info
            }
        ]);
    }

    [HealthCheck("55555555-5555-5555-5555-555555555555", "Multi-status warning check", Description = "Multi", Group = "Content")]
    private sealed class MultiStatusWarningHealthCheck : HealthCheck
    {
        public override Task<IEnumerable<HealthCheckStatus>> GetStatusAsync()
        => Task.FromResult<IEnumerable<HealthCheckStatus>>(
        [
            new HealthCheckStatus("Part one passed.")
            {
                ResultType = StatusResultType.Success
            },
            new HealthCheckStatus("Part two needs attention.")
            {
                ResultType = StatusResultType.Warning
            }
        ]);
    }
}
