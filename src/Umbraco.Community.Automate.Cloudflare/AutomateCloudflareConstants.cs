namespace Umbraco.Community.Automate.Cloudflare;

internal static class AutomateCloudflareConstants
{
    internal static class Actions
    {
        public static class PurgeUrl
        {
            public const string Alias = "community.cloudflare.purgeUrl";
            public const string Name = "Purge Cloudflare URL";
            public const string Description = "Purge a specific URL from the Cloudflare cache.";
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
