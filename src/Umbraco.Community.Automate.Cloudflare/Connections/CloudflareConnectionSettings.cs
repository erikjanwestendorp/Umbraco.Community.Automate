using Umbraco.Automate.Core.Settings;

namespace Umbraco.Community.Automate.Cloudflare.Connections;

public sealed class CloudflareConnectionSettings
{
    [Field(Label = "API token", Description = "Cloudflare API token with Cache Purge permissions.", IsSensitive = true)]
    public string ApiToken { get; set; } = string.Empty;

    [Field(Label = "Zone ID", Description = "The Cloudflare Zone ID for the website.")]
    public string ZoneId { get; set; } = string.Empty;
}