using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace NETMetaCoder
{
    /// <summary>
    /// A helper class for manipulating file paths.
    /// </summary>
    public static class PathHelper
    {
        /// <summary>
        /// Get the path to a file, relative to another path.
        /// </summary>
        /// <param name="relativeTo"></param>
        /// <param name="path"></param>
        public static string GetRelativePath(string relativeTo, string path)
        {
            return GetRelativePath(relativeTo, path, StringComparison);
        }

        private static string GetRelativePath(string relativeTo, string path, StringComparison comparisonType)
        {
            if (relativeTo == null)
            {
                throw new ArgumentNullException(nameof(relativeTo));
            }

            if (relativeTo.AsSpan().IsEmpty)
            {
                throw new ArgumentException("The relative path must not be empty.", nameof(relativeTo));
            }

            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (path.AsSpan().IsEmpty)
            {
                throw new ArgumentException("The relative path must not be empty.", nameof(path));
            }

            System.Diagnostics.Debug.Assert(comparisonType == StringComparison.Ordinal ||
                                            comparisonType == StringComparison.OrdinalIgnoreCase);

            relativeTo = Path.GetFullPath(relativeTo);
            path = Path.GetFullPath(path);

            // Need to check if the roots are different- if they are we need to return the "to" path.
            if (!AreRootsEqual(relativeTo, path, comparisonType))
            {
                return path;
            }

            var commonLength =
                GetCommonPathLength(relativeTo, path, comparisonType == StringComparison.OrdinalIgnoreCase);

            // If there is nothing in common they can't share the same root, return the "to" path as is.
            if (commonLength == 0)
            {
                return path;
            }

            // Trailing separators aren't significant for comparison
            var relativeToLength = relativeTo.Length;

            if (EndsInDirectorySeparator(relativeTo.AsSpan()))
            {
                relativeToLength--;
            }

            var pathEndsInSeparator = EndsInDirectorySeparator(path.AsSpan());
            var pathLength = path.Length;

            if (pathEndsInSeparator)
            {
                pathLength--;
            }

            // If we have effectively the same path, return "."
            if (relativeToLength == pathLength && commonLength >= relativeToLength)
            {
                return ".";
            }

            var sb = new StringBuilder(Math.Max(relativeTo.Length, path.Length));

            // Add parent segments for segments past the common on the "from" path
            if (commonLength < relativeToLength)
            {
                sb.Append("..");

                for (var i = commonLength + 1; i < relativeToLength; i++)
                {
                    if (IsDirectorySeparatorChar(relativeTo[i]))
                    {
                        sb.Append(Path.DirectorySeparatorChar);
                        sb.Append("..");
                    }
                }
            }
            else if (IsDirectorySeparatorChar(path[commonLength]))
            {
                // No parent segments and we need to eat the initial separator
                //  (C:\Foo C:\Foo\Bar case)
                commonLength++;
            }

            // Now add the rest of the "to" path, adding back the trailing separator
            var differenceLength = pathLength - commonLength;

            if (pathEndsInSeparator)
            {
                differenceLength++;
            }

            if (differenceLength > 0)
            {
                if (sb.Length > 0)
                {
                    sb.Append(Path.DirectorySeparatorChar);
                }

                sb.Append(path, commonLength, differenceLength);
            }

            return sb.ToString();
        }

        private static bool EndsInDirectorySeparator(ReadOnlySpan<char> path)
            => path.Length > 0 && IsDirectorySeparatorChar(path[path.Length - 1]);

        private static StringComparison StringComparison =>
            IsCaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

        private static bool IsCaseSensitive
        {
            get
            {
#if PLATFORM_WINDOWS || PLATFORM_OSX
                return false;
#else
                return true;
#endif
            }
        }

        private static bool AreRootsEqual(string first, string second, StringComparison comparisonType)
        {
            var firstRootLength = GetRootLength(first.AsSpan());
            var secondRootLength = GetRootLength(second.AsSpan());

            return firstRootLength == secondRootLength &&
                   string.Compare(
                       strA: first,
                       indexA: 0,
                       strB: second,
                       indexB: 0,
                       length: firstRootLength,
                       comparisonType: comparisonType) ==
                   0;
        }

        private static int GetRootLength(ReadOnlySpan<char> path) =>
            path.Length > 0 && IsDirectorySeparatorChar(path[0]) ? 1 : 0;

        private static int GetCommonPathLength(string first, string second, bool ignoreCase)
        {
            var commonChars = EqualStartingCharacterCount(first, second, ignoreCase: ignoreCase);

            // If nothing matches
            if (commonChars == 0)
            {
                return commonChars;
            }

            // Or we're a full string and equal length or match to a separator
            if (commonChars == first.Length &&
                (commonChars == second.Length || IsDirectorySeparatorChar(second[commonChars])))
            {
                return commonChars;
            }

            if (commonChars == second.Length && IsDirectorySeparatorChar(first[commonChars]))
            {
                return commonChars;
            }

            // It's possible we matched somewhere in the middle of a segment e.g. C:\Foodie and C:\Foobar.
            while (commonChars > 0 && first[commonChars - 1] != Path.DirectorySeparatorChar)
            {
                commonChars--;
            }

            return commonChars;
        }

        private static unsafe int EqualStartingCharacterCount(string first, string second, bool ignoreCase)
        {
            if (string.IsNullOrEmpty(first) || string.IsNullOrEmpty(second))
            {
                return 0;
            }

            var commonChars = 0;

            fixed (char* f = first)
            fixed (char* s = second)
            {
                char* l = f;
                char* r = s;
                char* leftEnd = l + first.Length;
                char* rightEnd = r + second.Length;

                while (l != leftEnd &&
                       r != rightEnd &&
                       (*l == *r || (ignoreCase && char.ToUpperInvariant((*l)) == char.ToUpperInvariant((*r)))))
                {
                    commonChars++;
                    l++;
                    r++;
                }
            }

            return commonChars;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsDirectorySeparatorChar(char c) => c == Path.DirectorySeparatorChar;
    }
}
