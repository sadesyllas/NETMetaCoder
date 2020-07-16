using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NETMetaCoder.Abstractions;

namespace NETMetaCoder.SyntaxWrappers
{
    internal static class SyntaxWrapperUtilities
    {
        internal static IImmutableList<string> Properties(string attributeName, TypeSyntax containerTypeSyntax,
            MethodDeclarationSyntax syntax, string newMethodName)
        {
            var propertyName = GetPropertyName(syntax, attributeName);

            var memberDeclaration = $@"
internal static Lazy<NETMetaCoderAttribute> {propertyName} {{ get; }} = new Lazy<NETMetaCoderAttribute>(() => {{
    var __stackFrame__ = new System.Diagnostics.StackTrace().GetFrames().FirstOrDefault(sf =>
        sf.GetMethod().DeclaringType.IsGenericType && typeof({containerTypeSyntax}).IsGenericType
            ? sf.GetMethod().DeclaringType.GetGenericTypeDefinition().ToString() ==
                typeof({containerTypeSyntax}).GetGenericTypeDefinition().ToString()
            : sf.GetMethod().DeclaringType == typeof({containerTypeSyntax}));

    if (__stackFrame__ == null)
    {{
        throw new Exception(
            ""[NETMetaCoder] Failed to get calling method, for wrapped method \""{newMethodName}\"", in type "" +
            $""{{typeof({containerTypeSyntax}).FullName}}."");
    }}

    var __wrapperMethodInfo__ = __stackFrame__.GetMethod();
    var __parameters__ = __wrapperMethodInfo__.GetParameters();
    var __parameterInfoEqualityComparer__ = new ParameterInfoEqualityComparer();

    var __wrappedMethodInfo__ = __wrapperMethodInfo__.DeclaringType
        .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
        .FirstOrDefault(m =>
            m.Name == ""{newMethodName}"" &&
            m.GetParameters().SequenceEqual(__parameters__, __parameterInfoEqualityComparer__));

    if (__wrappedMethodInfo__ == null)
    {{
        throw new Exception(
            ""[NETMetaCoder] Failed to get wrapped method \""{newMethodName}\"", in type "" +
            $""{{typeof({containerTypeSyntax}).FullName}}."");
    }}

    var attribute =
        (NETMetaCoderAttribute)__wrappedMethodInfo__
        .GetCustomAttributes()
        .FirstOrDefault(a => {{
            var attributeType = a.GetType();

            return
                attributeType.IsSubclassOf(typeof(NETMetaCoderAttribute)) &&
                attributeType.Name.StartsWith(""{attributeName}"");
        }})
        ?? throw new Exception(
            ""[NETMetaCoder] Attribute of type \""{nameof(NETMetaCoderAttribute)}\"" not found on method"" +
            ""\""{newMethodName}\""."");

    attribute.Init(__wrapperMethodInfo__, __wrappedMethodInfo__);

    return attribute;
}});

";

            return new[] {memberDeclaration}.ToImmutableList();
        }

        internal static string GetAttributeVariableName(string attributeName) => $"__attribute{attributeName}";

        internal static string GetPropertyName(MethodDeclarationSyntax syntax, string attributeName)
        {
            var needle = syntax.ParameterList.Parameters.Select(p => p.Type?.ToString()).Aggregate("", (a, i) => a + i);

            if (needle == "")
            {
                needle = "void";
            }

            needle = BitConverter.ToString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(needle))).Replace("-", "");

            return $"{syntax.Identifier.ToString()}__PropertyForAttribute{attributeName}__{needle}";
        }

        internal static string FormatArgumentList(IEnumerable<ParameterSyntax> arguments) =>
            string.Join(", ", arguments
                .Where(p => !p.Modifiers.Any(m => m.IsKind(SyntaxKind.OutKeyword)))
                .Select(p => p.Identifier.ToString()));

        internal static bool IsNoValueReturn(MethodDeclarationSyntax syntax, out string returnTypeDescription)
        {
            var returnType = syntax.ReturnType.ToString();

            if (returnType == "void")
            {
                returnTypeDescription = returnType;

                return true;
            }

            if (returnType.Split('.').Last() == "Task")
            {
                returnTypeDescription = syntax.HasAsyncModifier() ? "async Task" : "Task";

                return true;
            }

            returnTypeDescription = null;

            return false;
        }

        internal static string GetInterceptionResultVariableName(string attributeName) =>
            $"__interceptionResult{attributeName}";

        internal static string GetGenericTypeForInterception(MethodDeclarationSyntax syntax)
        {
            var returnType = syntax.ReturnType.ToString();

            return returnType == "void" ? "" : $"<{returnType}>";
        }
    }
}
