using Umbraco.Automate.Core.Actions;
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

    public PurgeUrlsAction(
        ActionInfrastructure infrastructure,
        ICloudflareClient cloudflareClient)
        : base(infrastructure)
    {
        _cloudflareClient = cloudflareClient;
    }

    public override async Task<ActionResult> ExecuteAsync(
        ActionContext context,
        CancellationToken cancellationToken)
    {
        var settings = context.GetSettings<PurgeUrlsSettings>();

        var urls = (settings.Urls ?? [])
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
}
