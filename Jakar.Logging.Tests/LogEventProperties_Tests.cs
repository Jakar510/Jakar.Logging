// Jakar.Extensions :: Jakar.Logging.Tests
// 11/15/2026

using System.Linq;
using Serilog.Events;
using Serilog.Parsing;



namespace Jakar.Extensions.Tests;


/// <summary> The Serilog properties for the Jakar.Extensions models, which moved here from the models themselves. </summary>
[TestFixture]
public sealed class LogEventProperties_Tests
{
    private static LogEvent Event() => new(DateTimeOffset.UtcNow, LogEventLevel.Information, null, new MessageTemplate([new TextToken("test")]), []);


    [Test] public void AppInformation_IsAStructureNamedAfterTheType()
    {
        AppInformation   info     = new(new AppVersion(1, 2, 3), Guid.Empty, "app", "com.app");
        LogEventProperty property = info.GetProperty();

        Assert.That(property.Name, Is.EqualTo(nameof(AppInformation)));
        Assert.That(((StructureValue)property.Value).Properties.Select(static x => x.Name), Is.EqualTo(new[] { "Version", "AppID", "AppName", "PackageName" }));
    }


    [Test] public void GcInfo_NestsTheMemoryInformation()
    {
        LogEventProperty property = GcInfo.Create().GetProperty();
        StructureValue   value    = (StructureValue)property.Value;

        Assert.That(property.Name,                              Is.EqualTo(nameof(GcInfo)));
        Assert.That(value.Properties.Single(static x => x.Name == nameof(GcInfo.MemoryInfo)).Value, Is.InstanceOf<StructureValue>());
    }


    [Test] public void ThreadInformation_HasTheThreadID()
    {
        ThreadInformation thread = ThreadInformation.Create();
        StructureValue    value  = (StructureValue)thread.GetProperty().Value;

        Assert.That(value.Properties.Single(static x => x.Name == nameof(ThreadInformation.ManagedThreadID)).Value.ToString(), Is.EqualTo(thread.ManagedThreadID.ToString()));
    }


    [Test] public void DeviceInformationEnricher_ReadsTheDeviceWhenEnriching()
    {
        DeviceInformation device = new() { DeviceID = "first" };
        LogEvent          before = Event();
        device.ToEnricher().Enrich(before, null!);

        device.DeviceID = "second";
        LogEvent after = Event();
        device.ToEnricher().Enrich(after, null!);

        Assert.That(DeviceID(before), Is.EqualTo("\"first\""));
        Assert.That(DeviceID(after),  Is.EqualTo("\"second\""), "not cached: a changed device shows up in later events");

        static string DeviceID( LogEvent log ) => ((StructureValue)log.Properties[nameof(DeviceInformation)]).Properties.Single(static x => x.Name == nameof(DeviceInformation.DeviceID)).Value.ToString();
    }
}