# Jakar.Logging.Serilog

Jakar.Extensions integration for Jakar.Logging: Serilog properties for AppInformation, ThreadInformation, GcInfo and DeviceInformation, and app/device enrichment from a TelemetrySource.

```sh
dotnet add package Jakar.Logging.Serilog
```

It depends on [Jakar.Extensions](https://www.nuget.org/packages/Jakar.Extensions/) and [Jakar.Logging](https://www.nuget.org/packages/Jakar.Logging/); the types are in the `Jakar.Extensions` namespace.

## What's in it

Jakar.Logging has no dependency on Jakar.Extensions. This package adds the Jakar.Extensions side:

| Type | What it does |
|---|---|
| `LogEventProperties` | Serilog properties for `AppInformation`, `ThreadInformation`, `GcInfo` / `GcMemoryInformation` / `GcGenerationInformation` and `DeviceInformation` (`GetProperty()` / `ToProperty()`), plus `GetProperty(AppVersion, name)`. |
| `DeviceInformationEnricher` | `device.ToEnricher()`: the device's properties on every event, read when each event is enriched. |
| `TelemetrySourceEnricher` | The app (`TelemetrySource.Info`) and device properties on every event. |
| `OpenTelemetryActivityEnricher` | Jakar.Logging's `ActivityEnricher` plus `TelemetrySourceEnricher` (what Jakar.Logging 10.1.0's enricher did). |
| `AppLoggerOptions.CreateLogger(TelemetrySource, DeviceInformation?)`, `AppLoggerOptions.Info` | Extension members on Jakar.Logging's `AppLoggerOptions`. |

## Migrating from Jakar.Logging 10.1.0

Add this package next to Jakar.Logging 10.2.0. `new OpenTelemetryActivityEnricher(loggerOptions, source)` and `loggerOptions.CreateLogger(source)` keep compiling; the device now comes from the optional `device` argument (and the source) instead of `AppLoggerOptions.DeviceInfo`.

## Versioning

Versions are `<.NET major>.<library major>.<library minor>`: a breaking change bumps the middle number (`10.1.0` → `10.2.0`), anything else the last one.

## Repository

| Path | What it is |
|---|---|
| `Jakar.Logging/` | The Jakar.Logging package (Serilog only). |
| `Jakar.Logging.Tests/` | NUnit tests. |
| `Jakar.Logging.Serilog/` | The Jakar.Logging.Serilog package: the Jakar.Extensions integration (see its README). |
| `Jakar.Logging.Serilog.Tests/` | NUnit tests. |
| `.github/workflows/ci.yml` | Build and test on Ubuntu and Windows, for every push and pull request. |
| `.github/workflows/publish.yml` | On `master`: CI, then pack both packages and push them to nuget.org (Trusted Publishing). |