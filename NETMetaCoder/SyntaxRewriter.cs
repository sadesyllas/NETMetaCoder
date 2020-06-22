using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NETMetaCoder.Abstractions;

namespace NETMetaCoder
{
    /// <summary>
    /// This type is responsible for rewriting a compilation unit's syntax.
    /// </summary>
    public static class SyntaxRewriter
    {
        /// <summary>
        /// Constructs a new <see cref="SyntaxRewriter"/> instance.
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="attributeNames"></param>
        /// <param name="syntaxEnvelope"></param>
        /// <returns></returns>
        public static SyntaxRewriteResult RewriteSyntaxTree(
            SyntaxTree tree, IEnumerable<string> attributeNames, SyntaxEnvelope.SyntaxEnvelope syntaxEnvelope) =>
            new SyntaxRewriterWithContext(tree, attributeNames, syntaxEnvelope).RewriteSyntaxTree();

        private sealed class SyntaxRewriterWithContext : CSharpSyntaxRewriter
        {
            private readonly SyntaxTree _tree;
            private readonly IEnumerable<string> _attributeNames;
            private readonly HashSet<ushort> _targetNodeIndices;
            private ushort _nodeIndex;
            private bool _hasChanges;

            public SyntaxRewriterWithContext(SyntaxTree tree, IEnumerable<string> attributeNames,
                SyntaxEnvelope.SyntaxEnvelope syntaxEnvelope)
            {
                _tree = tree;
                _attributeNames = attributeNames.ToList();
                _targetNodeIndices = syntaxEnvelope.GatherNodeIndices();
            }

            public SyntaxRewriteResult RewriteSyntaxTree()
            {
                if (_tree.GetDiagnostics().Any(d => d.Severity == DiagnosticSeverity.Error))
                {
                    throw new NETMetaCoderException("The syntax tree has errors and no rewriting will take place.");
                }

                if (!_targetNodeIndices.Any())
                {
                    return new SyntaxRewriteResult
                    {
                        SyntaxTree = _tree,
                        HasChanges = false
                    };
                }

                var newTree = _tree.WithRootAndOptions(Visit(_tree.GetCompilationUnitRoot()), _tree.Options);

                return new SyntaxRewriteResult
                {
                    SyntaxTree = newTree,
                    HasChanges = _hasChanges
                };
            }

            public override SyntaxNode VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
            {
                _nodeIndex++;

                return base.VisitNamespaceDeclaration(node);
            }

            public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                _nodeIndex++;

                if (!_targetNodeIndices.Contains(_nodeIndex))
                {
                    return base.VisitClassDeclaration(node);
                }

                node = node.WithPartialKeywordPrefix();

                return base.VisitClassDeclaration(node);
            }

            public override SyntaxNode VisitStructDeclaration(StructDeclarationSyntax node)
            {
                _nodeIndex++;

                if (!_targetNodeIndices.Contains(_nodeIndex))
                {
                    return base.VisitStructDeclaration(node);
                }

                node = node.WithPartialKeywordPrefix();

                return base.VisitStructDeclaration(node);
            }

            public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
            {
                _nodeIndex++;

                if (!_targetNodeIndices.Contains(_nodeIndex))
                {
                    return node;
                }

                var attributeNamesFound = node.FindAttributes(_attributeNames, null, null);
                var isTarget = attributeNamesFound.Any();

                _hasChanges = isTarget;

                if (isTarget)
                {
                    var attributeNameNeedles = attributeNamesFound
                        .Select(StringExtensions.ToAttributeNameNeedle)
                        .Distinct()
                        .ToList();

                    attributeNameNeedles.Sort();

                    var newMethodName = node.Identifier.ToString();

                    newMethodName = attributeNameNeedles.Aggregate(newMethodName,
                        (methodName, attributeNameNeedle) => methodName.Replace(attributeNameNeedle, ""));

                    newMethodName = attributeNameNeedles.Aggregate(newMethodName,
                        (methodName, attributeNameNeedle) => methodName + attributeNameNeedle);

                    var attributeLists = new SyntaxList<AttributeListSyntax>(node.AttributeLists.Select(al =>
                    {
                        var attributes = al.Attributes.Select(a =>
                            a.IsMethodObsoletionAttribute()
                                ? a.WithArgumentList(SyntaxFactory.AttributeArgumentList())
                                : a);

                        return al.WithAttributes(SyntaxFactory.SeparatedList(attributes));
                    }).Where(al => al.Attributes.Any()));

                    var newModifiers = new SyntaxTokenList(node.Modifiers.Where(modifier =>
                        !modifier.IsKind(SyntaxKind.OverrideKeyword) && !modifier.IsKind(SyntaxKind.SealedKeyword)));

                    node = node
                        .WithAttributeLists(attributeLists)
                        .WithModifiers(newModifiers)
                        .WithExplicitInterfaceSpecifier(null)
                        .WithIdentifier(SyntaxFactory.Identifier(newMethodName));
                }

                return base.VisitMethodDeclaration(node);
            }
        }
    }
}
