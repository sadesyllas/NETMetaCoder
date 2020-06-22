using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NETMetaCoder.Abstractions
{
    /// <summary>
    /// A function that generates the code that wraps a method call.
    /// </summary>
    /// <param name="attributeName">
    /// The attribute name which caused the method call to be wrapped.
    /// </param>
    /// <param name="syntax">
    /// The method declaration syntax which defines the method to be wrapped.
    /// </param>
    /// <param name="newMethodName">
    /// The new method name that the wrapped method will have, after it has been wrapped by the generated method body.
    /// </param>
    public delegate IEnumerable<string> MethodSyntaxGenerator(string attributeName, MethodDeclarationSyntax syntax,
        string newMethodName);
}
