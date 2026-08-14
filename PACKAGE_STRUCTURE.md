# Package Structure

All Umbraco Community packages must follow the same repository layout.

## Source Projects

```text
src/
├── Umbraco.Community.{PackageName}
├── Umbraco.Community.{PackageName}.Assets
└── Umbraco.Community.{PackageName}.Client
```

### Umbraco.Community.{PackageName}

Main package project.

Contains:

* Composers
* Notifications
* Extensions
* Services
* Configuration
* Public APIs

### Umbraco.Community.{PackageName}.Assets

Contains:

* App_Plugins files
* Static assets
* Manifest files
* Embedded resources

Should not contain business logic.

### Umbraco.Community.{PackageName}.Client

Contains:

* TypeScript
* Lit components
* Backoffice extensions
* Client-side build output

Should compile into the Assets project output.

## Example Sites

```text
examples/
└── Umbraco.Community.{PackageName}.Site.{UmbracoMajorVersion}.x
```

Examples:

```text
examples/
├── Umbraco.Community.HealthChecks.Site.16.x
├── Umbraco.Community.HealthChecks.Site.17.x
```

Rules:

* One example site per supported major Umbraco version.
* Example sites reference the local package projects.
* Example sites are never packed into NuGet packages.

## Tests

```text
tests/
└── Umbraco.Community.{PackageName}.Tests
```

Contains:

* Unit tests
* Integration tests
* Snapshot tests where applicable

## Naming

Repository:

```text
Umbraco.Community.{PackageName}
```

PackageId:

```text
Umbraco.Community.{PackageName}
```

Root namespace:

```text
Umbraco.Community.{PackageName}
```

Assembly names:

```text
Umbraco.Community.{PackageName}
Umbraco.Community.{PackageName}.Assets
Umbraco.Community.{PackageName}.Client
```
