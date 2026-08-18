using Umbraco.Automate.Core.Settings;

namespace Umbraco.Community.Automate.Cloudflare.Actions;

public sealed class PurgeUrlsSettings
{
    [Field(
        Label = "URLs",
        Description = "Absolute URLs to purge from the Cloudflare cache.",
        SupportsBindings = true)]
    public string[] Urls { get; set; } = [];
}
