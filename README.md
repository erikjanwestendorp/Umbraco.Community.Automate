# Umbraco Community Automate

A collection of community extensions and integrations for [Umbraco Automate](https://docs.umbraco.com/umbraco-automate).

The goal of this repository is to provide small, reusable building blocks that can be combined in Umbraco Automate workflows.

The repository currently contains two packages:

- **Umbraco.Community.Automate.Extensions** — generic actions and extensions for Umbraco Automate.
- **Umbraco.Community.Automate.Cloudflare** — Cloudflare integration for purging cached URLs.

> [!IMPORTANT]
> These packages are currently in **beta** and are being actively developed.

## Requirements

The current packages target:

- Umbraco 17
- Umbraco Automate

## Packages

### Umbraco.Community.Automate.Extensions

Generic extensions for Umbraco Automate that are not tied to a specific third-party integration.

Install using:

```bash
dotnet add package Umbraco.Community.Automate.Extensions --prerelease
```

#### Get Published Content URLs

Adds a **Get Published Content URLs** action to Automate.

The action resolves all absolute URLs for a published content item for a specific culture.

Inputs:

| Setting | Description |
| --- | --- |
| Content Key | Key of the published Umbraco content item |
| Culture | Culture to resolve, for example `en` or `nl-NL` |

The action returns a collection of URLs:

```text
Urls[]
```

This is particularly useful for multi-language and multi-domain installations.

For example, an Umbraco installation could expose the same content through:

```text
https://www.example-a.com/
https://www.example-a.com/en/

https://www.example-b.com/
https://www.example-b.com/en/
```

For the `en` culture, the action can return:

```text
https://www.example-a.com/en/
https://www.example-b.com/en/
```

The action uses Umbraco's published URL infrastructure to resolve both the primary URL and alternative URLs for the content item.

This makes the action useful outside of the Cloudflare integration as well, for example for:

- Webhooks
- Search indexing
- CDN integrations
- External APIs
- Custom Automate workflows

---

### Umbraco.Community.Automate.Cloudflare

Adds Cloudflare integration to Umbraco Automate.

Install using:

```bash
dotnet add package Umbraco.Community.Automate.Cloudflare --prerelease
```

The package currently provides:

- A **Cloudflare connection type**
- A **Purge Cloudflare URL** action

#### Cloudflare connection

Create a Cloudflare connection from the Automate backoffice.

The connection requires:

| Setting | Description |
| --- | --- |
| Account ID | Your Cloudflare Account ID |
| Zone ID | The Cloudflare Zone ID containing the site |
| API Token | Cloudflare Account API Token |

The connection validates the API token before it can be used by an automation.

The token should have the minimum Cloudflare permissions required to purge the cache for the configured zone.

#### Purge Cloudflare URL

The **Purge Cloudflare URL** action accepts an absolute URL and removes that URL from the Cloudflare cache.

The URL supports Automate bindings, which means it can be provided by a trigger, another action or a loop.

For example:

```text
${ loop.item }
```

## Example: Purge Cloudflare when content is published

One of the reasons these packages were created was to build a fully configurable cache invalidation workflow.

The goal is:

> When content is published, purge every published URL for every affected culture from Cloudflare.

The resulting workflow looks like this:

```text
Content Published
        │
        │ cultures
        ▼
┌──────────────────────┐
│ For Each Culture     │
└──────────┬───────────┘
           │
           ▼
┌──────────────────────────────┐
│ Get Published Content URLs   │
│                              │
│ Content Key = contentKey     │
│ Culture     = loop.item      │
└──────────────┬───────────────┘
               │
               │ URLs
               ▼
       ┌──────────────────┐
       │ For Each URL     │
       └────────┬─────────┘
                │
                ▼
       ┌──────────────────────┐
       │ Purge Cloudflare URL │
       └──────────────────────┘
```

### 1. Content Published trigger

Start the automation using the built-in **Content Published** trigger.

The trigger exposes, among others:

```text
contentKey
contentName
contentTypeKey
contentTypeAlias
cultures
```

### 2. Iterate over cultures

Add a **For Each** step and use:

```text
${ trigger.cultures }
```

as the collection.

### 3. Resolve the URLs

Inside the loop, add **Get Published Content URLs**.

Configure:

```text
Content Key:
${ trigger.contentKey }

Culture:
${ loop.item }
```

The action returns every URL for the current culture.

### 4. Iterate over the URLs

Add another **For Each** using the `Urls` output from the previous action.

A culture can have more than one URL when multiple hostnames or domains are configured.

### 5. Purge Cloudflare

Inside the URL loop, add **Purge Cloudflare URL** and bind the URL to:

```text
${ loop.item }
```

Automate will now purge every affected URL from Cloudflare when the content is published.

## Why separate packages?

The URL resolution functionality deliberately lives outside the Cloudflare package.

Resolving published URLs is an Umbraco concern:

```text
Content Key + Culture
        ↓
Published URLs
```

Purging a URL is a Cloudflare concern:

```text
URL
        ↓
Cloudflare API
```

Keeping these responsibilities separate means that **Get Published Content URLs** can also be reused with other Automate integrations.

It also keeps the Cloudflare action small: it doesn't need to know anything about Umbraco content, cultures or domains.

Automate is responsible for composing the actions into a workflow.

## Status

The packages are currently available as beta releases.

The initial focus is on Umbraco 17 and exploring useful community extensions for Umbraco Automate.

Possible future additions include:

- Additional generic Automate actions
- More Cloudflare cache purge options
- Purging multiple URLs in a single request
- Purge by hostname
- Purge by cache tag
- Purge by prefix
- Improved secret/configuration handling

Feedback, issues and pull requests are very welcome.

## Contributing

Contributions are welcome.

If you find a bug, have an idea for another Automate action, or want to improve an existing integration, feel free to open an issue or pull request.

## License

This project is licensed under the [MIT License](LICENSE).
