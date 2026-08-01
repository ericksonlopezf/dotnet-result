// Polyfill for System.Runtime.CompilerServices.IsExternalInit required by C# 9+ init/record features
// when targeting netstandard2.0. Roslyn source generators target netstandard2.0 and this type
// is not present in that TFM's BCL. This is the standard pattern used by Microsoft's own generators.
// Sealed + internal ensures it doesn't conflict if another assembly provides the same polyfill.
namespace System.Runtime.CompilerServices
{
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    internal sealed class IsExternalInit { }
}
