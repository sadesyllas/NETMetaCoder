using Microsoft.CodeAnalysis;

namespace NETMetaCoder
{
    /// <summary>
    /// Represents the result of an invocation of <see cref="SyntaxRewriter"/>.
    /// </summary>
    public struct SyntaxRewriteResult
    {
        /// <summary>
        /// The potentially rewritten syntax tree of the processed compilation unit.
        /// </summary>
        public SyntaxTree SyntaxTree { get; set; }

        /// <summary>
        /// If true, then a rewrite occured for the processed compilation unit.
        /// </summary>
        public bool HasChanges { get; set; }
    }
}
