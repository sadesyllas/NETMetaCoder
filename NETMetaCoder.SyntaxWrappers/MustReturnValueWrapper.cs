using System.Collections.Generic;
using System.Collections.Immutable;
using NETMetaCoder.Abstractions;

namespace NETMetaCoder.SyntaxWrappers
{
    /// <summary>
    /// A wrapper type that checks that the method being wrapped returns a value.
    /// </summary>
    /// <remarks>
    /// It is meant to be used with <see cref="CommonWrapper"/>.
    /// </remarks>
    /// <seealso cref="CommonWrapper"/>
    /// <seealso cref="WithoutGenericParametersWrapper"/>
    public class MustReturnValueWrapper
    {
        /// <inheritdoc cref="CommonWrapper.SyntaxWrappers"/>
        public static IImmutableList<SyntaxWrapper> SyntaxWrappers { get; } = new List<SyntaxWrapper>
            {
                new SyntaxWrapper
                {
                    PreMapper = (attributeName, syntax, newMethodName) =>
                    {
                        if (SyntaxWrapperUtilities.IsNoValueReturn(syntax, out var returnTypeDescription))
                        {
                            throw new NETMetaCoderException(
                                $"Method \"{syntax.Identifier}\" cannot be wrapped because it returns " +
                                $"\"{returnTypeDescription}\" but it was requested that it returns a value.");
                        }

                        return new string[0];
                    }
                }
            }
            .ToImmutableList();
    }
}
