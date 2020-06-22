namespace NETMetaCoder.Abstractions
{
    /// <summary>
    /// A placeholder type to be used in the rewritten syntax, when a generic parameter is used in the original code but
    /// is not available in the rewritten code.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// It is expected to be used only as <c>GenericPlaceholder&lt;&gt;</c>.
    /// </remarks>
    // ReSharper disable once UnusedTypeParameter
    public sealed class GenericPlaceholder<T>
    {
    }
}
