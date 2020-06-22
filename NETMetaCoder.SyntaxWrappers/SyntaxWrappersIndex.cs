using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NETMetaCoder.Abstractions;

namespace NETMetaCoder.SyntaxWrappers
{
    /// <summary>
    /// An index of wrapper types, keyed by their names.
    /// </summary>
    public static class SyntaxWrappersIndex
    {
        /// <inheritdoc cref="SyntaxWrappersIndex"/>
        public static readonly IImmutableDictionary<string,
            (SyntaxList<UsingDirectiveSyntax> Usings,
            PropertySyntaxGenerator PropertySyntaxGenerator,
            IImmutableList<SyntaxWrapper> StatementWrappers)> WrapperTypes =
            new Dictionary<string,
                (SyntaxList<UsingDirectiveSyntax> Usings,
                PropertySyntaxGenerator PropertySyntaxGenerator,
                IImmutableList<SyntaxWrapper> SyntaxWrappers)>
            {
                {
                    nameof(CommonWrapper),
                    (CommonWrapper.Usings, SyntaxWrapperUtilities.Properties, CommonWrapper.SyntaxWrappers)
                },
                {
                    nameof(MustReturnValueWrapper),
                    (CommonWrapper.Usings, SyntaxWrapperUtilities.Properties,
                        MustReturnValueWrapper.SyntaxWrappers.Concat(CommonWrapper.SyntaxWrappers).ToImmutableList())
                },
                {
                    nameof(WithoutGenericParametersWrapper),
                    (CommonWrapper.Usings, SyntaxWrapperUtilities.Properties,
                        WithoutGenericParametersWrapper.SyntaxWrappers
                            .Concat(MustReturnValueWrapper.SyntaxWrappers)
                            .Concat(CommonWrapper.SyntaxWrappers)
                            .ToImmutableList())
                }
            }.ToImmutableDictionary();
    }
}
