using System.Net.Http.Headers;
using Umbraco.Automate.Core.Connections;

namespace Umbraco.Community.Automate.Cloudflare.Connections;

[ConnectionType(
    AutomateCloudflareConstants.ConnectionTypes.Cloudflare.Alias,
    AutomateCloudflareConstants.ConnectionTypes.Cloudflare.Name,
    Description = AutomateCloudflareConstants.ConnectionTypes.Cloudflare.Description,
    Group = AutomateCloudflareConstants.Groups.Cloudflare,
    Icon = AutomateCloudflareConstants.Icons.Cloud)]
public sealed class CloudflareConnectionType
    : ConnectionTypeBase<CloudflareConnectionSettings>
{
    private readonly IHttpClientFactory _httpClientFactory;

    public CloudflareConnectionType(
        ConnectionTypeInfrastructure infrastructure,
        IHttpClientFactory httpClientFactory)
        : base(infrastructure)
    {
        _httpClientFactory = httpClientFactory;
    }

    public override async Task<ConnectionValidationResult> ValidateAsync(
        object? settings,
        CancellationToken cancellationToken)
    {
        if (settings is not CloudflareConnectionSettings typed)
            return ConnectionValidationResult.Failure("Cloudflare connection settings are missing.");

        if (string.IsNullOrWhiteSpace(typed.ApiToken))
            return ConnectionValidationResult.Failure("An API token is required.");

        if (string.IsNullOrWhiteSpace(typed.ZoneId))
            return ConnectionValidationResult.Failure("A Zone ID is required.");

        try
        {
            using var client = _httpClientFactory.CreateClient();
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                "https://api.cloudflare.com/client/v4/user/tokens/verify");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", typed.ApiToken);

            using var response = await client.SendAsync(request, cancellationToken);

            return response.IsSuccessStatusCode
                ? ConnectionValidationResult.Success("Successfully connected to Cloudflare.")
                : ConnectionValidationResult.Failure(
                    $"Cloudflare rejected the API token: {response.ReasonPhrase}");
        }
        catch (Exception ex)
        {
            return ConnectionValidationResult.Failure(
                "Could not connect to Cloudflare.",
                [ex.Message]);
        }
    }
}
