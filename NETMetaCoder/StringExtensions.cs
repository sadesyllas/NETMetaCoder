using System;
using System.Linq;

namespace NETMetaCoder
{
    /// <summary>
    /// Extension methods for <see cref="string"/>, relevant to the requirements of this library.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Transforms an attribute name so as to append it to a wrapped method's name.
        /// </summary>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static string ToAttributeNameNeedle(this string attributeName)
        {
            if (string.IsNullOrWhiteSpace(attributeName))
            {
                throw new ArgumentException("The attribute name must not be null or whitespace.",
                    nameof(attributeName));
            }

            attributeName = attributeName.Split('.').Last();

            return $"__WrappedBy{attributeName}";
        }
    }
}
