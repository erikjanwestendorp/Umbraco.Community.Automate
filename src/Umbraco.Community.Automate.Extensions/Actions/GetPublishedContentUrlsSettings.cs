using Umbraco.Automate.Core.Settings;

namespace Umbraco.Community.Automate.Extensions.Actions;

public sealed class GetPublishedContentUrlsSettings
{
    [Field(Label = "Content key", Description = "The key of the published Umbraco content item.", SupportsBindings = true)]
    public string ContentKey { get; set; } = string.Empty;

    [Field(Label = "Culture", Description = "The culture to resolve, for example nl-NL or en.", SupportsBindings = true)]
    public string Culture { get; set; } = string.Empty;
}