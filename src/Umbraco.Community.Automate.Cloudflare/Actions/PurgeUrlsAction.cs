using System.Text.Json;
using Umbraco.Automate.Core.Actions;
using Umbraco.Automate.Core.Bindings;
using Umbraco.Community.Automate.Cloudflare.Client;
using Umbraco.Community.Automate.Cloudflare.Connections;

namespace Umbraco.Community.Automate.Cloudflare.Actions;

[Action(
    AutomateCloudflareConstants.Actions.PurgeUrls.Alias,
    AutomateCloudflareConstants.Actions.PurgeUrls.Name,
    Description = AutomateCloudflareConstants.Actions.PurgeUrls.Description,
    Group = AutomateCloudflareConstants.Groups.Cloudflare,
    Icon = AutomateCloudflareConstants.Icons.Cloud,
    ConnectionTypeAlias = AutomateCloudflareConstants.ConnectionTypes.Cloudflare.Alias)]
public sealed class PurgeUrlsAction
    : ActionBase<PurgeUrlsSettings, PurgeUrlsOutput>
{
    private readonly ICloudflareClient _cloudflareClient;
    private readonly BindingEvaluator _bindingEvaluator;

    public PurgeUrlsAction(
        ActionInfrastructure infrastructure,
        ICloudflareClient cloudflareClient,
        BindingEvaluator bindingEvaluator)
        : base(infrastructure)
    {
        _cloudflareClient = cloudflareClient;
        _bindingEvaluator = bindingEvaluator;
    }

    public override async Task<ActionResult> ExecuteAsync(
        ActionContext context,
        CancellationToken cancellationToken)
    {
        var settings = context.GetSettings<PurgeUrlsSettings>();

        var value = _bindingEvaluator.EvaluateRaw(
            settings.Urls,
            context.BindingData
                ?? new Dictionary<string, object?>());

        var urls = value switch
        {
            string[] array => array,

            IEnumerable<string> enumerable =>
                enumerable.ToArray(),

            IEnumerable<object?> objects =>
                objects
                    .Select(x => x?.ToString())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Cast<string>()
                    .ToArray(),

            string json =>
                TryDeserializeUrls(json),

            _ => []
        };

        urls = urls
            .Where(u => !string.IsNullOrWhiteSpace(u))
            .Select(u => u.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (urls.Length == 0)
        {
            return ActionResult.Failed(new ArgumentException("At least one URL is required."));
        }

        var invalidUrl = Array.Find(urls, u => !Uri.TryCreate(u, UriKind.Absolute, out _));
        if (invalidUrl is not null)
        {
            return ActionResult.Failed(new ArgumentException($"'{invalidUrl}' is not a valid absolute URL."));
        }

        var connection = context.Connection
            ?? throw new InvalidOperationException("A Cloudflare connection is required.");

        var connectionSettings =
            connection.GetSettings<CloudflareConnectionSettings>();

        await _cloudflareClient.PurgeUrlsAsync(
            connectionSettings.ApiToken,
            connectionSettings.ZoneId,
            urls,
            cancellationToken);

        return Success(new PurgeUrlsOutput
        {
            PurgedUrlCount = urls.Length,
            PurgedUrls = urls,
        });
    }

    private static string[] TryDeserializeUrls(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<string[]>(value) ?? [];
        }
        catch (JsonException)
        {
            return [value];
        }
    }
}
