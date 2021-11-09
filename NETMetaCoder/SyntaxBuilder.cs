using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NETMetaCoder.Abstractions;
using NETMetaCoder.SyntaxEnvelope;

namespace NETMetaCoder
{
    /// <summary>
    /// This type produces the syntax that wraps method calls in a compilation unit, based on a
    /// <see cref="SyntaxEnvelope"/>.
    /// </summary>
    public class SyntaxBuilder
    {
        private const string ResultIdentifier = "__result";

        private static readonly UsingDirectiveSyntax UsingSystemRuntimeCompilerServicesUsingDirectiveSyntax =
            SyntaxFactory.UsingDirective(
                SyntaxFactory.IdentifierName("System.Runtime.CompilerServices").WithLeadingSpace());

        private static readonly UsingDeclarationSyntaxComparer UsingDeclarationSyntaxComparer =
            new UsingDeclarationSyntaxComparer();

        // `using System.Runtime.CompilerServices;` is added elsewhere.
        private static readonly AttributeListSyntax MethodImplementationAttributeListSyntax =
            SyntaxFactory
                .ParseSyntaxTree(
                    // `MethodImplOptions.AggressiveOptimization` is not used since it can change the intended method
                    // semantics.
                    "[MethodImpl(MethodImplOptions.AggressiveInlining)]")
                .GetCompilationUnitRoot()
                .Members
                .First()
                .AttributeLists
                .First();

        private readonly CodeWrapTransformationOptions _options;
        private readonly SyntaxEnvelope.SyntaxEnvelope _syntaxEnvelope;

        /// <summary>
        /// Constructs a new <see cref="SyntaxBuilder"/> instance.
        /// </summary>
        /// <param name="syntaxEnvelope"></param>
        /// <param name="options"></param>
        public SyntaxBuilder(SyntaxEnvelope.SyntaxEnvelope syntaxEnvelope, ref CodeWrapTransformationOptions options)
        {
            _options = options;
            _syntaxEnvelope = syntaxEnvelope;
        }

        /// <summary>
        /// This method builds the syntax tree which wraps method calls in a compilation unit.
        /// </summary>
        /// <remarks>
        /// The steps taken by this method are:
        /// 1. It instantiates a new syntax tree, which is an instance of <see cref="CompilationUnitSyntax"/>, and it
        ///   traverses the syntax tree held by a <see cref="SyntaxEnvelope"/>.
        /// 2. For each namespace, class and struct syntax node, it produces the relevant declaration syntax.
        /// 3. In the case when the <see cref="SyntaxEnvelope"/> traversal reaches a method declaration, then the syntax
        ///   that is produced wraps a call to the original method, using the syntax generators in
        /// <see cref="CodeWrapTransformationOptions"/>.
        /// </remarks>
        public SyntaxTree Build()
        {
            var tree = SyntaxFactory.ParseSyntaxTree("");
            var root = tree.GetCompilationUnitRoot();
            var namespaceMembers = Build(_syntaxEnvelope.NamespaceSyntaxEnvelopes).Cast<MemberDeclarationSyntax>();
            var classMembers = Build(_syntaxEnvelope.ClassOrStructSyntaxEnvelopes).Cast<MemberDeclarationSyntax>();
            var members = new SyntaxList<MemberDeclarationSyntax>(namespaceMembers.Concat(classMembers));

            var usings =
                new SyntaxList<UsingDirectiveSyntax>(_syntaxEnvelope.Usings
                    .Select(syntax => syntax.WithoutTrivia().WithSurroundingLineFeed())
                    .Concat(_options.SelectUsings(_syntaxEnvelope.AttributeNamesFound))
                    // Add `using System.Runtime.CompilerServices;` to support the `MethodImpl` method attribute.
                    .Append(UsingSystemRuntimeCompilerServicesUsingDirectiveSyntax)
                    .Distinct(UsingDeclarationSyntaxComparer));

            root = root.WithUsings(usings).WithMembers(members);

            return tree.WithRootAndOptions(root, tree.Options);
        }

        private IEnumerable<NamespaceDeclarationSyntax> Build(
            IEnumerable<NamespaceSyntaxEnvelope> namespaceSyntaxEnvelopes) => namespaceSyntaxEnvelopes.Select(Build);

        private NamespaceDeclarationSyntax Build(NamespaceSyntaxEnvelope namespaceSyntaxEnvelope)
        {
            var syntax = namespaceSyntaxEnvelope.NamespaceDeclarationSyntax.WithoutTrivia().WithSurroundingLineFeed() ??
                         throw new ArgumentNullException(
                             $"{nameof(namespaceSyntaxEnvelope.NamespaceDeclarationSyntax)} must not be null.");

            var namespaceMembers =
                Build(namespaceSyntaxEnvelope.NamespaceSyntaxEnvelopes).Cast<MemberDeclarationSyntax>();

            var classOrStructMembers = Build(namespaceSyntaxEnvelope.ClassOrStructSyntaxEnvelopes)
                .Cast<MemberDeclarationSyntax>();

            var members = new SyntaxList<MemberDeclarationSyntax>(namespaceMembers.Concat(classOrStructMembers));

            return SyntaxFactory.NamespaceDeclaration(syntax.AttributeLists, syntax.Modifiers,
                syntax.Name.WithLeadingSpace(), syntax.Externs, syntax.Usings, members);
        }

        private IEnumerable<TypeDeclarationSyntax>
            Build(IEnumerable<ClassOrStructSyntaxEnvelope> classOrStructSyntaxEnvelopes) =>
            classOrStructSyntaxEnvelopes.Select(Build);

        private TypeDeclarationSyntax Build(ClassOrStructSyntaxEnvelope classOrStructSyntaxEnvelope)
        {
            var syntax = classOrStructSyntaxEnvelope.DeclarationSyntax.WithoutTrivia().WithSurroundingLineFeed() ??
                         throw new ArgumentNullException(
                             $"{nameof(classOrStructSyntaxEnvelope.DeclarationSyntax)} must not be null.");

            var typeSyntax = SyntaxFactory.ParseTypeName($"{syntax.Identifier.ToString()}{syntax.TypeParameterList}");

            var propertyAndMethodMembers = classOrStructSyntaxEnvelope.MethodSyntaxEnvelopes
                .Select(methodSyntaxEnvelope => Build(typeSyntax, methodSyntaxEnvelope));

            var propertyMembers = new List<MemberDeclarationSyntax>();
            var methodMembers = new List<MemberDeclarationSyntax>();

            foreach (var (propertyMemberGroup, methodMember) in propertyAndMethodMembers)
            {
                propertyMembers.AddRange(propertyMemberGroup);
                methodMembers.Add(methodMember);
            }

            var classMembers = classOrStructSyntaxEnvelope.ClassOrStructSyntaxEnvelopes.Select(Build)
                .Cast<MemberDeclarationSyntax>();

            var attributeLists = new SyntaxList<AttributeListSyntax>();
            var modifiers = syntax.Modifiers;
            var identifier = syntax.Identifier.WithLeadingSpace();
            var typeParameterList = syntax.TypeParameterList;
            var baseList = syntax.BaseList;
            var constraintClauses = syntax.ConstraintClauses;

            var members =
                new SyntaxList<MemberDeclarationSyntax>(propertyMembers.Concat(methodMembers).Concat(classMembers));

            return classOrStructSyntaxEnvelope.IsClassDeclarationSyntax
                ? (TypeDeclarationSyntax)SyntaxFactory
                    .ClassDeclaration(attributeLists, modifiers, identifier, typeParameterList, baseList,
                        constraintClauses, members)
                    .WithPartialKeywordPrefix()
                : SyntaxFactory
                    .StructDeclaration(attributeLists, modifiers, identifier, typeParameterList, baseList,
                        constraintClauses, members)
                    .WithPartialKeywordPrefix();
        }

        private (IImmutableList<PropertyDeclarationSyntax>, MethodDeclarationSyntax) Build(
            TypeSyntax containerTypeSyntax, MethodSyntaxEnvelope methodSyntaxEnvelope)
        {
            var syntax = methodSyntaxEnvelope.MethodDeclarationSyntax.WithoutTrivia().WithSurroundingLineFeed() ??
                         throw new ArgumentNullException(
                             $"{nameof(methodSyntaxEnvelope.MethodDeclarationSyntax)} must not be null.");

            var methodName = syntax.Identifier;

            var attributeNameNeedles = methodSyntaxEnvelope.AttributeNamesFound
                .Select(StringExtensions.ToAttributeNameNeedle)
                .ToList();

            attributeNameNeedles.Sort();

            var wrappedMethodName = attributeNameNeedles.Aggregate(methodName.ToString(),
                (tmpMethodName, attributeNameNeedle) => tmpMethodName + attributeNameNeedle);

            var typeArguments = syntax.TypeParameterList != null && syntax.TypeParameterList.Parameters.Any()
                ? "<" +
                  string.Join(", ", syntax.TypeParameterList.Parameters.Select(p => p.Identifier.ToString())) +
                  ">"
                : "";

            var outParameters = new List<ParameterSyntax>();

            var hasRefParameter = false;
            var hasOutParameter = false;

            var arguments = SyntaxFactory.ArgumentList(
                SyntaxFactory.SeparatedList(syntax.ParameterList.Parameters.Select(p =>
                {
                    foreach (var modifier in p.Modifiers)
                    {
                        switch (modifier.Kind())
                        {
                            case SyntaxKind.RefKeyword:
                                hasRefParameter = true;

                                break;
                            case SyntaxKind.OutKeyword:
                                hasOutParameter = true;

                                outParameters.Add(p);

                                break;
                        }
                    }

                    var parameterExpression = hasRefParameter
                        ? SyntaxFactory.ParseExpression($"ref {p.Identifier}")
                        : hasOutParameter
                            ? SyntaxFactory.ParseExpression($"out {p.Identifier}")
                            : SyntaxFactory.ParseExpression(p.Identifier.ToString());

                    return SyntaxFactory.Argument(parameterExpression);
                })));

            var returnType = syntax.ReturnType.ToString();
            var isVoid = returnType == "void";

            var outParameterInitializations = outParameters.Select(p => $"{p.Identifier} = default({p.Type});");

            var resultDeclarationExpression =
                isVoid ? null : $"{returnType} {ResultIdentifier} = default({returnType});";

            var callToWrappedMethodDelegateTypeExpression = isVoid ? "System.Action" : $"System.Func<{returnType}>";

            var callToWrappedMethodLocalFunctionDeclarationExpression =
                $"{callToWrappedMethodDelegateTypeExpression} __wrappedMethodCaller = " + (
                    hasRefParameter || hasOutParameter
                        ? "() => throw new InvalidOperationException(" +
                          "\"Cannot create a delegate for a wrapped method that has a `ref` or `out` parameter. \");"
                        : $"() => {wrappedMethodName}{typeArguments}{arguments.ToFullString()};"
                );

            var resultAssignmentExpression = isVoid ? "__wrappedMethodCaller();" : "__result = __wrappedMethodCaller();";

            var preExpressions = _options.SelectPreExpressionMappers(methodSyntaxEnvelope.AttributeNamesFound)
                .Reverse()
                .SelectMany(mapper => mapper.SyntaxGenerators
                    .SelectMany(generator => generator(mapper.AttributeName, syntax, wrappedMethodName)));

            var postExpressions = _options.SelectPostExpressionMappers(methodSyntaxEnvelope.AttributeNamesFound)
                .SelectMany(mapper => mapper.SyntaxGenerators
                    .SelectMany(generator => generator(mapper.AttributeName, syntax, wrappedMethodName)));

            var returnExpression = isVoid ? null : $"return {ResultIdentifier};";

            var expressions = new List<string>()
                .Concat(outParameterInitializations)
                .Append(resultDeclarationExpression)
                .Append(callToWrappedMethodLocalFunctionDeclarationExpression)
                .Concat(preExpressions)
                .Append(resultAssignmentExpression)
                .Concat(postExpressions)
                .Append(returnExpression)
                .Where(statement => statement != null);

            var netMetaCoderMarkerAttributeListSyntaxes = methodSyntaxEnvelope.AttributeNamesFound.Select(_ =>
                SyntaxFactory.AttributeList(SyntaxFactory.SeparatedList(new[]
                {
                    SyntaxFactory.Attribute(SyntaxFactory.ParseName("NETMetaCoderMarker"))
                })));

            var attributeLists = new SyntaxList<AttributeListSyntax>()
                .AddRange(netMetaCoderMarkerAttributeListSyntaxes)
                .Add(MethodImplementationAttributeListSyntax);

            if (methodSyntaxEnvelope.MethodObsoletion != null)
            {
                attributeLists = attributeLists.Add(
                    SyntaxFactory.AttributeList(SyntaxFactory.SeparatedList(new[]
                        { methodSyntaxEnvelope.MethodObsoletion })));
            }

            var attributes = new SyntaxList<AttributeListSyntax>(attributeLists);
            var modifiers = new SyntaxTokenList(syntax.Modifiers.Where(m => !m.IsKind(SyntaxKind.AsyncKeyword)));
            var body = SyntaxFactory.Block(SyntaxFactory.ParseStatement($"{{{string.Join("\n", expressions)}}}"));

            var propertiesSyntax = _options.SelectPropertySyntaxGenerators(methodSyntaxEnvelope.AttributeNamesFound)
                .SelectMany(generator =>
                    generator.SyntaxGenerator(generator.AttributeName, containerTypeSyntax, syntax, wrappedMethodName));

            // The last argument (ie, `SyntaxToken semicolonToken`) is set to `null` because we are using a
            // `BlockSyntax`.
            var methodSyntax = SyntaxFactory.MethodDeclaration(attributes, modifiers, syntax.ReturnType,
                syntax.ExplicitInterfaceSpecifier, methodName, syntax.TypeParameterList, syntax.ParameterList,
                syntax.ConstraintClauses, body, null);

            var propertyDeclarations = propertiesSyntax
                .Select(p => (PropertyDeclarationSyntax)SyntaxFactory.ParseMemberDeclaration(p))
                .ToImmutableList();

            return (propertyDeclarations, methodSyntax);
        }
    }

    internal class UsingDeclarationSyntaxComparer : IEqualityComparer<UsingDirectiveSyntax>
    {
        public bool Equals(UsingDirectiveSyntax a, UsingDirectiveSyntax b) => a?.Name.ToString() == b?.Name.ToString();

        public int GetHashCode(UsingDirectiveSyntax syntax) => syntax.Name.ToString().GetHashCode();
    }
}
