using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Umbraco.Community.Automate.Cloudflare.Client;

internal sealed class CloudflareClient(HttpClient httpClient) : ICloudflareClient
{
    public async Task PurgeUrlAsync(
        string apiToken,
        string zoneId,
        string url,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(apiToken);
        ArgumentException.ThrowIfNullOrWhiteSpace(zoneId);
        ArgumentException.ThrowIfNullOrWhiteSpace(url);

        if (!Uri.TryCreate(url, UriKind.Absolute, out _))
            throw new ArgumentException($"'{url}' is not a valid absolute URL.", nameof(url));

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"zones/{zoneId}/purge_cache");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);
        request.Content = JsonContent.Create(new { files = new[] { url } });

        using var response = await httpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
            return;

        var responseBody =
            await response.Content.ReadAsStringAsync(cancellationToken);

        throw new HttpRequestException(
            $"Cloudflare cache purge failed with status code {(int)response.StatusCode}: {responseBody}");
    }
}
