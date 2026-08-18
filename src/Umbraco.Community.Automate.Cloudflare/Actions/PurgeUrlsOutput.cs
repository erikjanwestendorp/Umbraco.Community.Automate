namespace Umbraco.Community.Automate.Cloudflare.Actions;

public sealed class PurgeUrlsOutput
{
    public int PurgedUrlCount { get; set; }

    public string[] PurgedUrls { get; set; } = [];
}
