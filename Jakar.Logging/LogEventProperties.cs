// Jakar.Extensions :: Jakar.Logging
// 11/15/2026

using Serilog.Core;
using Serilog.Events;



namespace Jakar.Extensions;


/// <summary> Serilog properties for the Jakar.Extensions models: they live here, so the models in Jakar.Extensions don't depend on Serilog. </summary>
public static class LogEventProperties
{
    extension( AppInformation self )
    {
        public StructureValue   GetStructureValue() => new([Enricher.GetProperty(self.Version, nameof(self.Version)), Enricher.GetProperty(self.AppID, nameof(self.AppID)), Enricher.GetProperty(self.AppName, nameof(self.AppName)), Enricher.GetProperty(self.PackageName, nameof(self.PackageName))]);
        public LogEventProperty GetProperty()       => new(nameof(AppInformation), self.GetStructureValue());
    }



    extension( ThreadInformation self )
    {
        public StructureValue   GetStructureValue() => new([Enricher.GetProperty(self.Name, nameof(self.Name)), Enricher.GetProperty(self.ManagedThreadID, nameof(self.ManagedThreadID)), Enricher.GetProperty(self.CurrentCulture.DisplayName, nameof(self.CurrentCulture)), Enricher.GetProperty(self.CurrentUICulture.DisplayName, nameof(self.CurrentUICulture))]);
        public LogEventProperty GetProperty()       => new(nameof(ThreadInformation), self.GetStructureValue());
    }



    extension( GcInfo self )
    {
        public LogEventProperty GetProperty() => new(nameof(GcInfo),
                                                     new StructureValue([
                                                                            Enricher.GetProperty(self.TotalMemory,                    nameof(self.TotalMemory)),
                                                                            Enricher.GetProperty(self.TotalPauseDuration,             nameof(self.TotalPauseDuration)),
                                                                            Enricher.GetProperty(self.TotalAllocatedBytes,            nameof(self.TotalAllocatedBytes)),
                                                                            Enricher.GetProperty(self.AllocatedBytesForCurrentThread, nameof(self.AllocatedBytesForCurrentThread)),
                                                                            self.MemoryInfo.GetProperty(nameof(self.MemoryInfo))
                                                                        ]));
    }



    extension( GcMemoryInformation self )
    {
        public StructureValue GetProperty() =>
            new([
                    Enricher.GetProperty(self.Compacted,                    nameof(self.Compacted)),
                    Enricher.GetProperty(self.HighMemoryLoadThresholdBytes, nameof(self.HighMemoryLoadThresholdBytes)),
                    Enricher.GetProperty(self.MemoryLoadBytes,              nameof(self.MemoryLoadBytes)),
                    Enricher.GetProperty(self.TotalAvailableMemoryBytes,    nameof(self.TotalAvailableMemoryBytes)),
                    Enricher.GetProperty(self.HeapSizeBytes,                nameof(self.HeapSizeBytes)),
                    Enricher.GetProperty(self.FragmentedBytes,              nameof(self.FragmentedBytes)),
                    Enricher.GetProperty(self.Generation,                   nameof(self.Generation)),
                    Enricher.GetProperty(self.Concurrent,                   nameof(self.Concurrent)),
                    Enricher.GetProperty(self.TotalCommittedBytes,          nameof(self.TotalCommittedBytes)),
                    Enricher.GetProperty(self.PromotedBytes,                nameof(self.PromotedBytes)),
                    Enricher.GetProperty(self.PinnedObjectsCount,           nameof(self.PinnedObjectsCount)),
                    Enricher.GetProperty(self.FinalizationPendingCount,     nameof(self.FinalizationPendingCount)),
                    Enricher.GetProperty(self.PauseTimePercentage,          nameof(self.PauseTimePercentage)),
                    Enricher.GetProperty(self.GenerationInfo,               nameof(self.GenerationInfo)),
                    Enricher.GetProperty(self.PauseDurations,               nameof(self.PauseDurations))
                ]);
        public LogEventProperty GetProperty( string name ) => new(name, self.GetProperty());
    }



    extension( GcGenerationInformation self )
    {
        public StructureValue   GetProperty()              => new([Enricher.GetProperty(self.SizeBeforeBytes, nameof(self.SizeBeforeBytes)), Enricher.GetProperty(self.FragmentationBeforeBytes, nameof(self.FragmentationBeforeBytes)), Enricher.GetProperty(self.SizeAfterBytes, nameof(self.SizeAfterBytes)), Enricher.GetProperty(self.FragmentationAfterBytes, nameof(self.FragmentationAfterBytes))]);
        public LogEventProperty GetProperty( string name ) => new(name, self.GetProperty());
    }



    extension( DeviceInformation self )
    {
        public LogEventProperty ToProperty() => new(nameof(DeviceInformation),
                                                    new StructureValue([
                                                                           Enricher.GetProperty(self.Category,     nameof(self.Category)),
                                                                           Enricher.GetProperty(self.DeviceID,     nameof(self.DeviceID)),
                                                                           Enricher.GetProperty(self.DeviceName,   nameof(self.DeviceName)),
                                                                           Enricher.GetProperty(self.Manufacturer, nameof(self.Manufacturer)),
                                                                           Enricher.GetProperty(self.Model,        nameof(self.Model)),
                                                                           Enricher.GetProperty(self.OsVersion,    nameof(self.OsVersion)),
                                                                           Enricher.GetProperty(self.PackageName,  nameof(self.PackageName)),
                                                                           Enricher.GetProperty(self.Platform,     nameof(self.Platform)),
                                                                           Enricher.GetProperty(self.Type,         nameof(self.Type)),
                                                                           Enricher.GetProperty(self.Version,      nameof(self.Version))
                                                                       ]));

        /// <summary> A Serilog enricher that adds this device's properties to every event. </summary>
        public ILogEventEnricher ToEnricher() => new DeviceInformationEnricher(self);
    }
}



/// <summary> Adds a <see cref="DeviceInformation"/>'s properties to every event (read when each event is enriched, so later changes to the device show up). </summary>
public sealed class DeviceInformationEnricher( DeviceInformation device ) : ILogEventEnricher
{
    public void Enrich( LogEvent log, ILogEventPropertyFactory propertyFactory ) => log.AddPropertyIfAbsent(device.ToProperty());
}