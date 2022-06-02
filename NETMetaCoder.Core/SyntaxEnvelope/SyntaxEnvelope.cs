using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NETMetaCoder.Core.SyntaxEnvelope
{
    /// <summary>
    /// A type that holds a syntax tree for a compilation unit.
    ///
    /// The syntax tree serves to filter out all but what is important for this library to work on.
    ///
    /// Namely, this library needs only an hierarchy of namespaces, classes, structs and methods.
    /// </summary>
    public sealed class SyntaxEnvelope : NamespaceSyntaxEnvelopeBase
    {
        private readonly List<UsingDirectiveSyntax> _usingDirectiveSyntaxes = new List<UsingDirectiveSyntax>();

        private readonly HashSet<string> _attributeNamesFound = new HashSet<string>();

        /// <summary>
        /// The using declaration syntax nodes that are used in the compilation unit, held by an instance of
        /// <see cref="SyntaxEnvelope"/>.
        /// </summary>
        public ImmutableList<UsingDirectiveSyntax> Usings => _usingDirectiveSyntaxes.ToImmutableList();

        /// <summary>
        /// A set of attribute names found, on method declarations, throughout the compilation unit's syntax tree.
        /// </summary>
        public ImmutableHashSet<string> AttributeNamesFound => _attributeNamesFound.ToImmutableHashSet();

        /// <summary>
        /// Returns true if there are any namespace, class or struct declarations in a compilation unit.
        /// </summary>
        public bool HasSyntaxToRender => NamespaceSyntaxEnvelopes.Any() || ClassOrStructSyntaxEnvelopes.Any();

        /// <summary>
        /// Adds a using declaration syntax node to the envelope.
        /// </summary>
        /// <param name="syntax"></param>
        public void AddUsingDirectiveSyntax(UsingDirectiveSyntax syntax) => _usingDirectiveSyntaxes.Add(syntax);

        /// <summary>
        /// Adds a found attribute's name to the envelope.
        /// </summary>
        public void AddAttributeNameFound(string attributeNameFound) => _attributeNamesFound.Add(attributeNameFound);

        /// <summary>
        /// Gathers the syntax node indices of the whole tree so that the returned <see cref="HashSet{T}"/> can serve as
        /// an index of seen syntax nodes.
        ///
        /// This is index is used by subsequent compilation unit passes, to filter out unwanted syntax nodes.
        /// </summary>
        /// <returns></returns>
        public HashSet<ushort> GatherNodeIndices()
        {
            var indices = new HashSet<ushort>();

            foreach (var namespaceSyntaxEnvelope in NamespaceSyntaxEnvelopes)
            {
                GatherNodeIndices(namespaceSyntaxEnvelope, indices);
            }

            foreach (var classSyntaxEnvelope in ClassOrStructSyntaxEnvelopes)
            {
                GatherNodeIndices(classSyntaxEnvelope, indices);
            }

            return indices;
        }

        /// <summary>
        /// Prunes the syntax node tree rooted at an instance of <see cref="SyntaxEnvelope"/> from empty children
        /// envelopes.
        /// </summary>
        public new void Prune() => base.Prune();

        private static void GatherNodeIndices(NamespaceSyntaxEnvelope namespaceSyntaxEnvelope, HashSet<ushort> indices)
        {
            indices.Add(namespaceSyntaxEnvelope.NodeIndex);

            foreach (var subNamespaceSyntaxEnvelope in namespaceSyntaxEnvelope.NamespaceSyntaxEnvelopes)
            {
                GatherNodeIndices(subNamespaceSyntaxEnvelope, indices);
            }

            foreach (var subClassOrStructSyntaxEnvelope in namespaceSyntaxEnvelope.ClassOrStructSyntaxEnvelopes)
            {
                GatherNodeIndices(subClassOrStructSyntaxEnvelope, indices);
            }
        }

        private static void GatherNodeIndices(ClassOrStructSyntaxEnvelope classOrStructSyntaxEnvelope,
            HashSet<ushort> indices)
        {
            indices.Add(classOrStructSyntaxEnvelope.NodeIndex);

            foreach (var methodSyntaxEnvelope in classOrStructSyntaxEnvelope.MethodSyntaxEnvelopes)
            {
                indices.Add(methodSyntaxEnvelope.NodeIndex);
            }

            foreach (var subClassOrStructSyntaxEnvelope in classOrStructSyntaxEnvelope.ClassOrStructSyntaxEnvelopes)
            {
                GatherNodeIndices(subClassOrStructSyntaxEnvelope, indices);
            }
        }
    }
}
