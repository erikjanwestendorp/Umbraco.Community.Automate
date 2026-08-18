using System.Reflection;
using NSubstitute;
using Umbraco.Automate.Core.Actions;
using Umbraco.Automate.Core.Bindings;
using Umbraco.Automate.Core.Bindings.Filters;
using Umbraco.Automate.Core.Connections;
using Umbraco.Automate.Core.Models;
using Umbraco.Automate.Core.Settings;
using Umbraco.Automate.Core.StepTypes;
using Umbraco.Community.Automate.Cloudflare.Actions;
using Umbraco.Community.Automate.Cloudflare.Client;
using Umbraco.Community.Automate.Cloudflare.Connections;
using Xunit;

namespace Umbraco.Community.Automate.Cloudflare.Tests.Actions;

public sealed class PurgeUrlsActionTests
{
    private const string ApiToken = "test-token";
    private const string ZoneId = "test-zone";

    private readonly ICloudflareClient _cloudflareClient;
    private readonly PurgeUrlsAction _action;
    private readonly List<IEnumerable<string>> _capturedUrlBatches = [];

    public PurgeUrlsActionTests()
    {
        _cloudflareClient = Substitute.For<ICloudflareClient>();
        _cloudflareClient
            .PurgeUrlsAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                _capturedUrlBatches.Add(callInfo.Arg<IEnumerable<string>>().ToArray());
                return Task.CompletedTask;
            });

        var modelResolver = Substitute.For<IEditableModelResolver>();
        var infrastructure = new ActionInfrastructure(modelResolver);
        var bindingEvaluator = new BindingEvaluator(
            new BindingFilterCollection(() => []));

        _action = new PurgeUrlsAction(infrastructure, _cloudflareClient, bindingEvaluator);
    }

    [Fact]
    public async Task ExecuteAsync_BindingResolvesToMultipleUrls_PurgesAllUrlsInSingleRequest()
    {
        var urls = new[] { "https://example.com/a", "https://example.com/b", "https://example.com/c" };
        var context = BuildContext(
            settings: new PurgeUrlsSettings { Urls = "${previous.urls}" },
            bindingData: new Dictionary<string, object?> { ["previous"] = new Dictionary<string, object?> { ["urls"] = urls } },
            connection: BuildConnection());

        var result = await _action.ExecuteAsync(context, CancellationToken.None);

        Assert.Equal(ActionResultStatus.Success, result.Status);
        var output = Assert.IsType<PurgeUrlsOutput>(result.OutputData);
        Assert.Equal(3, output.PurgedUrlCount);
        Assert.Equal(urls, output.PurgedUrls);

        // All URLs sent in exactly one API call
        Assert.Single(_capturedUrlBatches);
        Assert.Equal(urls, _capturedUrlBatches[0].ToArray());
        await _cloudflareClient.Received(1).PurgeUrlsAsync(
            ApiToken, ZoneId, Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_BindingResolvesToSingleItemCollection_PurgesThatUrl()
    {
        var urls = new[] { "https://example.com/page" };
        var context = BuildContext(
            settings: new PurgeUrlsSettings { Urls = "${previous.urls}" },
            bindingData: new Dictionary<string, object?> { ["previous"] = new Dictionary<string, object?> { ["urls"] = urls } },
            connection: BuildConnection());

        var result = await _action.ExecuteAsync(context, CancellationToken.None);

        Assert.Equal(ActionResultStatus.Success, result.Status);
        var output = Assert.IsType<PurgeUrlsOutput>(result.OutputData);
        Assert.Equal(1, output.PurgedUrlCount);
        Assert.Equal(urls, output.PurgedUrls);
    }

    [Fact]
    public async Task ExecuteAsync_DuplicateUrls_DeduplicatesAndPurgesOnce()
    {
        var urls = new[] { "https://example.com/page", "https://example.com/page", "HTTPS://EXAMPLE.COM/PAGE" };
        var context = BuildContext(
            settings: new PurgeUrlsSettings { Urls = "${previous.urls}" },
            bindingData: new Dictionary<string, object?> { ["previous"] = new Dictionary<string, object?> { ["urls"] = urls } },
            connection: BuildConnection());

        var result = await _action.ExecuteAsync(context, CancellationToken.None);

        Assert.Equal(ActionResultStatus.Success, result.Status);
        var output = Assert.IsType<PurgeUrlsOutput>(result.OutputData);
        Assert.Equal(1, output.PurgedUrlCount);
        Assert.Single(output.PurgedUrls);
        Assert.Equal("https://example.com/page", output.PurgedUrls[0]);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidAbsoluteUrl_ReturnsFailedResult()
    {
        var urls = new[] { "https://example.com/valid", "not-a-url" };
        var context = BuildContext(
            settings: new PurgeUrlsSettings { Urls = "${previous.urls}" },
            bindingData: new Dictionary<string, object?> { ["previous"] = new Dictionary<string, object?> { ["urls"] = urls } },
            connection: BuildConnection());

        var result = await _action.ExecuteAsync(context, CancellationToken.None);

        Assert.Equal(ActionResultStatus.Failed, result.Status);
        await _cloudflareClient.DidNotReceive().PurgeUrlsAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_EmptyCollection_ReturnsFailedResult()
    {
        var context = BuildContext(
            settings: new PurgeUrlsSettings { Urls = "${previous.urls}" },
            bindingData: new Dictionary<string, object?> { ["previous"] = new Dictionary<string, object?> { ["urls"] = Array.Empty<string>() } },
            connection: BuildConnection());

        var result = await _action.ExecuteAsync(context, CancellationToken.None);

        Assert.Equal(ActionResultStatus.Failed, result.Status);
        await _cloudflareClient.DidNotReceive().PurgeUrlsAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_NullOrUnresolvedBinding_ReturnsFailedResult()
    {
        // Binding expression that doesn't match anything in the binding data
        var context = BuildContext(
            settings: new PurgeUrlsSettings { Urls = "${previous.urls}" },
            bindingData: new Dictionary<string, object?>(),
            connection: BuildConnection());

        var result = await _action.ExecuteAsync(context, CancellationToken.None);

        Assert.Equal(ActionResultStatus.Failed, result.Status);
        await _cloudflareClient.DidNotReceive().PurgeUrlsAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_AllUrlsSentInSingleCloudflareRequest()
    {
        var urls = new[] { "https://example.com/page-a", "https://example.com/page-b" };
        var context = BuildContext(
            settings: new PurgeUrlsSettings { Urls = "${previous.urls}" },
            bindingData: new Dictionary<string, object?> { ["previous"] = new Dictionary<string, object?> { ["urls"] = urls } },
            connection: BuildConnection());

        await _action.ExecuteAsync(context, CancellationToken.None);

        // Exactly one API request containing all URLs
        await _cloudflareClient.Received(1).PurgeUrlsAsync(
            ApiToken, ZoneId, Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>());
        Assert.Single(_capturedUrlBatches);
        Assert.Equal(urls.Length, _capturedUrlBatches[0].Count());
    }

    private static ActionContext BuildContext(
        PurgeUrlsSettings settings,
        IReadOnlyDictionary<string, object?> bindingData,
        ConfiguredConnection? connection = null)
        => new()
        {
            AutomationId = Guid.NewGuid(),
            RunId = Guid.NewGuid(),
            StepId = Guid.NewGuid(),
            ActionAlias = "community.cloudflare.purgeUrls",
            Settings = settings,
            BindingData = bindingData,
            Connection = connection,
        };

    private static ConfiguredConnection BuildConnection()
    {
        var connectionSettings = new CloudflareConnectionSettings
        {
            ApiToken = ApiToken,
            ZoneId = ZoneId,
        };

        var inner = new Connection
        {
            Alias = "test-cloudflare",
            Name = "Test Cloudflare",
            Type = "community.cloudflare",
        };

        var connectionType = Substitute.For<IConnectionType>();

        var ctor = typeof(ConfiguredConnection)
            .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
            .Single();

        return (ConfiguredConnection)ctor.Invoke([inner, connectionType, connectionSettings]);
    }
}
