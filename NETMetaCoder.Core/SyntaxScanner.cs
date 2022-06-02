using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NETMetaCoder.Abstractions;
using NETMetaCoder.Core.SyntaxEnvelope;

namespace NETMetaCoder.Core
{
    /// <summary>
    /// This type encapsulates the logic for building a <see cref="SyntaxEnvelope"/>, by scanning the syntax tree of a
    /// compilation unit.
    /// </summary>
    public static class SyntaxScanner
    {
        /// <summary>
        /// Constructs a new <see cref="SyntaxScanner"/> instance.
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="attributeNames"></param>
        /// <returns></returns>
        public static SyntaxEnvelope.SyntaxEnvelope
            ScanSyntaxTree(SyntaxTree tree, IEnumerable<string> attributeNames) =>
            new SyntaxScannerWithContext(tree, attributeNames).ScanSyntaxTree();

        private sealed class SyntaxScannerWithContext : CSharpSyntaxWalker
        {
            private readonly SyntaxTree _tree;
            private readonly IEnumerable<string> _attributeNames;
            private readonly SyntaxEnvelope.SyntaxEnvelope _syntaxEnvelope = new SyntaxEnvelope.SyntaxEnvelope();
            private ushort _nodeIndex;

            private readonly Stack<NamespaceSyntaxEnvelope> _namespaceSyntaxEnvelopeStack =
                new Stack<NamespaceSyntaxEnvelope>();

            private readonly Stack<ClassOrStructSyntaxEnvelope> _classOrStructSyntaxEnvelopeStack =
                new Stack<ClassOrStructSyntaxEnvelope>();

            public SyntaxScannerWithContext(SyntaxTree tree, IEnumerable<string> attributeNames)
            {
                _tree = tree;
                _attributeNames = attributeNames;
            }

            public SyntaxEnvelope.SyntaxEnvelope ScanSyntaxTree()
            {
                if (_tree.GetDiagnostics().Any(d => d.Severity == DiagnosticSeverity.Error))
                {
                    var errorIds = string.Join(Environment.NewLine, _tree.GetDiagnostics()
                        .Where(d => d.Severity == DiagnosticSeverity.Error)
                        .Select(d => d.ToString()));

                    throw new NETMetaCoderException(
                        $"The syntax tree has errors and no scanning will take place:{Environment.NewLine}{errorIds}");
                }

                Visit(_tree.GetCompilationUnitRoot());

                _syntaxEnvelope.Prune();

                return _syntaxEnvelope;
            }

            public override void VisitUsingDirective(UsingDirectiveSyntax node) =>
                _syntaxEnvelope.AddUsingDirectiveSyntax(node);

            public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
            {
                _nodeIndex++;

                UpdateNamespaceSyntaxEnvelopeStack(node);

                foreach (var childNode in node.ChildNodes())
                {
                    Visit(childNode);
                }
            }

            public override void VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                _nodeIndex++;

                UpdateClassOrStructSyntaxEnvelopeStack(node);

                foreach (var childNode in node.ChildNodes())
                {
                    Visit(childNode);
                }
            }

            public override void VisitStructDeclaration(StructDeclarationSyntax node)
            {
                _nodeIndex++;

                UpdateClassOrStructSyntaxEnvelopeStack(node);

                foreach (var childNode in node.ChildNodes())
                {
                    Visit(childNode);
                }
            }

            public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
            {
                _nodeIndex++;

                AttributeSyntax methodObsoletion = null;

                var attributeNamesFound = node.FindAttributes(_attributeNames,
                    (_, referenceAttributeName) => { _syntaxEnvelope.AddAttributeNameFound(referenceAttributeName); },
                    attributeSyntax =>
                    {
                        if (attributeSyntax.IsMethodObsoletionAttribute())
                        {
                            methodObsoletion = attributeSyntax;
                        }
                    });

                if (attributeNamesFound.Any())
                {
                    UpdateClassOrStructSyntaxEnvelopeStackHead(node, attributeNamesFound.ToImmutableHashSet(),
                        methodObsoletion);
                }
            }

            private static bool IsSameNode(NamespaceDeclarationSyntax a, NamespaceDeclarationSyntax b)
            {
                if (a == null && b == null)
                {
                    return true;
                }

                var canBeTheSameNode = a != null && b != null && a.Name.ToString() == b.Name.ToString();

                if (canBeTheSameNode)
                {
                    if (a.Parent is CompilationUnitSyntax && b.Parent is CompilationUnitSyntax)
                    {
                        return true;
                    }

                    if (a.Parent is NamespaceDeclarationSyntax aa && b.Parent is NamespaceDeclarationSyntax bb)
                    {
                        return IsSameNode(aa, bb);
                    }
                }

                return false;
            }

            private static bool IsSameNode(TypeDeclarationSyntax a, TypeDeclarationSyntax b)
            {
                if (a == null && b == null)
                {
                    return true;
                }

                if (a is ClassDeclarationSyntax && !(b is ClassDeclarationSyntax) ||
                    a is StructDeclarationSyntax && !(b is StructDeclarationSyntax))
                {
                    return false;
                }

                var canBeTheSameNode = a != null && b != null && a.Identifier.ToString() == b.Identifier.ToString();

                if (canBeTheSameNode)
                {
                    if (a.Parent is ClassDeclarationSyntax ac && b.Parent is ClassDeclarationSyntax bc)
                    {
                        return IsSameNode(ac, bc);
                    }

                    if (a.Parent is StructDeclarationSyntax @as && b.Parent is StructDeclarationSyntax bs)
                    {
                        return IsSameNode(@as, bs);
                    }

                    if (a.Parent is NamespaceDeclarationSyntax an && b.Parent is NamespaceDeclarationSyntax bn)
                    {
                        return IsSameNode(an, bn);
                    }

                    if (a.Parent is CompilationUnitSyntax && b.Parent is CompilationUnitSyntax)
                    {
                        return true;
                    }
                }

                return false;
            }

            private void UpdateNamespaceSyntaxEnvelopeStack(NamespaceDeclarationSyntax node)
            {
                // A new namespace means that the current class or struct nesting level must be abandoned.
                _classOrStructSyntaxEnvelopeStack.Clear();

                {
                    // While no common parent can be found, pop the namespace syntax stack,
                    while (_namespaceSyntaxEnvelopeStack.Any())
                    {
                        var namespaceSyntaxEnvelope = _namespaceSyntaxEnvelopeStack.Peek();

                        // if the current namespace syntax node is the same as the new syntax node's parent, then we
                        // have found the node under which we need to place the new syntax node.
                        if (node.Parent is NamespaceDeclarationSyntax namespaceSyntaxParent &&
                            IsSameNode(namespaceSyntaxParent, namespaceSyntaxEnvelope.NamespaceDeclarationSyntax))
                        {
                            break;
                        }

                        _namespaceSyntaxEnvelopeStack.Pop();
                    }
                }

                {
                    NamespaceSyntaxEnvelope newNamespaceSyntaxEnvelope;

                    // If we have any namespace syntax nodes left in the stack,
                    if (_namespaceSyntaxEnvelopeStack.Any())
                    {
                        var namespaceSyntaxEnvelope = _namespaceSyntaxEnvelopeStack.Peek();

                        // we place the new syntax node under it.
                        newNamespaceSyntaxEnvelope = namespaceSyntaxEnvelope.AddNamespaceSyntax(node, _nodeIndex);
                    }
                    // Else,
                    else
                    {
                        // we place the new syntax node under the compilation unit's root.
                        newNamespaceSyntaxEnvelope = _syntaxEnvelope.AddNamespaceSyntax(node, _nodeIndex);
                    }

                    _namespaceSyntaxEnvelopeStack.Push(newNamespaceSyntaxEnvelope);
                }
            }

            private void UpdateClassOrStructSyntaxEnvelopeStack(TypeDeclarationSyntax node)
            {
                {
                    var foundParentClassOrStructSyntax = false;

                    // While no common parent can be found, pop the class syntax stack,
                    while (_classOrStructSyntaxEnvelopeStack.Any())
                    {
                        var classOrStructSyntaxEnvelope = _classOrStructSyntaxEnvelopeStack.Peek();

                        // if the current class or struct syntax node is the same as the new syntax node's parent, then
                        // we have found the node under which we need to place the new syntax node.
                        if ((node.Parent is ClassDeclarationSyntax classSyntaxParent &&
                             IsSameNode(classSyntaxParent, classOrStructSyntaxEnvelope.ClassDeclarationSyntax)) ||
                            (node.Parent is StructDeclarationSyntax structSyntaxParent &&
                             IsSameNode(structSyntaxParent, classOrStructSyntaxEnvelope.StructDeclarationSyntax)))
                        {
                            foundParentClassOrStructSyntax = true;

                            break;
                        }

                        _classOrStructSyntaxEnvelopeStack.Pop();
                    }

                    if (!foundParentClassOrStructSyntax)
                    {
                        // While no common parent can be found, pop the namespace syntax stack,
                        while (_namespaceSyntaxEnvelopeStack.Any())
                        {
                            var namespaceSyntaxEnvelope = _namespaceSyntaxEnvelopeStack.Peek();

                            // if the current namespace syntax node is the same as the new syntax node's parent, then we
                            // have found the node under which we need to place the new syntax node.
                            if (node.Parent is NamespaceDeclarationSyntax namespaceSyntaxParent &&
                                IsSameNode(namespaceSyntaxParent, namespaceSyntaxEnvelope.NamespaceDeclarationSyntax))
                            {
                                break;
                            }

                            _namespaceSyntaxEnvelopeStack.Pop();
                        }
                    }
                }

                {
                    ClassOrStructSyntaxEnvelope newClassOrStructSyntaxEnvelope;

                    // If we have any class syntax nodes left in the stack,
                    if (_classOrStructSyntaxEnvelopeStack.Any())
                    {
                        var classOrStructSyntaxEnvelope = _classOrStructSyntaxEnvelopeStack.Peek();

                        // we place the new syntax node under it.
                        if (node is ClassDeclarationSyntax classDeclarationSyntax)
                        {
                            newClassOrStructSyntaxEnvelope =
                                classOrStructSyntaxEnvelope.AddClassSyntax(classDeclarationSyntax, _nodeIndex);
                        }
                        else if (node is StructDeclarationSyntax structDeclarationSyntax)
                        {
                            newClassOrStructSyntaxEnvelope =
                                classOrStructSyntaxEnvelope.AddStructSyntax(structDeclarationSyntax, _nodeIndex);
                        }
                        else
                        {
                            throw new ArgumentException(
                                $"The syntax node must be either of type {nameof(ClassDeclarationSyntax)}, or " +
                                $"{nameof(StructDeclarationSyntax)}.");
                        }
                    }
                    // Else, if we have any namespace syntax nodes left in the stack,
                    else if (_namespaceSyntaxEnvelopeStack.Any())
                    {
                        var namespaceSyntaxEnvelope = _namespaceSyntaxEnvelopeStack.Peek();

                        // we place the new syntax node under it.
                        if (node is ClassDeclarationSyntax classDeclarationSyntax)
                        {
                            newClassOrStructSyntaxEnvelope =
                                namespaceSyntaxEnvelope.AddClassSyntax(classDeclarationSyntax, _nodeIndex);
                        }
                        else if (node is StructDeclarationSyntax structDeclarationSyntax)
                        {
                            newClassOrStructSyntaxEnvelope =
                                namespaceSyntaxEnvelope.AddStructSyntax(structDeclarationSyntax, _nodeIndex);
                        }
                        else
                        {
                            throw new ArgumentException(
                                $"The syntax node must be either of type {nameof(ClassDeclarationSyntax)}, or " +
                                $"{nameof(StructDeclarationSyntax)}.");
                        }
                    }
                    // Else,
                    else
                    {
                        // we place the new syntax node under the compilation unit's root.
                        if (node is ClassDeclarationSyntax classDeclarationSyntax)
                        {
                            newClassOrStructSyntaxEnvelope =
                                _syntaxEnvelope.AddClassSyntax(classDeclarationSyntax, _nodeIndex);
                        }
                        else if (node is StructDeclarationSyntax structDeclarationSyntax)
                        {
                            newClassOrStructSyntaxEnvelope =
                                _syntaxEnvelope.AddStructSyntax(structDeclarationSyntax, _nodeIndex);
                        }
                        else
                        {
                            throw new ArgumentException(
                                $"The syntax node must be either of type {nameof(ClassDeclarationSyntax)}, or " +
                                $"{nameof(StructDeclarationSyntax)}.");
                        }
                    }

                    _classOrStructSyntaxEnvelopeStack.Push(newClassOrStructSyntaxEnvelope);
                }
            }

            private void UpdateClassOrStructSyntaxEnvelopeStackHead(MethodDeclarationSyntax node,
                ImmutableHashSet<string> attributeNamesFound, AttributeSyntax methodObsoletion)
            {
                // While no common parent can be found, pop the class or struct syntax stack,
                while (_classOrStructSyntaxEnvelopeStack.Any())
                {
                    var classOrStructSyntaxEnvelope = _classOrStructSyntaxEnvelopeStack.Peek();

                    // if the current class or struct syntax node is the same as the new syntax node's parent, then we
                    // have found the node under which we need to place the new syntax node.
                    if ((node.Parent is ClassDeclarationSyntax classSyntaxParent &&
                         IsSameNode(classSyntaxParent, classOrStructSyntaxEnvelope.ClassDeclarationSyntax)) ||
                        (node.Parent is StructDeclarationSyntax structSyntaxParent &&
                         IsSameNode(structSyntaxParent, classOrStructSyntaxEnvelope.StructDeclarationSyntax)))
                    {
                        break;
                    }

                    _classOrStructSyntaxEnvelopeStack.Pop();
                }

                // This call should never fail because we check for error diagnostics before we begin and syntactically,
                // it is not possible to have a method outside a class or struct.
                _classOrStructSyntaxEnvelopeStack.Peek()
                    .AddMethodSyntax(node, _nodeIndex, attributeNamesFound, methodObsoletion);
            }
        }
    }
}