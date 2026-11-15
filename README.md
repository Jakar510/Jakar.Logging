# Jakar.Logging

Serilog and Microsoft.Extensions.Logging support: app logger options, a file logger, OpenTelemetry activity enrichment and Serilog properties for the Jakar.Extensions models.

```sh
dotnet add package Jakar.Logging
```

Split out of [Jakar.Extensions](https://github.com/Jakar510/Jakar.Extensions) in 10.12.0. It depends on [Jakar.Extensions](https://www.nuget.org/packages/Jakar.Extensions/). The types keep the `Jakar.Extensions` namespace, so code that used them from Jakar.Extensions only needs this package reference (and a recompile).

## What's in it

- `AppLoggerOptions`: Serilog configuration for an app (sinks, paths, screenshots, enrichers).
- `FileLoggerProvider` (and `FileLoggerProviderOptions`, `ScopeProvider`, `NullScope`): a rolling-file `ILoggerProvider`.
- `OpenTelemetryActivityEnricher` / `IOpenTelemetryActivityEnricher`: adds the current `Activity` (trace, span, parent, tags, baggage) to log events.
- `Enricher`: Serilog property helpers; `LogEventProperties` adds `GetProperty()` / `ToProperty()` to `AppInformation`, `ThreadInformation`, `GcInfo` and `DeviceInformation`; `DeviceInformationEnricher` enriches events with a device.
- `ScreenShot`: attaches a screenshot reference to log events.

`SelfLogger` stays in Jakar.Extensions.

## Versioning

Versions are `<.NET major>.<library major>.<library minor>`: a breaking change bumps the middle number (`10.1.0` → `10.2.0`), anything else the last one.

## Repository

| Path | What it is |
|---|---|
| `Jakar.Logging/` | The package. |
| `Jakar.Logging.Tests/` | NUnit tests. |
| `.github/workflows/ci.yml` | Build and test on Ubuntu and Windows, for every push and pull request. |
| `.github/workflows/publish.yml` | On `master`: CI, then pack and push to nuget.org (Trusted Publishing). |