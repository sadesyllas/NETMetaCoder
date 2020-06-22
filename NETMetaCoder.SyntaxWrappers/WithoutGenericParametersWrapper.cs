using System.Collections.Generic;
using System.Collections.Immutable;
using NETMetaCoder.Abstractions;

namespace NETMetaCoder.SyntaxWrappers
{
    /// <summary>
    /// A wrapper type that checks that the method being wrapped does not depend on generic parameters.
    /// </summary>
    /// <remarks>
    /// It is meant to be used with <see cref="CommonWrapper"/>.
    /// </remarks>
    /// <seealso cref="CommonWrapper"/>
    /// <seealso cref="MustReturnValueWrapper"/>
    public class WithoutGenericParametersWrapper
    {
        /// <inheritdoc cref="CommonWrapper.SyntaxWrappers"/>
        public static IImmutableList<SyntaxWrapper> SyntaxWrappers { get; } = new List<SyntaxWrapper>
            {
                new SyntaxWrapper
                {
                    PreMapper = (attributeName, syntax, newMethodName) =>
                    {
                        var genericTypeParameters = syntax.GetGenericTypeParameters();

                        if (syntax.ReturnType.IsGenericWithGenericTypeParameter(genericTypeParameters))
                        {
                            throw new NETMetaCoderException(
                                $"Method \"{syntax.Identifier}\" cannot be wrapped because it has unbound generic " +
                                "parameters but it was requested that it has none.");
                        }

                        return new string[0];
                    }
                }
            }
            .ToImmutableList();
    }
}
