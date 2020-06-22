using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NETMetaCoder.Abstractions
{
    /// <summary>
    /// A function that generates syntax for declaring a property.
    ///
    /// That property lazily returns the instance of an attribute, for which a method has been wrapped.
    ///
    /// The returned attribute instance is used by the generated code to access its <see cref="NETMetaCoderAttribute"/>
    /// implementation.
    /// </summary>
    /// <param name="attributeName"></param>
    /// <param name="containerTypeSyntax"></param>
    /// <param name="syntax"></param>
    /// <param name="newMethodName"></param>
    public delegate IImmutableList<string> PropertySyntaxGenerator(string attributeName, TypeSyntax containerTypeSyntax,
        MethodDeclarationSyntax syntax, string newMethodName);
}
