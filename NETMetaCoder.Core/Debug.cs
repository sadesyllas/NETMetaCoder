using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NETMetaCoder.Core.SyntaxEnvelope;

namespace NETMetaCoder.Core
{
    /// <summary>
    /// Provides utilities meant only as a debugging tool, during development.
    /// </summary>
    public static class Debug
    {
        private const char IndentChar = ' ';
        private const int IndentLevel = 4;

        /// <summary>
        /// Pretty prints a <see cref="SyntaxEnvelope"/>.
        /// </summary>
        /// <param name="result"></param>
        public static void Print(SyntaxEnvelope.SyntaxEnvelope result)
        {
#if DEBUG
            if (result == null)
            {
                Console.WriteLine($"null {nameof(SyntaxEnvelope.SyntaxEnvelope)}");

                return;
            }

            Console.WriteLine($"{nameof(result.HasSyntaxToRender)} = {result.HasSyntaxToRender}");

            Print(result.NamespaceSyntaxEnvelopes, -IndentLevel);

            Print(result.ClassOrStructSyntaxEnvelopes, -IndentLevel);

            Console.WriteLine("Indices: [" +
                result.GatherNodeIndices().Select(index => index.ToString())
                    .Aggregate((acc, value) => $"{acc}, {value}") +
                "]");
#endif
        }

        private static void Print(IImmutableList<NamespaceSyntaxEnvelope> namespaceSyntaxEnvelopes, int indent)
        {
            foreach (var namespaceSyntaxEnvelope in namespaceSyntaxEnvelopes)
            {
                Print(namespaceSyntaxEnvelope, indent + IndentLevel);
            }
        }

        private static void Print(IImmutableList<ClassOrStructSyntaxEnvelope> classOrStructSyntaxEnvelopes, int indent)
        {
            foreach (var classOrStructSyntaxEnvelope in classOrStructSyntaxEnvelopes)
            {
                Print(classOrStructSyntaxEnvelope, indent + IndentLevel);
            }
        }

        private static void Print(NamespaceSyntaxEnvelope namespaceSyntaxEnvelope, int indent)
        {
            var syntax = namespaceSyntaxEnvelope.NamespaceDeclarationSyntax ??
                throw new ArgumentNullException(
                    $"{nameof(namespaceSyntaxEnvelope.NamespaceDeclarationSyntax)} must not be null.");

            Console.WriteLine(
                $"{new string(IndentChar, indent)}(N{namespaceSyntaxEnvelope.NodeIndex}) {syntax.Name.ToString()}");

            Print(namespaceSyntaxEnvelope.NamespaceSyntaxEnvelopes, indent + IndentLevel);

            Print(namespaceSyntaxEnvelope.ClassOrStructSyntaxEnvelopes, indent + IndentLevel);
        }

        private static void Print(ClassOrStructSyntaxEnvelope classOrStructSyntaxEnvelope, int indent)
        {
            var syntax = classOrStructSyntaxEnvelope.DeclarationSyntax ??
                throw new ArgumentNullException(
                    $"{nameof(classOrStructSyntaxEnvelope.DeclarationSyntax)} must not be null.");

            var prefix = classOrStructSyntaxEnvelope.IsClassDeclarationSyntax ? "C" : "S";

            Console.WriteLine($"{new string(IndentChar, indent)}({prefix}{classOrStructSyntaxEnvelope.NodeIndex}) " +
                $"{syntax.Identifier.ToString()}");

            foreach (var methodSyntaxEnvelope in classOrStructSyntaxEnvelope.MethodSyntaxEnvelopes)
            {
                Print(methodSyntaxEnvelope.MethodDeclarationSyntax, methodSyntaxEnvelope.NodeIndex,
                    indent + IndentLevel);
            }

            Print(classOrStructSyntaxEnvelope.ClassOrStructSyntaxEnvelopes, indent);
        }

        private static void Print(MethodDeclarationSyntax methodDeclarationSyntax, ushort nodeIndex, int indent) =>
            Console.WriteLine(
                $"{new string(IndentChar, indent)}(M{nodeIndex}) {methodDeclarationSyntax.Identifier.ToString()}");
    }
}
