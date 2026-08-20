namespace Umbraco.Community.Automate.Extensions;

internal static class AutomateExtensionsConstants
{
    internal static class Actions
    {
        internal static class GetPublishedContentUrls
        {
            public const string Alias = "community.extensions.getPublishedContentUrls";
            public const string Name = "Get Published Content URL's";
            public const string Description = "Gets all absolute URLs for a published content item for a specific culture, including alternate hostnames.";
        }
    }

    internal static class Triggers
    {
        internal static class HealthCheckCompleted
        {
            public const string Alias = "community.extensions.healthCheckCompleted";
            public const string Name = "Health Check Completed";
            public const string Description = "Fires when an Umbraco health check execution has completed.";
        }
    }

    internal static class Groups
    {
        public const string Community = "Community";
    }

    internal static class Icons
    {
        public const string Link = "icon-link";
        public const string Health = "icon-health";
    }
}
