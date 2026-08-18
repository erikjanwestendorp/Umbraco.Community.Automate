using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Umbraco.Community.Automate.Cloudflare.Client;

internal sealed class CloudflareClient(HttpClient httpClient) : ICloudflareClient
{
    public async Task PurgeUrlsAsync(
        string apiToken,
        string zoneId,
        IEnumerable<string> urls,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(apiToken);
        ArgumentException.ThrowIfNullOrWhiteSpace(zoneId);

        ArgumentNullException.ThrowIfNull(urls);

        var files = urls.ToArray();

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"zones/{zoneId}/purge_cache");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);
        request.Content = JsonContent.Create(new { files });

        using var response = await httpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
            return;

        var responseBody =
            await response.Content.ReadAsStringAsync(cancellationToken);

        throw new HttpRequestException(
            $"Cloudflare cache purge failed with status code {(int)response.StatusCode}: {responseBody}");
    }
}
