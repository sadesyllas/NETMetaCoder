using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NETMetaCoder.SyntaxEnvelope
{
    /// <summary>
    /// A type that holds the syntax elements found in a class or struct declaration.
    /// </summary>
    /// <seealso cref="SyntaxEnvelope"/>
    public abstract class ClassOrStructSyntaxEnvelopeBase
    {
        private readonly List<ClassOrStructSyntaxEnvelope> _classOrStructSyntaxEnvelopes =
            new List<ClassOrStructSyntaxEnvelope>();

        /// <summary>
        /// The <see cref="ClassDeclarationSyntax"/> held inside the <see cref="ClassOrStructSyntaxEnvelope"/> instance.
        /// </summary>
        public ClassDeclarationSyntax ClassDeclarationSyntax { get; private protected set; }

        /// <summary>
        /// The <see cref="StructDeclarationSyntax"/> held inside the <see cref="ClassOrStructSyntaxEnvelope"/>
        /// instance.
        /// </summary>
        public StructDeclarationSyntax StructDeclarationSyntax { get; private protected set; }

        internal EnvelopeType Type => ClassDeclarationSyntax != null ? EnvelopeType.Class : EnvelopeType.Struct;

        /// <summary>
        /// Returns <c>true</c> if this <see cref="ClassOrStructSyntaxEnvelope"/> instance holds a
        /// <see cref="ClassDeclarationSyntax"/> and <c>false</c> otherwise.
        /// </summary>
        public bool IsClassDeclarationSyntax => Type == EnvelopeType.Class;

        /// <summary>
        /// The class and struct syntax nodes that are direct children of the class or struct syntax node, held by an
        /// instance of <see cref="ClassOrStructSyntaxEnvelope"/>.
        /// </summary>
        public ImmutableList<ClassOrStructSyntaxEnvelope> ClassOrStructSyntaxEnvelopes =>
            _classOrStructSyntaxEnvelopes.ToImmutableList();

        /// <summary>
        /// Adds a class declaration syntax node to the envelope.
        /// </summary>
        /// <param name="syntax"></param>
        /// <param name="nodeIndex"></param>
        /// <returns>
        /// It returns a new envelope, which is a child of the envelope instance in the context of which,
        /// <see cref="AddClassSyntax"/> was called (ie, it returns the next level in the tree of syntax nodes).
        /// </returns>
        public ClassOrStructSyntaxEnvelope AddClassSyntax(ClassDeclarationSyntax syntax, ushort nodeIndex)
        {
            var envelope = new ClassOrStructSyntaxEnvelope(syntax, nodeIndex);

            _classOrStructSyntaxEnvelopes.Add(envelope);

            return envelope;
        }

        /// <summary>
        /// Adds a struct declaration syntax node to the envelope.
        /// </summary>
        /// <param name="syntax"></param>
        /// <param name="nodeIndex"></param>
        /// <returns>
        /// It returns a new envelope, which is a child of the envelope instance in the context of which,
        /// <see cref="AddStructSyntax"/> was called (ie, it returns the next level in the tree of syntax nodes).
        /// </returns>
        public ClassOrStructSyntaxEnvelope AddStructSyntax(StructDeclarationSyntax syntax, ushort nodeIndex)
        {
            var envelope = new ClassOrStructSyntaxEnvelope(syntax, nodeIndex);

            _classOrStructSyntaxEnvelopes.Add(envelope);

            return envelope;
        }

        /// <summary>
        /// Prunes the syntax node tree rooted at an instance of <see cref="ClassOrStructSyntaxEnvelope"/> from empty
        /// children envelopes.
        /// </summary>
        protected void Prune() => Prune(_classOrStructSyntaxEnvelopes);

        private protected static void Prune(List<ClassOrStructSyntaxEnvelope> classOrStructSyntaxEnvelopes)
        {
            // `.ToArray()` is used because we are modifying the original collection.
            foreach (var classOrStructSyntaxEnvelope in classOrStructSyntaxEnvelopes.ToArray())
            {
                classOrStructSyntaxEnvelope.Prune();

                if (!classOrStructSyntaxEnvelope.ClassOrStructSyntaxEnvelopes.Any() &&
                    !classOrStructSyntaxEnvelope.MethodSyntaxEnvelopes.Any())
                {
                    classOrStructSyntaxEnvelopes.Remove(classOrStructSyntaxEnvelope);
                }
            }
        }

        internal enum EnvelopeType : byte
        {
            Class,
            Struct
        }
    }
}
