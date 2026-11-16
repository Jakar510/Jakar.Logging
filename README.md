# Jakar.Logging

Serilog and Microsoft.Extensions.Logging support: app logger options, OpenTelemetry activity enrichment, thread and GC properties, screenshots and scopes.

```sh
dotnet add package Jakar.Logging
```

Split out of [Jakar.Extensions](https://github.com/Jakar510/Jakar.Extensions) in 10.12.0. Since 10.2.0 it doesn't depend on Jakar.Extensions or Jakar.Files: only Serilog (and its sinks), Microsoft.Extensions.Logging.Abstractions/Options and System.IO.Hashing. The types are in the `Jakar.Extensions` namespace. For the Jakar.Extensions integration (`AppInformation`, `DeviceInformation`, `TelemetrySource`), add [Jakar.Logging.Serilog](https://www.nuget.org/packages/Jakar.Logging.Serilog/).

## What's in it

- `AppLoggerOptions`: Serilog configuration for an app: debug and rolling-file sinks, `Enrichers`, and the folders (`AppDataDirectory`, `CacheDirectory`, `LogsDirectory`, `SeqBuffer`) and files (`LogsFile`, `ScreenshotFile`) as paths. `CreateLogger(configure)` builds the logger.
- `ActivityEnricher`: adds the current `Activity` (trace, span, parent, tags, baggage, events), plus the thread and GC state on warnings and above.
- `RuntimeProperties`: `Thread()` / `Gc()` as Serilog properties (the same shape as Jakar.Extensions' `ThreadInformation` / `GcInfo`).
- `Enricher`: Serilog property helpers and `Activity` span/trace/parent IDs.
- `ScreenShot`: attaches a screenshot reference to log events.
- `ScopeProvider`, `NullScope`: `IExternalScopeProvider` implementations.

## Migrating from 10.1.0

- Add [Jakar.Logging.Serilog](https://www.nuget.org/packages/Jakar.Logging.Serilog/) if you use `LogEventProperties` (`AppInformation.GetProperty()`, `DeviceInformation.ToProperty()`, ...), `DeviceInformationEnricher`, `OpenTelemetryActivityEnricher` or `CreateLogger(TelemetrySource)`; they moved there.
- `AppLoggerOptions` is no longer a `BaseClass` or disposable, and has no `DeviceInfo` (pass the device to Jakar.Logging.Serilog' `CreateLogger(source, device)`). `Paths` (a Jakar.Files `FilePaths`) became `LogsDirectory`, `LogsFile` and `ScreenshotFile`; the directories are strings. `ConfigureLogger` no longer takes a `TelemetrySource`.
- `IOpenTelemetryActivityEnricher` is gone; `ActivityEnricher` takes the `AppLoggerOptions`.
- `ScreenShot.File` is a path; `Enricher` lost its `AppVersion` / `GcGenerationInformation` overloads (they're on `LogEventProperties` in Jakar.Logging.Serilog).

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