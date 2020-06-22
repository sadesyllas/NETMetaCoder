using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NETMetaCoder.Abstractions
{
    /// <summary>
    /// Extension methods for manipulating syntax nodes.
    /// </summary>
    public static class SyntaxExtensions
    {
        /// <summary>
        /// Adds a leading space to a <see cref="SyntaxToken"/>.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static SyntaxToken WithLeadingSpace(this SyntaxToken token) =>
            token.WithLeadingTrivia(SyntaxFactory.Space);

        /// <summary>
        /// Adds a leading space syntax node.
        /// </summary>
        /// <param name="node"></param>
        /// <typeparam name="T"></typeparam>
        public static T WithLeadingSpace<T>(this T node) where T : SyntaxNode =>
            node.WithLeadingTrivia(SyntaxFactory.Space);

        /// <summary>
        /// Adds a trailing new line syntax node.
        /// </summary>
        /// <param name="node"></param>
        /// <typeparam name="T"></typeparam>
        public static T WithTrailingLineFeed<T>(this T node) where T : SyntaxNode =>
            node.WithTrailingTrivia(SyntaxFactory.LineFeed);

        /// <summary>
        /// Adds a leading and a trailing new line syntax nodes.
        /// </summary>
        /// <param name="node"></param>
        /// <typeparam name="T"></typeparam>
        public static T WithSurroundingLineFeed<T>(this T node) where T : SyntaxNode =>
            node.WithLeadingTrivia(SyntaxFactory.LineFeed).WithTrailingLineFeed();

        /// <summary>
        /// Adds a <c>partial</c> modifier to a <see cref="ClassDeclarationSyntax"/>.
        /// </summary>
        /// <param name="syntax"></param>
        public static ClassDeclarationSyntax WithPartialKeywordPrefix(this ClassDeclarationSyntax syntax) =>
            (ClassDeclarationSyntax) WithPartialKeywordPrefix((TypeDeclarationSyntax) syntax);

        /// <summary>
        /// Adds a <c>partial</c> modifier to a <see cref="StructDeclarationSyntax"/>.
        /// </summary>
        /// <param name="syntax"></param>
        public static StructDeclarationSyntax WithPartialKeywordPrefix(this StructDeclarationSyntax syntax) =>
            (StructDeclarationSyntax) WithPartialKeywordPrefix((TypeDeclarationSyntax) syntax);

        /// <summary>
        /// Returns true is the <see cref="MethodDeclarationSyntax"/> contains the <c>async</c> modifier.
        /// </summary>
        /// <param name="syntax"></param>
        public static bool HasAsyncModifier(this MethodDeclarationSyntax syntax) =>
            syntax.Modifiers.Any(SyntaxKind.AsyncKeyword);

        /// <summary>
        /// Returns true if the <see cref="MethodDeclarationSyntax"/> defines that the method returns a
        /// <see cref="Task"/> or a <see cref="Task{T}"/>.
        /// </summary>
        /// <param name="syntax"></param>
        public static bool IsAsync(this MethodDeclarationSyntax syntax) =>
            syntax.ReturnType.ToString().Split('.').Last().Split('<').First() == "Task";

        /// <summary>
        /// Extracts the names of the generic parameters of a <see cref="MethodDeclarationSyntax"/>.
        /// </summary>
        /// <param name="syntax"></param>
        public static IImmutableSet<string> GetGenericTypeParameters(this MethodDeclarationSyntax syntax) =>
            syntax.TypeParameterList?.Parameters.Select(p => p.Identifier.ToString()).ToImmutableHashSet() ??
            ImmutableHashSet<string>.Empty;

        /// <summary>
        /// Returns true if the <see cref="TypeSyntax"/> represents a generic type.
        /// </summary>
        /// <param name="syntax"></param>
        /// <param name="methodGenericParameters"></param>
        public static bool IsGenericWithGenericTypeParameter(this TypeSyntax syntax,
            IImmutableSet<string> methodGenericParameters)
        {
            if (syntax == null || methodGenericParameters == null || !methodGenericParameters.Any())
            {
                return false;
            }

            bool CheckChildNodesRecursively(IEnumerable<SyntaxNode> childNodes)
            {
                // ReSharper disable PossibleMultipleEnumeration
                if (childNodes == null || !childNodes.Any())
                {
                    return false;
                }

                foreach (var childNode in childNodes)
                {
                    if (childNode.IsKind(SyntaxKind.TypeArgumentList))
                    {
                        var typeArgumentListSyntax = (TypeArgumentListSyntax) childNode;

                        if (typeArgumentListSyntax.Arguments.Any(a => methodGenericParameters.Contains(a.ToString())))
                        {
                            return true;
                        }
                    }

                    if (CheckChildNodesRecursively(childNode?.ChildNodes()))
                    {
                        return true;
                    }
                }

                return false;
                // ReSharper restore PossibleMultipleEnumeration
            }

            return CheckChildNodesRecursively(syntax.ChildNodes());
        }

        /// <summary>
        /// Removes the type parameters from a type, if any.
        ///
        /// Ie, it turns <c>A&lt;T&gt;</c> into <c>A&lt;&gt;</c>.
        /// </summary>
        /// <param name="syntax"></param>
        public static TypeSyntax RemoveTypeArguments(this TypeSyntax syntax)
        {
            if (syntax == null)
            {
                return null;
            }

            var typeArgumentListSyntax = (TypeArgumentListSyntax) syntax
                .ChildNodes()
                .FirstOrDefault(n => n.IsKind(SyntaxKind.TypeArgumentList));

            if (typeArgumentListSyntax != null)
            {
                var emptyTypeArguments = typeArgumentListSyntax.Arguments.Select(_ => SyntaxFactory.ParseTypeName(""));

                var newTypeArgumentListSyntax =
                    typeArgumentListSyntax.WithArguments(SyntaxFactory.SeparatedList(emptyTypeArguments));

                syntax = syntax.ReplaceNode(typeArgumentListSyntax, newTypeArgumentListSyntax);
            }

            return syntax;
        }

        /// <summary>
        /// Returns true if the <see cref="MethodDeclarationSyntax"/> represents a method that returns a value.
        /// </summary>
        /// <param name="syntax"></param>
        /// <param name="isVoid"></param>
        /// <param name="asyncTaskTypeArgument"></param>
        public static bool ReturnsValue(this MethodDeclarationSyntax syntax, out bool isVoid,
            out string asyncTaskTypeArgument)
        {
            var returnType = syntax.ReturnType.ToString();

            isVoid = returnType == "void";
            asyncTaskTypeArgument = null;

            if (isVoid)
            {
                return false;
            }

            if (!syntax.HasAsyncModifier())
            {
                return true;
            }

            asyncTaskTypeArgument = (
                (TypeArgumentListSyntax) syntax.ReturnType.ChildNodes()
                    .FirstOrDefault(n => n.IsKind(SyntaxKind.TypeArgumentList))
            )?.Arguments.FirstOrDefault()?.ToString();

            return asyncTaskTypeArgument != null;
        }

        /// <summary>
        /// Returns true if the <see cref="AttributeSyntax"/> represents a <see cref="ObsoleteAttribute"/>.
        /// </summary>
        /// <param name="syntax"></param>
        public static bool IsMethodObsoletionAttribute(this AttributeSyntax syntax) =>
            syntax.Name.ToString().Split('.').Last() == "Obsolete";

        /// <summary>
        /// Extracts attribute names from a <see cref="MethodDeclarationSyntax"/>, based on the provided criteria.
        /// </summary>
        /// <param name="syntax"></param>
        /// <param name="referenceAttributeNames"></param>
        /// <param name="matchingAttributeCallback"></param>
        /// <param name="attributeCallback"></param>
        public static HashSet<string> FindAttributes(this MethodDeclarationSyntax syntax,
            IEnumerable<string> referenceAttributeNames,
            Action<AttributeSyntax, string> matchingAttributeCallback,
            Action<AttributeSyntax> attributeCallback)
        {
            var attributeNamesFound = new HashSet<string>();

            foreach (var attributeList in syntax.AttributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    var attributeName = attribute.Name.ToString();

                    // ReSharper disable once PossibleMultipleEnumeration
                    foreach (var referenceAttributeName in referenceAttributeNames)
                    {
                        if (attributeName.Contains(referenceAttributeName) ||
                            attributeName.Contains(referenceAttributeName + Constants.AttributeSuffix))
                        {
                            attributeNamesFound.Add(referenceAttributeName);

                            matchingAttributeCallback?.Invoke(attribute, referenceAttributeName);
                        }
                    }

                    attributeCallback?.Invoke(attribute);
                }
            }

            return attributeNamesFound;
        }

        private static TypeDeclarationSyntax WithPartialKeywordPrefix(this TypeDeclarationSyntax syntax)
        {
            if (!syntax.Modifiers.Any(SyntaxKind.PartialKeyword))
            {
                var hasModifiers = syntax.Modifiers.Any();

                var partialKeywordToken = SyntaxFactory.Token(SyntaxKind.PartialKeyword)
                    .WithTrailingTrivia(SyntaxFactory.Space);

                partialKeywordToken = hasModifiers
                    ? partialKeywordToken
                    : partialKeywordToken.WithLeadingTrivia(syntax.GetLeadingTrivia());

                syntax = hasModifiers ? syntax : syntax.WithLeadingTrivia(SyntaxTriviaList.Empty);
                syntax = syntax.WithModifiers(syntax.Modifiers.Add(partialKeywordToken));
            }

            return syntax;
        }
    }
}
