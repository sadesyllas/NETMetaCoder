namespace NETMetaCoder.Abstractions
{
    /// <summary>
    /// This type represents a wrapper around a method call.
    /// </summary>
    public sealed class SyntaxWrapper
    {
        /// <summary>
        /// The output of this syntax generator is placed before a wrapped method call.
        /// </summary>
        public MethodSyntaxGenerator PreMapper { get; set; }

        /// <summary>
        /// The output of this syntax generator is placed after a wrapped method call.
        /// </summary>
        public MethodSyntaxGenerator PostMapper { get; set; }
    }
}
