// Jakar.Extensions :: Jakar.Logging.Serilog.Tests
// 11/15/2026

using System.Linq;
using Serilog.Events;
using Serilog.Parsing;



namespace Jakar.Extensions.Tests;


[TestFixture]
public sealed class TelemetrySourceEnricher_Tests
{
    private static LogEvent Event( LogEventLevel level = LogEventLevel.Information ) => new(DateTimeOffset.UtcNow, level, null, new MessageTemplate([new TextToken("test")]), []);


    [Test] public void OpenTelemetryActivityEnricher_AddsAppDeviceAndRuntimeState()
    {
        using TelemetrySource source   = new(new AppVersion(1, 2, 3), Guid.Empty, "app", "com.app");
        DeviceInformation     device   = new() { DeviceID = "device" };
        LogEvent              log      = Event(LogEventLevel.Error);

        new OpenTelemetryActivityEnricher(new AppLoggerOptions(), source, device).Enrich(log, null!);

        Assert.That(log.Properties.Keys, Is.SupersetOf(new[] { nameof(AppInformation), nameof(DeviceInformation), RuntimeProperties.GC_INFO, RuntimeProperties.THREAD_INFORMATION }));
        Assert.That(((StructureValue)log.Properties[nameof(AppInformation)]).Properties.Single(static x => x.Name == nameof(AppInformation.AppName)).Value.ToString(), Is.EqualTo("\"app\""));
    }


    [Test] public void TelemetrySourceEnricher_WithoutADevice_AddsOnlyTheApp()
    {
        using TelemetrySource source = new(new AppVersion(1, 0), Guid.Empty, "app", null);
        LogEvent              log    = Event();

        new TelemetrySourceEnricher(source).Enrich(log, null!);

        Assert.That(log.Properties.Keys, Is.EquivalentTo(new[] { nameof(AppInformation) }));
    }


    [Test] public void AppLoggerOptions_Info_IsTheCurrentSource()
    {
        TelemetrySource? previous = TelemetrySource.Current;

        try
        {
            using TelemetrySource source = new(new AppVersion(2, 0), Guid.Empty, "current", null);
            TelemetrySource.Current = source;
            Assert.That(new AppLoggerOptions().Info.AppName, Is.EqualTo("current"));
        }
        finally { TelemetrySource.Current = previous; }
    }
}