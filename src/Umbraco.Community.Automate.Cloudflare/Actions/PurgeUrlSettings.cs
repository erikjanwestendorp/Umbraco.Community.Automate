using Umbraco.Automate.Core.Settings;

namespace Umbraco.Community.Automate.Cloudflare.Actions;

public sealed class PurgeUrlSettings
{
    [Field(Label = "URL", Description = "Absolute URL to purge from the Cloudflare cache.", SupportsBindings = true)]
    public string Url { get; set; } = string.Empty;
}