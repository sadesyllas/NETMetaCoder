using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NETMetaCoder.Core.SyntaxEnvelope
{
    /// <summary>
    /// A type that holds the syntax elements found in a namespace declaration.
    /// </summary>
    /// <seealso cref="SyntaxEnvelope"/>
    public abstract class NamespaceSyntaxEnvelopeBase : ClassOrStructSyntaxEnvelopeBase
    {
        private readonly List<NamespaceSyntaxEnvelope> _namespaceSyntaxEnvelopes = new List<NamespaceSyntaxEnvelope>();

        /// <summary>
        /// The <see cref="NamespaceDeclarationSyntax"/> held inside the <see cref="NamespaceSyntaxEnvelope"/> instance.
        /// </summary>
        public NamespaceDeclarationSyntax NamespaceDeclarationSyntax { get; private protected set; }

        /// <summary>
        /// The namespace syntax nodes that are direct children of the namespace syntax node, held by an instance of
        /// <see cref="NamespaceSyntaxEnvelope"/>.
        /// </summary>
        public ImmutableList<NamespaceSyntaxEnvelope> NamespaceSyntaxEnvelopes =>
            _namespaceSyntaxEnvelopes.ToImmutableList();

        /// <inheritdoc cref="IIndexedSyntaxEnvelope.NodeIndex"/>
        public ushort NodeIndex { get; set; }

        /// <summary>
        /// Adds a namespace declaration syntax node to the envelope.
        /// </summary>
        /// <param name="syntax"></param>
        /// <param name="nodeIndex"></param>
        /// <returns>
        /// It returns a new envelope, which is a child of the envelope instance in the context of which,
        /// <see cref="AddNamespaceSyntax"/> was called (ie, it returns the next level in the tree of syntax nodes).
        /// </returns>
        public NamespaceSyntaxEnvelope AddNamespaceSyntax(NamespaceDeclarationSyntax syntax, ushort nodeIndex)
        {
            var envelope = new NamespaceSyntaxEnvelope(syntax, nodeIndex);

            _namespaceSyntaxEnvelopes.Add(envelope);

            return envelope;
        }

        /// <summary>
        /// Prunes the syntax node tree rooted at an instance of <see cref="NamespaceSyntaxEnvelope"/> from empty
        /// children envelopes.
        /// </summary>
        private new void Prune()
        {
            Prune(_namespaceSyntaxEnvelopes);

            base.Prune();
        }

        private protected static void Prune(List<NamespaceSyntaxEnvelope> namespaceSyntaxEnvelopes)
        {
            // `.ToArray()` is used because we are modifying the original collection.
            foreach (var namespaceSyntaxEnvelope in namespaceSyntaxEnvelopes.ToArray())
            {
                namespaceSyntaxEnvelope.Prune();

                if (!namespaceSyntaxEnvelope.NamespaceSyntaxEnvelopes.Any() &&
                    !namespaceSyntaxEnvelope.ClassOrStructSyntaxEnvelopes.Any())
                {
                    namespaceSyntaxEnvelopes.Remove(namespaceSyntaxEnvelope);
                }
            }
        }
    }
}
