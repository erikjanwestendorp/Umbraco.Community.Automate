using Microsoft.Extensions.DependencyInjection;
using Umbraco.Automate.Core.Actions;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Community.Automate.Extensions.Actions;

[Action(
    AutomateExtensionsConstants.Actions.GetPublishedContentUrls.Alias,
    AutomateExtensionsConstants.Actions.GetPublishedContentUrls.Name,
    Description = AutomateExtensionsConstants.Actions.GetPublishedContentUrls.Description,
    Group = AutomateExtensionsConstants.Groups.Community,
    Icon = AutomateExtensionsConstants.Icons.Link)]
public class GetPublishedContentUrlsAction(
    ActionInfrastructure infrastructure,
    IServiceScopeFactory serviceScopeFactory) : ActionBase<GetPublishedContentUrlsSettings, GetPublishedContentUrlsOutput>(infrastructure)
{
    public override Task<ActionResult> ExecuteAsync(
        ActionContext context,
        CancellationToken cancellationToken)
    {
        var settings =
            context.GetSettings<GetPublishedContentUrlsSettings>();

        if (!Guid.TryParse(settings.ContentKey, out var contentKey))
        {
            return Task.FromResult(
                ActionResult.Failed(
                    new ArgumentException(
                        "A valid content key is required.")));
        }

        var culture = settings.Culture.Trim();

        if (string.IsNullOrWhiteSpace(culture))
        {
            return Task.FromResult(
                ActionResult.Failed(
                    new ArgumentException(
                        "A culture is required.")));
        }

        using var scope = serviceScopeFactory.CreateScope();

        var publishedContentQuery =
            scope.ServiceProvider
                .GetRequiredService<IPublishedContentQuery>();

        var publishedUrlProvider =
            scope.ServiceProvider
                .GetRequiredService<IPublishedUrlProvider>();

        var umbracoContextFactory =
            scope.ServiceProvider
                .GetRequiredService<IUmbracoContextFactory>();

        using var umbracoContextReference =
            umbracoContextFactory.EnsureUmbracoContext();

        var content =
            publishedContentQuery.Content(contentKey);

        if (content is null)
        {
            return Task.FromResult(
                ActionResult.Failed(
                    new InvalidOperationException(
                        $"Published content with key '{contentKey}' could not be found.")));
        }

        if (content.Cultures.Any() &&
            !content.Cultures.ContainsKey(culture))
        {
            return Task.FromResult(
                ActionResult.Failed(
                    new InvalidOperationException(
                        $"Content '{contentKey}' is not published for culture '{culture}'.")));
        }

        var urls =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

        var primaryUrl = content.Url(
            publishedUrlProvider,
            culture: culture,
            mode: UrlMode.Absolute);

        if (TryAddAbsoluteUrl(urls, primaryUrl) &&
            Uri.TryCreate(
                primaryUrl,
                UriKind.Absolute,
                out var current))
        {
            foreach (var otherUrl in
                     publishedUrlProvider.GetOtherUrls(
                         content.Id,
                         current))
            {
                if (CultureMatches(
                        otherUrl.Culture,
                        culture))
                {
                    TryAddAbsoluteUrl(
                        urls,
                        otherUrl.Url?.ToString());
                }
            }
        }

        return Task.FromResult(
            Success(
                new GetPublishedContentUrlsOutput
                {
                    ContentKey = content.Key,
                    Culture = culture,
                    Urls = urls.ToArray()
                }));
    }

    private static bool CultureMatches(string? urlCulture, string requestedCulture)
    => string.IsNullOrWhiteSpace(urlCulture) ||
       string.Equals(urlCulture, requestedCulture, StringComparison.OrdinalIgnoreCase);

    private static bool TryAddAbsoluteUrl(ISet<string> urls, string? value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value == "#" ||
            !Uri.TryCreate(value, UriKind.Absolute, out var uri))
        {
            return false;
        }

        urls.Add(uri.ToString());
        return true;
    }
}
