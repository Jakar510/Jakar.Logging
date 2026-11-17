// Jakar.Extensions :: Jakar.Logging
// 11/16/2026

namespace Jakar.Extensions;


/// <summary> The Jakar.Extensions exception/reflection helpers <see cref="ExceptionDetails"/> uses, kept internal so they don't clash with Jakar.Extensions' public ones. </summary>
internal static class DiagnosticsHelpers
{
    public static string[] SplitAndTrimLines( this string value, char separator = '\n' )
    {
        string[] array = value.Split(separator);
        for ( int i = 0; i < array.Length; i++ ) { array[i] = array[i].Trim(); }

        return array;
    }


    public static string? MethodClass( this MethodBase method ) => method.DeclaringType?.FullName;

    /// <summary> The same text as Jakar.Extensions' <c> MethodBase.MethodSignature() </c>. </summary>
    public static string MethodSignature( this MethodBase method )
    {
        ParameterInfo[] parameters = method.GetParameters();
        string[]        names      = new string[parameters.Length];
        for ( int i = 0; i < parameters.Length; i++ ) { names[i] = parameters[i].ParameterType.FullName ?? parameters[i].ParameterType.Name; }

        return $"( {string.Join(method.Name, names)} )";
    }


    [RequiresUnreferencedCode("Metadata for the method might be incomplete or removed")] public static string?        MethodClass( this     Exception e ) => e.TargetSite?.MethodClass();
    [RequiresUnreferencedCode("Metadata for the method might be incomplete or removed")] public static string?        MethodSignature( this Exception e ) => e.TargetSite?.MethodSignature();
    [RequiresUnreferencedCode("Metadata for the method might be incomplete or removed")] public static MethodDetails? MethodInfo( this      Exception e ) => e.TargetSite is { } method ? new MethodDetails(method) : null;


    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Diagnostic text only. Exception.TargetSite returns null when the metadata was trimmed, and any failure reading it is reported as null ('unknown').")]
    public static string? TryGetMethodSignature( this Exception e )
    {
        try { return e.TargetSite?.MethodSignature(); }
        catch ( Exception ) { return null; }
    }


    /// <summary> <see cref="Exception.Data"/> as a JSON object (no reflection-based serialization). </summary>
    public static JObjectNode GetData( this Exception e )
    {
        JObjectNode result = [];
        foreach ( DictionaryEntry entry in e.Data ) { result[entry.Key.ToString() ?? string.Empty] = ToJsonValue(entry.Value); }

        return result;
    }


    public static JNode? ToJsonValue( object? value ) => value switch
                                                         {
                                                             null => null,
                                                             JNode node => node.Parent is null
                                                                               ? node
                                                                               : node.DeepClone(),
                                                             string str       => str,
                                                             bool b           => b,
                                                             char c           => c.ToString(),
                                                             byte n           => n,
                                                             sbyte n          => n,
                                                             short n          => n,
                                                             ushort n         => n,
                                                             int n            => n,
                                                             uint n           => n,
                                                             long n           => n,
                                                             ulong n          => n,
                                                             float n          => n,
                                                             double n         => n,
                                                             decimal n        => n,
                                                             Guid g           => g,
                                                             DateTime d       => d,
                                                             DateTimeOffset d => d,
                                                             Enum en          => en.ToString(),
                                                             IFormattable f   => f.ToString(null, CultureInfo.InvariantCulture),
                                                             _                => value.ToString()
                                                         };
}