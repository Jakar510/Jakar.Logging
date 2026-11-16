// Jakar.Extensions :: Jakar.Logging.Serilog
// 11/15/2026

using Serilog;
using Serilog.Core;
using Serilog.Events;



namespace Jakar.Extensions;


/// <summary> Adds the app (<see cref="TelemetrySource.Info"/>) and device properties to every event: the given <see cref="DeviceInformation"/>, then the source's own device when it has one. </summary>
public sealed class TelemetrySourceEnricher( TelemetrySource source, DeviceInformation? device = null ) : ILogEventEnricher
{
    public readonly LogEventProperty AppInfo = source.Info.GetProperty();


    public void Enrich( LogEvent log, ILogEventPropertyFactory factory )
    {
        ArgumentNullException.ThrowIfNull(log);

        log.AddPropertyIfAbsent(AppInfo);
        if ( device is not null ) { log.AddPropertyIfAbsent(device.ToProperty()); }

        TryAddDeviceInformation(in log, source);
    }


    public static void TryAddDeviceInformation( ref readonly LogEvent log, TelemetrySource source )
    {
        DeviceInformation? device = source.TryGetDeviceInformation();
        if ( device is not null ) { log.AddOrUpdateProperty(device.ToProperty()); }
    }
}



/// <summary> <see cref="ActivityEnricher"/> (spans, thread/GC state, <see cref="AppLoggerOptions.Enrichers"/>) plus <see cref="TelemetrySourceEnricher"/> (app and device properties). </summary>
public sealed class OpenTelemetryActivityEnricher( AppLoggerOptions options, TelemetrySource source, DeviceInformation? device = null ) : ILogEventEnricher
{
    private readonly ActivityEnricher        __activity = new(options);
    private readonly TelemetrySourceEnricher __source   = new(source, device);


    public LogEventProperty AppInfo => __source.AppInfo;


    public void Enrich( LogEvent log, ILogEventPropertyFactory factory )
    {
        __source.Enrich(log, factory);
        __activity.Enrich(log, factory);
    }
}



/// <summary> The Jakar.Extensions side of <see cref="AppLoggerOptions"/>. </summary>
public static class AppLoggerOptionsTelemetry
{
    extension( AppLoggerOptions options )
    {
        /// <summary> The app of <see cref="TelemetrySource.Current"/>, or <see cref="AppInformation.Invalid"/>. </summary>
        public AppInformation Info => TelemetrySource.Current?.Info ?? AppInformation.Invalid;


        /// <summary> <see cref="AppLoggerOptions.CreateLogger"/>, with the app and device properties from <paramref name="source"/> (and <paramref name="device"/>) on every event. </summary>
        public Logger CreateLogger( TelemetrySource source, DeviceInformation? device = null ) => options.CreateLogger(configuration => configuration.Enrich.With(new TelemetrySourceEnricher(source, device)));
    }
}
