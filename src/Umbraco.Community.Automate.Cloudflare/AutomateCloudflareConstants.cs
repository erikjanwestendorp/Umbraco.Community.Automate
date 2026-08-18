namespace Umbraco.Community.Automate.Cloudflare;

internal static class AutomateCloudflareConstants
{
    internal static class Actions
    {
        public static class PurgeUrls
        {
            public const string Alias = "community.cloudflare.purgeUrls";
            public const string Name = "Purge Cloudflare URLs";
            public const string Description = "Purge one or more URLs from the Cloudflare cache.";
        }
    }
    internal static class ConnectionTypes
    {
        internal static class Cloudflare
        {
            public const string Alias = "community.cloudflare";
            public const string Name = "Cloudflare";
            public const string Description = "Connect to Cloudflare to manage cached content.";
        }
    }

    internal static class Groups
    {
        public const string Cloudflare = "Cloudflare";
    }

    internal static class Icons
    {
        public const string Cloud = "icon-cloud";
    }
}
