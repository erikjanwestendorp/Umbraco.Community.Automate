# Umbraco Community Automate

A collection of community-built extensions and integrations for Umbraco Automate.

This package is a **convenience package** that installs the complete Umbraco Community Automate collection.

It currently includes:

- `Umbraco.Community.Automate.Extensions`
- `Umbraco.Community.Automate.Cloudflare`

## Getting Started

```bash
dotnet add package Umbraco.Community.Automate
```

Installing this package automatically installs the individual extension and integration packages.

## Packages

### Umbraco.Community.Automate.Extensions

Provides generic and reusable actions for Umbraco Automate that are not tied to a specific third-party integration.

This currently includes the **Get Published Content URLs** action, which resolves all published URLs for an Umbraco content item for a specific culture.

### Umbraco.Community.Automate.Cloudflare

Provides a Cloudflare integration for Umbraco Automate, including a Cloudflare connection type and actions for purging cached URLs.

## Installing Packages Individually

The packages can also be installed separately:

```bash
dotnet add package Umbraco.Community.Automate.Extensions
```

```bash
dotnet add package Umbraco.Community.Automate.Cloudflare
```

This allows you to install only the extensions and integrations required by your Automate workflows.

## Documentation

For documentation, examples and source code, visit:

https://github.com/erikjanwestendorp/Umbraco.Community.Automate
