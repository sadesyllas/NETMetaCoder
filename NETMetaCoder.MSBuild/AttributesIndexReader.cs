using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using NETMetaCoder.Abstractions;
using Newtonsoft.Json.Linq;

namespace NETMetaCoder.MSBuild
{
    /// <summary>
    /// A helper class that finds and reads the attributes index file.
    /// </summary>
    public static class AttributesIndexReader
    {
        private const string AttributesIndexFileName = "NETMetaCoder.Index.json";

        /// <summary>
        /// Reads the attributes index file.
        /// </summary>
        /// <param name="directoryToSearchIn"></param>
        /// <returns></returns>
        /// <exception cref="NETMetaCoderException"></exception>
        public static IImmutableList<AttributeDescriptor> Read(string directoryToSearchIn)
        {
            var attributesIndexFilePath = GetAttributesIndexFilePath(directoryToSearchIn);

            if (attributesIndexFilePath == null)
            {
                throw new NETMetaCoderException(
                    $"Could not find \"{AttributesIndexFileName}\" searching from the provided directory " +
                    $"{directoryToSearchIn} upwards.");
            }

            JObject json;

            try
            {
                json = JObject.Parse(File.ReadAllText(attributesIndexFilePath));
            }
            catch (Exception exception)
            {
                throw new NETMetaCoderException(
                    $"Exception thrown while parsing \"{attributesIndexFilePath}\":\n" +
                    $"[Message]\n{exception.Message}\n[StackTrace]\n{exception.StackTrace}");
            }

            var attributesIndex = json.ToObject<AttributesIndex>() ??
                throw new NETMetaCoderException(
                    $"Could not convert the contents of \"{attributesIndexFilePath}\" into an instance of " +
                    $"{nameof(AttributesIndex)}");

            return attributesIndex.Attributes
                .Select(attribute => new AttributeDescriptor(attribute.Name, attribute.Order, attribute.Wrapper))
                .ToImmutableList();
        }

        private static string GetAttributesIndexFilePath(string directoryToSearchIn)
        {
            if (!Directory.Exists(directoryToSearchIn))
            {
                throw new NETMetaCoderException($"\"{directoryToSearchIn}\" is not a directory.");
            }

            var attributesIndexFilePath = Path.Combine(directoryToSearchIn, AttributesIndexFileName);
            var found = false;

            while (true)
            {
                if (File.Exists(attributesIndexFilePath))
                {
                    found = true;

                    break;
                }

                var parentDirectory = Directory.GetParent(attributesIndexFilePath).Parent?.FullName;

                if (!Directory.Exists(parentDirectory))
                {
                    break;
                }

                attributesIndexFilePath = Path.Combine(parentDirectory, AttributesIndexFileName);
            }

            return found ? attributesIndexFilePath : null;
        }
    }
}
