using Umbraco.Automate.Core.Actions;
using Umbraco.Community.Automate.Cloudflare.Client;
using Umbraco.Community.Automate.Cloudflare.Connections;

namespace Umbraco.Community.Automate.Cloudflare.Actions;

[Action(
    AutomateCloudflareConstants.Actions.PurgeUrl.Alias,
    AutomateCloudflareConstants.Actions.PurgeUrl.Name,
    Description = AutomateCloudflareConstants.Actions.PurgeUrl.Description,
    Group = AutomateCloudflareConstants.Groups.Cloudflare,
    Icon = AutomateCloudflareConstants.Icons.Cloud,
    ConnectionTypeAlias = AutomateCloudflareConstants.ConnectionTypes.Cloudflare.Alias)]
public class PurgeUrlAction
    : ActionBase<PurgeUrlSettings, PurgeUrlOutput>
{
    private readonly ICloudflareClient _cloudflareClient;

    public PurgeUrlAction(
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
        var settings = context.GetSettings<PurgeUrlSettings>();

        if (string.IsNullOrWhiteSpace(settings.Url))
        {
            return ActionResult.Failed(new ArgumentException("A URL is required."));
        }
            

        if (!Uri.TryCreate(settings.Url, UriKind.Absolute, out _))
        {
            return ActionResult.Failed(new ArgumentException($"'{settings.Url}' is not a valid absolute URL."));
        }
            

        var connection = context.Connection
            ?? throw new InvalidOperationException("A Cloudflare connection is required.");

        var connectionSettings =
            connection.GetSettings<CloudflareConnectionSettings>();

        await _cloudflareClient.PurgeUrlAsync(
            connectionSettings.ApiToken,
            connectionSettings.ZoneId,
            settings.Url,
            cancellationToken);

        return Success(new PurgeUrlOutput { PurgedUrl = settings.Url });
    }
}
