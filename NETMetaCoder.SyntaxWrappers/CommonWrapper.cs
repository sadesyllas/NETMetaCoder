using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NETMetaCoder.Abstractions;

namespace NETMetaCoder.SyntaxWrappers
{
    /// <summary>
    /// A wrapper type that defines a fixed way to wrap method calls.
    ///
    /// The generated code is of the following format:
    ///
    /// <code>
    /// var attributeInstance = PropertyName.Value;
    /// var interceptionResult = attributeInstance.Intercept(new object[] {arg1, arg2, ...}[, ref result]);
    /// if (!interceptionResult.IsIntercepted)
    /// {
    ///     try
    ///     {
    ///         // call to wrapped method or inner block of previously wrapped method call
    ///     }
    ///     catch (Exception exception)
    ///     {
    ///         if (!attributeInstance.HandleException(exception, [ref result, ]ref interceptionResult))
    ///         {
    ///             throw;
    ///         }
    ///     }
    /// }
    /// attributeInstance.HandleInterceptionResult([ref result, ]ref interceptionResult);
    /// </code>
    ///
    /// The optional <c>ref result</c> is passed to the above calls only when the wrapped method returns a value.
    /// </summary>
    public class CommonWrapper
    {
        /// <summary>
        /// The using declarations that are required by the produced code.
        /// </summary>
        public static SyntaxList<UsingDirectiveSyntax> Usings => new SyntaxList<UsingDirectiveSyntax>(new[]
        {
            SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("System").WithLeadingSpace()),
            SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("System.Linq").WithLeadingSpace()),
            SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("System.Reflection").WithLeadingSpace()),
            SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("NETMetaCoder.Abstractions").WithLeadingSpace())
        });

        /// <summary>
        /// The syntax generator functions that produce the syntax that wraps a method call.
        /// </summary>
        public static IImmutableList<SyntaxWrapper> SyntaxWrappers { get; } = new[]
        {
            new SyntaxWrapper
            {
                PreMapper = (attributeName, syntax, newMethodName) =>
                {
                    var attributeVariableName = SyntaxWrapperUtilities.GetAttributeVariableName(attributeName);
                    var propertyName = SyntaxWrapperUtilities.GetPropertyName(syntax, attributeName);
                    var arguments = SyntaxWrapperUtilities.FormatArgumentList(syntax.ParameterList.Parameters);
                    var valueType = SyntaxWrapperUtilities.GetGenericTypeForInterception(syntax);
                    var refArgument = valueType != "" ? ", ref __result" : "";

                    var interceptionResultVariableName =
                        SyntaxWrapperUtilities.GetInterceptionResultVariableName(attributeName);

                    return new[]
                    {
                        $@"
var {attributeVariableName} = {propertyName}.Value;
var {interceptionResultVariableName} = {attributeVariableName}.Intercept(new object[] {{{arguments}}}{refArgument});
if (!{interceptionResultVariableName}.IsIntercepted)
{{
    try
    {{
",
                    };
                },
                PostMapper = (attributeName, syntax, newMethodName) =>
                {
                    var attributeVariableName = SyntaxWrapperUtilities.GetAttributeVariableName(attributeName);
                    var valueType = SyntaxWrapperUtilities.GetGenericTypeForInterception(syntax);
                    var refArgument = valueType != "" ? "ref __result, " : "";

                    var interceptionResultVariableName =
                        SyntaxWrapperUtilities.GetInterceptionResultVariableName(attributeName);

                    return new[]
                    {
                        $@"
    }}
    catch (Exception exception)
    {{
        if (!{attributeVariableName}.HandleException(exception, {refArgument}ref {interceptionResultVariableName}))
        {{
            throw;
        }}
    }}
}}
{attributeVariableName}.HandleInterceptionResult({refArgument}ref {interceptionResultVariableName});
"
                    };
                }
            }
        }.ToImmutableList();
    }
}
