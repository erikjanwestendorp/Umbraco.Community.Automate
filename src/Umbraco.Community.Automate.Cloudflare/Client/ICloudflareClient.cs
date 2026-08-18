namespace Umbraco.Community.Automate.Cloudflare.Client;

public interface ICloudflareClient
{
    Task PurgeUrlsAsync(
        string apiToken,
        string zoneId,
        IEnumerable<string> urls,
        CancellationToken cancellationToken = default);
}