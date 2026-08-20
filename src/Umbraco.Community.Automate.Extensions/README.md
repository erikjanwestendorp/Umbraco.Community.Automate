# Umbraco Community Automate Extensions

Generic and reusable actions and extensions for Umbraco Automate.

This package provides Automate functionality that is useful across different workflows and is not tied to a specific third-party integration.

## Getting Started

```bash
dotnet add package Umbraco.Community.Automate.Extensions
```

## Get Published Content URLs

The package currently provides the **Get Published Content URLs** action.

This action resolves all absolute published URLs for an Umbraco content item for a specific culture.

### Inputs

| Setting | Description |
| --- | --- |
| Content Key | The key of the published Umbraco content item. |
| Culture | The culture for which the URLs should be resolved, for example `en` or `nl-NL`. |

### Output

The action returns:

- `ContentKey` - The key of the content item.
- `Culture` - The requested culture.
- `Urls` - All absolute published URLs for the content item and culture.

## Multiple Domains

A single content item can have multiple published URLs when multiple domains or hostnames are configured in Umbraco.

For example:

```text
https://www.example-a.com/
https://www.example-a.com/en/

https://www.example-b.com/
https://www.example-b.com/en/
```

For the `en` culture, the action can resolve:

```text
https://www.example-a.com/en/
https://www.example-b.com/en/
```

This makes the action useful for workflows involving CDN cache invalidation, webhooks, search indexing, external APIs and other Automate integrations.

## Using the Output

The `Urls` output can be used as a binding by subsequent Automate steps.

For example, it can be combined with `Umbraco.Community.Automate.Cloudflare` to purge all published URLs for a content item after publishing.

## Documentation

For documentation, examples and source code, visit:

https://github.com/erikjanwestendorp/Umbraco.Community.Automate
