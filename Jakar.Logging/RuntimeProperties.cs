// Jakar.Extensions :: Jakar.Logging
// 11/15/2026

using Serilog.Events;



namespace Jakar.Extensions;


/// <summary> Thread and GC state as Serilog properties, read from the runtime. They have the same names and shape as Jakar.Extensions' <c> ThreadInformation </c> / <c> GcInfo </c> (see Jakar.Logging.Serilog). </summary>
public static class RuntimeProperties
{
    public const string GC_INFO            = "GcInfo";
    public const string THREAD_INFORMATION = "ThreadInformation";


    /// <summary> <c> ThreadInformation </c>: Name, ManagedThreadID, CurrentCulture, CurrentUICulture of the current thread. </summary>
    public static LogEventProperty Thread() => Thread(System.Threading.Thread.CurrentThread);
    public static LogEventProperty Thread( System.Threading.Thread thread ) => new(THREAD_INFORMATION,
                                                                                    new StructureValue([
                                                                                                           Enricher.GetProperty(thread.Name ?? string.Empty,                nameof(thread.Name)),
                                                                                                           Enricher.GetProperty(thread.ManagedThreadId,                     "ManagedThreadID"),
                                                                                                           Enricher.GetProperty(thread.CurrentCulture.DisplayName,          nameof(thread.CurrentCulture)),
                                                                                                           Enricher.GetProperty(thread.CurrentUICulture.DisplayName,        nameof(thread.CurrentUICulture))
                                                                                                       ]));


    /// <summary> <c> GcInfo </c>: TotalMemory, TotalPauseDuration, TotalAllocatedBytes, AllocatedBytesForCurrentThread and the last GC's MemoryInfo. </summary>
    public static LogEventProperty Gc() => new(GC_INFO,
                                               new StructureValue([
                                                                      Enricher.GetProperty(GC.GetTotalMemory(false),              "TotalMemory"),
                                                                      Enricher.GetProperty(GC.GetTotalPauseDuration(),            "TotalPauseDuration"),
                                                                      Enricher.GetProperty(GC.GetTotalAllocatedBytes(),           "TotalAllocatedBytes"),
                                                                      Enricher.GetProperty(GC.GetAllocatedBytesForCurrentThread(), "AllocatedBytesForCurrentThread"),
                                                                      new LogEventProperty("MemoryInfo", Memory(GC.GetGCMemoryInfo()))
                                                                  ]));


    public static StructureValue Memory( in GCMemoryInfo info )
    {
        ReadOnlySpan<GCGenerationInfo> generations = info.GenerationInfo;
        LogEventPropertyValue[]        generation  = new LogEventPropertyValue[generations.Length];
        for ( int i = 0; i < generations.Length; i++ ) { generation[i] = Generation(in generations[i]); }

        ReadOnlySpan<TimeSpan>  pauses = info.PauseDurations;
        LogEventPropertyValue[] pause  = new LogEventPropertyValue[pauses.Length];
        for ( int i = 0; i < pauses.Length; i++ ) { pause[i] = new ScalarValue(pauses[i]); }

        return new StructureValue([
                                      Enricher.GetProperty(info.Compacted,                    nameof(info.Compacted)),
                                      Enricher.GetProperty(info.HighMemoryLoadThresholdBytes, nameof(info.HighMemoryLoadThresholdBytes)),
                                      Enricher.GetProperty(info.MemoryLoadBytes,              nameof(info.MemoryLoadBytes)),
                                      Enricher.GetProperty(info.TotalAvailableMemoryBytes,    nameof(info.TotalAvailableMemoryBytes)),
                                      Enricher.GetProperty(info.HeapSizeBytes,                nameof(info.HeapSizeBytes)),
                                      Enricher.GetProperty(info.FragmentedBytes,              nameof(info.FragmentedBytes)),
                                      Enricher.GetProperty(info.Generation,                   nameof(info.Generation)),
                                      Enricher.GetProperty(info.Concurrent,                   nameof(info.Concurrent)),
                                      Enricher.GetProperty(info.TotalCommittedBytes,          nameof(info.TotalCommittedBytes)),
                                      Enricher.GetProperty(info.PromotedBytes,                nameof(info.PromotedBytes)),
                                      Enricher.GetProperty(info.PinnedObjectsCount,           nameof(info.PinnedObjectsCount)),
                                      Enricher.GetProperty(info.FinalizationPendingCount,     nameof(info.FinalizationPendingCount)),
                                      Enricher.GetProperty(info.PauseTimePercentage,          nameof(info.PauseTimePercentage)),
                                      new LogEventProperty(nameof(info.GenerationInfo), new SequenceValue(generation)),
                                      new LogEventProperty(nameof(info.PauseDurations), new SequenceValue(pause))
                                  ]);
    }


    public static StructureValue Generation( in GCGenerationInfo info ) => new([
                                                                                   Enricher.GetProperty(info.SizeBeforeBytes,          nameof(info.SizeBeforeBytes)),
                                                                                   Enricher.GetProperty(info.FragmentationBeforeBytes, nameof(info.FragmentationBeforeBytes)),
                                                                                   Enricher.GetProperty(info.SizeAfterBytes,           nameof(info.SizeAfterBytes)),
                                                                                   Enricher.GetProperty(info.FragmentationAfterBytes,  nameof(info.FragmentationAfterBytes))
                                                                               ]);
}
