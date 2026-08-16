namespace Umbraco.Community.Automate.Extensions.Actions;

public sealed class GetPublishedContentUrlsOutput
{
    public Guid ContentKey { get; set; }
    public string Culture { get; set; } = string.Empty;
    public string[] Urls { get; set; } = [];
}
