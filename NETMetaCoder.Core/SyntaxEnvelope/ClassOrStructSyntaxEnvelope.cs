using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NETMetaCoder.Core.SyntaxEnvelope
{
    /// <inheritdoc cref="ClassOrStructSyntaxEnvelopeBase"/>
    public sealed class ClassOrStructSyntaxEnvelope : ClassOrStructSyntaxEnvelopeBase, IIndexedSyntaxEnvelope
    {
        private readonly List<MethodSyntaxEnvelope> _methodSyntaxEnvelopes = new List<MethodSyntaxEnvelope>();

        /// <summary>
        /// Constructs an instance of <see cref="ClassOrStructSyntaxEnvelope"/> to hold an instance of
        /// <see cref="ClassDeclarationSyntax"/>, along with a sub tree of its descendant nodes..
        /// </summary>
        /// <param name="syntax">
        /// The class declaration syntax.
        /// </param>
        /// <param name="nodeIndex">
        /// A unique index for the syntax node, in order to identify it again in a later pass.
        /// </param>
        public ClassOrStructSyntaxEnvelope(ClassDeclarationSyntax syntax, ushort nodeIndex)
        {
            ClassDeclarationSyntax = syntax;
            NodeIndex = nodeIndex;
        }

        /// <summary>
        /// Constructs an instance of <see cref="ClassOrStructSyntaxEnvelope"/> to hold an instance of
        /// <see cref="StructDeclarationSyntax"/>.
        /// </summary>
        /// <param name="syntax">
        /// The struct declaration syntax.
        /// </param>
        /// <param name="nodeIndex">
        /// A unique index for the syntax node, in order to identify it again in a later pass.
        /// </param>
        /// <seealso cref="NodeIndex"/>
        public ClassOrStructSyntaxEnvelope(StructDeclarationSyntax syntax, ushort nodeIndex)
        {
            StructDeclarationSyntax = syntax;
            NodeIndex = nodeIndex;
        }



        /// <summary>
        /// The declaration syntax of the class or struct that this <see cref="ClassOrStructSyntaxEnvelope"/> refers to.
        /// </summary>
        public TypeDeclarationSyntax DeclarationSyntax =>
            IsClassDeclarationSyntax ? (TypeDeclarationSyntax) ClassDeclarationSyntax : StructDeclarationSyntax;

        /// <summary>
        /// The method syntax nodes that are direct children of the class or struct syntax node, held by an instance of
        /// <see cref="ClassOrStructSyntaxEnvelope"/>.
        /// </summary>
        public ImmutableList<MethodSyntaxEnvelope> MethodSyntaxEnvelopes => _methodSyntaxEnvelopes.ToImmutableList();

        /// <inheritdoc cref="IIndexedSyntaxEnvelope.NodeIndex"/>
        public ushort NodeIndex { get; set; }

        /// <summary>
        /// Adds a method declaration syntax node to the envelope.
        /// </summary>
        /// <param name="syntax"></param>
        /// <param name="nodeIndex"></param>
        /// <param name="attributeNamesFound">
        /// The names of the attributes found on the method declaration.
        /// </param>
        /// <param name="methodObsoletion">
        /// The syntax node for the <see cref="ObsoleteAttribute"/> found on the method declaration, if any.
        /// </param>
        /// <returns>
        /// It returns a new envelope, which is a child of the envelope instance in the context of which,
        /// <see cref="AddMethodSyntax"/> was called (ie, it returns the next level in the tree of syntax nodes).
        /// </returns>
        /// <seealso cref="MethodSyntaxEnvelope"/>
        public void AddMethodSyntax(MethodDeclarationSyntax syntax, ushort nodeIndex,
            ImmutableHashSet<string> attributeNamesFound, AttributeSyntax methodObsoletion)
        {
            var envelope = new MethodSyntaxEnvelope(syntax, nodeIndex, attributeNamesFound, methodObsoletion);

            _methodSyntaxEnvelopes.Add(envelope);
        }
    }
}
