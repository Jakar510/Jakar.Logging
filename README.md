# Jakar.Logging

Serilog and Microsoft.Extensions.Logging support for the Jakar packages: app logger options, OpenTelemetry activity enrichment, telemetry sources and spans, app/device/GC/thread properties, `AppVersion` and diagnostics.

```sh
dotnet add package Jakar.Logging
```

Split out of [Jakar.Extensions](https://github.com/Jakar510/Jakar.Extensions) in 10.12.0. It depends on [Jakar.Core](https://github.com/Jakar510/Jakar.Core) (and through it Jakar.Json) and Serilog; Jakar.Extensions 10.13 depends on it and type-forwards the types that moved here. The types are in the `Jakar.Extensions` namespace.

## What's in it

- `AppLoggerOptions`: Serilog configuration for an app: debug and rolling-file sinks, `Enrichers`, and the folders (`AppDataDirectory`, `CacheDirectory`, `LogsDirectory`, `SeqBuffer`) and files (`LogsFile`, `ScreenshotFile`) as paths. `CreateLogger(configure)` builds the logger; `CreateLogger(TelemetrySource, DeviceInformation?)` adds the app and device properties.
- `ActivityEnricher`: the current `Activity` (trace, span, parent, tags, baggage, events), plus thread and GC state on warnings and above. `OpenTelemetryActivityEnricher` adds the app/device properties of a `TelemetrySource` on top (`TelemetrySourceEnricher`).
- `TelemetrySource`, `TelemetrySpan`, `AppVersion`, `AppInformation`, `DeviceInformation`, `GcInfo`, `StopWatch`, `SelfLogger`, `ExceptionDetails`.
- `LogEventProperties`: `GetProperty()` / `ToProperty()` for `AppInformation`, `GcInfo` and `DeviceInformation`; `DeviceInformationEnricher` (`device.ToEnricher()`).
- `RuntimeProperties`: `Thread()` / `Gc()` read from the runtime; `Enricher`: Serilog property helpers; `ScreenShot`; `ScopeProvider`, `NullScope`.

## Migrating

- From 10.2.0: Jakar.Logging.Serilog is retired; its types are here, so drop that package. `ThreadInformation.GetProperty()` is gone: use `RuntimeProperties.Thread()`. Use Jakar.Extensions 10.13.0 or later.
- From 10.1.0: `AppLoggerOptions` is no longer a `BaseClass` or disposable and has no `DeviceInfo` (pass the device to `CreateLogger(source, device)`); `Paths` became `LogsDirectory`, `LogsFile` and `ScreenshotFile`; `IOpenTelemetryActivityEnricher` is gone; `ScreenShot.File` is a path.

## Versioning

Versions are `<.NET major>.<library major>.<library minor>`: a breaking change bumps the middle number (`10.1.0` → `10.2.0`), anything else the last one.

## Repository

| Path | What it is |
|---|---|
| `Jakar.Logging/` | The Jakar.Logging package (Serilog only). |
| `Jakar.Logging.Tests/` | NUnit tests. |
| `.github/workflows/ci.yml` | Build and test on Ubuntu and Windows, for every push and pull request. |
| `.github/workflows/publish.yml` | On `master`: CI, then pack both packages and push them to nuget.org (Trusted Publishing). |