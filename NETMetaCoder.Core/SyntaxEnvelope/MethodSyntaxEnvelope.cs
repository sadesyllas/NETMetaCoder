using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NETMetaCoder.Core.SyntaxEnvelope
{
    /// <summary>
    /// A type that holds the syntax elements found in a method declaration.
    /// </summary>
    /// <seealso cref="SyntaxEnvelope"/>
    public sealed class MethodSyntaxEnvelope : IIndexedSyntaxEnvelope
    {
        /// <summary>
        /// Constructs an instance of <see cref="MethodSyntaxEnvelope"/> to hold an instance of
        /// <see cref="MethodDeclarationSyntax"/>.
        /// </summary>
        /// <param name="syntax">
        /// The class declaration syntax.
        /// </param>
        /// <param name="nodeIndex">
        /// A unique index for the syntax node, in order to identify it again in a later pass.
        /// </param>
        /// <param name="attributeNamesFound">
        /// The names of the attributes found on the method declaration.
        /// </param>
        /// <param name="methodObsoletion">
        /// The syntax node for the <see cref="ObsoleteAttribute"/> found on the method declaration, if any.
        /// </param>
        public MethodSyntaxEnvelope(MethodDeclarationSyntax syntax, ushort nodeIndex,
            ImmutableHashSet<string> attributeNamesFound, AttributeSyntax methodObsoletion)
        {
            MethodDeclarationSyntax = syntax;
            NodeIndex = nodeIndex;
            AttributeNamesFound = attributeNamesFound;
            MethodObsoletion = methodObsoletion;
        }

        /// <summary>
        /// The <see cref="MethodDeclarationSyntax"/> held inside the <see cref="MethodSyntaxEnvelope"/> instance.
        /// </summary>
        public MethodDeclarationSyntax MethodDeclarationSyntax { get; }

        /// <summary>
        /// The names of the attributes found on the method declaration.
        /// </summary>
        public ImmutableHashSet<string> AttributeNamesFound { get; }

        /// <summary>
        /// The syntax node for the <see cref="ObsoleteAttribute"/> found on the method declaration, if any.
        /// </summary>
        public AttributeSyntax MethodObsoletion { get; }

        /// <inheritdoc cref="IIndexedSyntaxEnvelope.NodeIndex"/>
        public ushort NodeIndex { get; set; }
    }
}
