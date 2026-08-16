using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Community.Automate.Cloudflare.Client;

namespace Umbraco.Community.Automate.Cloudflare.Compose;

public class CloudflareComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddHttpClient<ICloudflareClient, CloudflareClient>(client =>
        {
            client.BaseAddress = new Uri("https://api.cloudflare.com/client/v4/");
        });
    }
}
