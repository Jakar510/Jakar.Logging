// Jakar.Extensions :: Jakar.Logging.Tests
// 11/15/2026

using System.Diagnostics;
using System.IO;
using System.Linq;
using Serilog.Core;
using Serilog.Events;
using Serilog.Parsing;



namespace Jakar.Extensions.Tests;


[TestFixture]
public sealed class RuntimeProperties_Tests
{
    private static LogEvent Event( LogEventLevel level = LogEventLevel.Information ) => new(DateTimeOffset.UtcNow, level, null, new MessageTemplate([new TextToken("test")]), []);
    private static string[] Names( LogEventProperty property ) => ((StructureValue)property.Value).Properties.Select(static x => x.Name).ToArray();


    [Test] public void Thread_HasTheThreadInformationShape()
    {
        LogEventProperty property = RuntimeProperties.Thread();

        Assert.That(property.Name,    Is.EqualTo("ThreadInformation"));
        Assert.That(Names(property), Is.EqualTo(new[] { "Name", "ManagedThreadID", "CurrentCulture", "CurrentUICulture" }));
    }


    [Test] public void Gc_NestsTheMemoryInformation()
    {
        LogEventProperty property = RuntimeProperties.Gc();

        Assert.That(property.Name,    Is.EqualTo("GcInfo"));
        Assert.That(Names(property), Is.EqualTo(new[] { "TotalMemory", "TotalPauseDuration", "TotalAllocatedBytes", "AllocatedBytesForCurrentThread", "MemoryInfo" }));
        Assert.That(((StructureValue)property.Value).Properties.Single(static x => x.Name == "MemoryInfo").Value, Is.InstanceOf<StructureValue>());
    }


    [Test] public void ActivityEnricher_AddsRuntimeStateOnWarnings_AndTheSpan()
    {
        AppLoggerOptions options  = new();
        ActivityEnricher enricher = new(options);

        LogEvent info = Event();
        enricher.Enrich(info, null!);
        Assert.That(info.Properties.ContainsKey("GcInfo"), Is.False, "only warnings and above");

        using Activity activity = new Activity("op").SetIdFormat(ActivityIdFormat.W3C).Start();
        LogEvent       warning  = Event(LogEventLevel.Warning);
        enricher.Enrich(warning, null!);

        Assert.That(warning.Properties.Keys, Is.SupersetOf(new[] { "GcInfo", "ThreadInformation", ActivityEnricher.SPAN_ID, nameof(Activity.OperationName) }));
    }


    [Test] public void AppLoggerOptions_PathsFollowTheDirectories()
    {
        string root = Path.Combine(Path.GetTempPath(), $"jakar-{Guid.NewGuid():N}");

        try
        {
            AppLoggerOptions options = new() { CacheDirectory = Path.Combine(root, "cache"), AppDataDirectory = Path.Combine(root, "data") };

            Assert.That(options.LogsFile,       Is.EqualTo(Path.Join(root, "cache", AppLoggerOptions.LOGS_DIRECTORY, AppLoggerOptions.LOGS_FILE)));
            Assert.That(options.ScreenshotFile, Is.EqualTo(Path.Join(root, "cache", AppLoggerOptions.SCREEN_SHOT_FILE)));
            Assert.That(options.SeqBuffer,      Is.EqualTo(Path.Join(root, "data", AppLoggerOptions.SEQ_BUFFER_DIRECTORY)));
            Assert.That(ScreenShot.File,        Is.EqualTo(options.ScreenshotFile), "the latest options are Current");
        }
        finally
        {
            if ( Directory.Exists(root) ) { Directory.Delete(root, true); }
        }
    }


    [Test] public void ScreenShot_ReferenceID_IsStable()
    {
        byte[] data = [1, 2, 3];
        Assert.That(new ScreenShot(data).ReferenceID, Is.EqualTo(ScreenShot.CreateID(data)));
    }
}