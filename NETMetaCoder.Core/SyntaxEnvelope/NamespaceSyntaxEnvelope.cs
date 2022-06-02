using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NETMetaCoder.Core.SyntaxEnvelope
{
    /// <inheritdoc cref="NamespaceSyntaxEnvelopeBase"/>
    public sealed class NamespaceSyntaxEnvelope : NamespaceSyntaxEnvelopeBase, IIndexedSyntaxEnvelope
    {
        /// <summary>
        /// Constructs an instance of <see cref="NamespaceSyntaxEnvelope"/> to hold an instance of
        /// <see cref="NamespaceDeclarationSyntax"/>, along with a sub tree of its descendant nodes.
        /// </summary>
        /// <param name="syntax">
        /// The namespace declaration syntax.
        /// </param>
        /// <param name="nodeIndex">
        /// A unique index for the syntax node, in order to identify it again in a later pass.
        /// </param>
        public NamespaceSyntaxEnvelope(NamespaceDeclarationSyntax syntax, ushort nodeIndex)
        {
            NamespaceDeclarationSyntax = syntax;
            NodeIndex = nodeIndex;
        }
    }
}
