using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NETMetaCoder.Abstractions;

namespace NETMetaCoder.SyntaxWrappers
{
    internal static class SyntaxWrapperUtilities
    {
        private static readonly Regex GenericTypeBacktickArity = new Regex(@"`[0-9]+", RegexOptions.Compiled);

        internal static IImmutableList<string> Properties(string attributeName, TypeSyntax containerTypeSyntax,
            MethodDeclarationSyntax syntax, string newMethodName)
        {
            var propertyName = GetPropertyName(syntax, attributeName);
            var isAsync = syntax.IsAsync().ToString().ToLowerInvariant();
            var genericTypeParameters = syntax.GetGenericTypeParameters();

            string returnType;

            if (genericTypeParameters.Contains(syntax.ReturnType.ToString()))
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                var type = GenericTypeBacktickArity.Replace(typeof(GenericPlaceholder<>).FullName, "");

                returnType = $"typeof({type}<>)";
            }
            else if (!syntax.ReturnType.IsGenericWithGenericTypeParameter(genericTypeParameters))
            {
                returnType = $"typeof({syntax.ReturnType})";
            }
            else
            {
                returnType = $"typeof({syntax.ReturnType.RemoveTypeArguments()})";
            }

            var methodName = $"{syntax.ExplicitInterfaceSpecifier}{syntax.Identifier.ToString()}";

            var parameterTypes = "new System.Type[] {" +
                string.Join(", ",
                    syntax.ParameterList.Parameters
                        .Where(p => p.Type != null)
                        .Select(p =>
                        {
                            if (genericTypeParameters.Contains(p.Type.ToString()))
                            {
                                // ReSharper disable once AssignNullToNotNullAttribute
                                var type = GenericTypeBacktickArity.Replace(typeof(GenericPlaceholder<>).FullName, "");

                                return $"typeof({type}<>)";
                            }

                            return $"typeof({p.WithType(p.Type.RemoveTypeArguments()).Type})";
                        })) +
                "}";

            var memberDeclaration = $@"
internal static Lazy<NETMetaCoderAttribute> {propertyName} {{ get; }} = new Lazy<NETMetaCoderAttribute>(() => {{
    var attribute =
        (NETMetaCoderAttribute)typeof({containerTypeSyntax})
                .GetMethod(
                    ""{newMethodName}"",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)?
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

    attribute.Init({isAsync}, typeof({containerTypeSyntax}), {returnType}, ""{methodName}"", {parameterTypes});

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
