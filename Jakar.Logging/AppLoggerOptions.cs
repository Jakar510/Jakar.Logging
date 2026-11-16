// Jakar.Extensions :: Jakar.Extensions
// 08/12/2025  16:43

using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Compact;
using Serilog.Sinks.Async;
using Serilog.Sinks.File;
using Serilog.Sinks.SystemConsole.Themes;



namespace Jakar.Extensions;


/// <summary> Serilog setup for an app: debug and rolling-file sinks, the <see cref="ActivityEnricher"/>, and the folders the logs and screenshots go to. </summary>
/// <remarks> Jakar.Logging.Serilog adds the Jakar.Extensions integration: <c> CreateLogger(TelemetrySource) </c> and the app/device properties. </remarks>

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class AppLoggerOptions : IOptions<AppLoggerOptions>
{
    public const string APP_DATA_DIRECTORY                  = "AppData";
    public const string CACHE_DIRECTORY                     = "Cache";
    public const string DEBUG_DEFAULT_DEBUG_OUTPUT_TEMPLATE = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";
    public const string LOGS_DIRECTORY                      = "Logs";
    public const string LOGS_FILE                           = "App.Logs";
    public const string SCREEN_SHOT_FILE                    = "ScreenShot.png";
    public const string SEQ_BUFFER_DIRECTORY                = "SeqBuffer";
    public static readonly LoggingLevelSwitch LoggingLevel = new(LogEventLevel.Verbose);
    private                string?            __logsDirectory;
    private                string?            __seqBuffer;


    public static AppLoggerOptions             Current       { get; private set; } = new();
    public        Action<LoggerConfiguration>? AddNativeLogs { get; set; }
    public        string?                      AppBuild      { get; set; }

    /// <summary> Defaults to <c> {current directory}/AppData </c>; created when set. </summary>
    public string AppDataDirectory
    {
        get => field ??= Path.Join(Environment.CurrentDirectory, APP_DATA_DIRECTORY);
        set
        {
            field = value;
            __seqBuffer        = null;
            Directory.CreateDirectory(value);
        }
    }

    /// <summary> Defaults to <c> {current directory}/Cache </c>; created when set. </summary>
    public string CacheDirectory
    {
        get => field ??= Path.Join(Environment.CurrentDirectory, CACHE_DIRECTORY);
        set
        {
            field = value;
            __logsDirectory  = null;
            Directory.CreateDirectory(value);
        }
    }

    public ILogEventEnricher[]        Enrichers { get; set; } = [];
    public ITextFormatter             Formatter { get; set; } = new CompactJsonFormatter();
    public FileLifecycleHooks?        Hooks     { get; set; }
    public IAsyncLogEventSinkMonitor? Monitor   { get; set; }

    /// <summary> Defaults to <c> {<see cref="CacheDirectory"/>}/Logs </c>; created when set. </summary>
    public string LogsDirectory
    {
        get => __logsDirectory ??= Path.Join(CacheDirectory, LOGS_DIRECTORY);
        set
        {
            __logsDirectory = value;
            Directory.CreateDirectory(value);
        }
    }

    /// <summary> The rolling log file: <c> {<see cref="LogsDirectory"/>}/App.Logs </c>. </summary>
    public string      LogsFile       => Path.Join(LogsDirectory, LOGS_FILE);
    public ScreenShot? ScreenShot     { get; set; }
    public string      ScreenshotFile => Path.Join(CacheDirectory, SCREEN_SHOT_FILE);

    /// <summary> Defaults to <c> {<see cref="AppDataDirectory"/>}/SeqBuffer </c>; created when set. </summary>
    public string SeqBuffer
    {
        get => __seqBuffer ??= Path.Join(AppDataDirectory, SEQ_BUFFER_DIRECTORY);
        set
        {
            __seqBuffer = value;
            Directory.CreateDirectory(value);
        }
    }

    public         TakeScreenShotAsync          TakeScreenShot        { get; set; } = ScreenShot.Empty;
    public virtual bool                         TakeScreenshotOnError { get; set; }
    public         ConsoleTheme?                Theme                 { get; set; }
    AppLoggerOptions IOptions<AppLoggerOptions>.Value                 => this;


    public AppLoggerOptions() => Current = this;


    /// <summary> Builds a logger from <see cref="ConfigureLogger"/>, then <see cref="AddNativeLogs"/>, then <paramref name="configure"/>. </summary>
    public Logger CreateLogger( Action<LoggerConfiguration>? configure = null )
    {
        LoggerConfiguration builder = new();

        ConfigureLogger(builder, builder.MinimumLevel, builder.WriteTo, builder.AuditTo, builder.Enrich, builder.Filter, builder.Destructure);

        AddNativeLogs?.Invoke(builder);
        configure?.Invoke(builder);

        return builder.CreateLogger();
    }


    protected virtual ILogEventEnricher CreateEnricher() => new ActivityEnricher(this);
    public virtual void ConfigureLogger( in LoggerConfiguration configuration, in LoggerMinimumLevelConfiguration minimumLevel, in LoggerSinkConfiguration sinkTo, in LoggerAuditSinkConfiguration auditTo, in LoggerEnrichmentConfiguration enrichment, in LoggerFilterConfiguration filter, in LoggerDestructuringConfiguration destructure )
    {
        minimumLevel.Verbose();
        minimumLevel.Override(nameof(Microsoft), LogEventLevel.Warning);
        minimumLevel.Override(nameof(System),    LogEventLevel.Warning);

        destructure.ToMaximumDepth(1000);
        destructure.ToMaximumStringLength(int.MaxValue);
        destructure.ToMaximumCollectionCount(99999);

        enrichment.FromLogContext();
        enrichment.With(CreateEnricher());

        SinkToDebug(in sinkTo);
        SinkToFile(in sinkTo);
    }
    public static void SinkToDebug( in LoggerSinkConfiguration sinkTo ) => sinkTo.Debug(LogEventLevel.Verbose, DEBUG_DEFAULT_DEBUG_OUTPUT_TEMPLATE, CultureInfo.InvariantCulture);
    public        void SinkToFile( in  LoggerSinkConfiguration sinkTo ) => sinkTo.File(Formatter, LogsFile, flushToDiskInterval: TimeSpan.FromSeconds(2), rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true, encoding: Encoding.Default, hooks: Hooks, retainedFileTimeLimit: TimeSpan.FromDays(15));
}



public delegate void ConfigureLogger( in AppLoggerOptions options, in LoggerMinimumLevelConfiguration minimumLevel, in LoggerSinkConfiguration sinkTo, in LoggerAuditSinkConfiguration auditTo, in LoggerEnrichmentConfiguration enrichment, in LoggerFilterConfiguration filter, in LoggerDestructuringConfiguration destructure );
