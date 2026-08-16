namespace Umbraco.Community.Automate.Cloudflare.Client;

public interface ICloudflareClient
{
    Task PurgeUrlAsync(
        string apiToken,
        string zoneId,
        string url,
        CancellationToken cancellationToken = default);
}