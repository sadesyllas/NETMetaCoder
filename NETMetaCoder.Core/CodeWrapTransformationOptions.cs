using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NETMetaCoder.Abstractions;

namespace NETMetaCoder.Core
{
    /// <summary>
    /// This type represents the options passed to <see cref="CodeTransformer"/>, in order to process a compilation
    /// unit.
    /// </summary>
    /// <seealso cref="CodeTransformer"/>
    public struct CodeWrapTransformationOptions
    {
        private readonly ImmutableDictionary<string, int> _attributeOrder;

        /// <summary>
        /// Constructs a new <see cref="CodeWrapTransformationOptions"/> instance.
        /// </summary>
        /// <param name="fileBasePath"></param>
        /// <param name="outputBasePath"></param>
        /// <param name="outputDirectoryName"></param>
        /// <param name="syntaxPerAttribute"></param>
        /// <param name="eol"></param>
        public CodeWrapTransformationOptions(string fileBasePath, string outputBasePath, string outputDirectoryName,
            IImmutableDictionary<AttributeDescriptor,
                    (SyntaxList<UsingDirectiveSyntax> Usings,
                    PropertySyntaxGenerator PropertySyntaxGenerator,
                    IImmutableList<SyntaxWrapper> SyntaxWrappers)>
                syntaxPerAttribute,
            string eol = "\n")
        {
            FileBasePath = fileBasePath;
            OutputBasePath = outputBasePath;
            OutputDirectoryName = outputDirectoryName;
            EndOfLine = eol;

            var orderedAttributeDescriptors = syntaxPerAttribute.Keys
                .OrderBy(attributeDescriptor => attributeDescriptor.Order)
                .Distinct()
                .ToArray();

            _attributeOrder = orderedAttributeDescriptors.ToImmutableDictionary(
                attributeDescriptor => attributeDescriptor.Name, attributeDescriptor => attributeDescriptor.Order);

            var orderedAttributeData = orderedAttributeDescriptors
                .Select(attributeDescriptor => syntaxPerAttribute[attributeDescriptor])
                .ToArray();

            AttributeNames = orderedAttributeDescriptors
                .Select(attributeDescriptor => attributeDescriptor.Name)
                .ToImmutableList();

            var usings = new Dictionary<string, SyntaxList<UsingDirectiveSyntax>>();
            var propertySyntaxGenerators = new Dictionary<string, PropertySyntaxGenerator>();
            var preExpressionMappers = new Dictionary<string, List<MethodSyntaxGenerator>>();
            var postExpressionsMappers = new Dictionary<string, List<MethodSyntaxGenerator>>();

            for (var i = 0; i < orderedAttributeDescriptors.Length; i++)
            {
                var attributeName = orderedAttributeDescriptors[i].Name;
                var attributeData = orderedAttributeData[i];

                usings.Add(attributeName, attributeData.Usings);

                propertySyntaxGenerators.Add(attributeName, attributeData.PropertySyntaxGenerator);

                {
                    if (!preExpressionMappers.TryGetValue(attributeName, out var list))
                    {
                        list = attributeData.SyntaxWrappers
                            .Where(syntaxWrapper => syntaxWrapper.PreMapper != null)
                            .Select(syntaxWrapper => syntaxWrapper.PreMapper)
                            .ToList();
                    }

                    preExpressionMappers.Add(attributeName, list);
                }

                {
                    if (!postExpressionsMappers.TryGetValue(attributeName, out var list))
                    {
                        list = attributeData.SyntaxWrappers
                            .Where(syntaxWrapper => syntaxWrapper.PostMapper != null)
                            .Select(syntaxWrapper => syntaxWrapper.PostMapper)
                            .ToList();
                    }

                    postExpressionsMappers.Add(attributeName, list);
                }
            }

            Usings = usings.ToImmutableDictionary();

            PropertySyntaxGenerators = propertySyntaxGenerators.ToImmutableDictionary();

            PreExpressionMappers = preExpressionMappers.ToImmutableDictionary(data => data.Key,
                data => (IImmutableList<MethodSyntaxGenerator>) data.Value.ToImmutableList());

            PostExpressionMappers = postExpressionsMappers.ToImmutableDictionary(data => data.Key,
                data => (IImmutableList<MethodSyntaxGenerator>) data.Value.ToImmutableList());
        }

        /// <summary>
        /// The path to a directory where the <see cref="OutputDirectoryName"/> directory will be created and the output
        /// of <see cref="CodeTransformer.Wrap"/> will be stored.
        /// </summary>
        /// <seealso cref="OutputDirectoryName"/>
        public string OutputBasePath { get; }

        /// <summary>
        /// The name of the directory in <see cref="OutputBasePath"/>, where the output of
        /// <see cref="CodeTransformer.Wrap"/> will be stored.
        /// </summary>
        /// <seealso cref="OutputBasePath"/>
        public string OutputDirectoryName { get; }

        /// <summary>
        /// The path to the directory where the search for <c>*.cs</c> files will be made.
        /// </summary>
        public string FileBasePath { get; private set; }

        /// <summary>
        /// The EOL character sequence to use for the generated code files.
        /// </summary>
        public string EndOfLine { get; }

        /// <summary>
        /// The names of the targeted attributes which will cause a compilation unit to be rewritten.
        /// </summary>
        public IImmutableList<string> AttributeNames { get; }

        /// <summary>
        /// The using declarations to write in the rewritten compilation unit.
        /// </summary>
        public IImmutableDictionary<string, SyntaxList<UsingDirectiveSyntax>> Usings { get; }

        /// <summary>
        /// Anonymous functions, keyed by an attribute name, that produce property declaration syntax nodes.
        ///
        /// The produces properties are part of the code that wraps calls to methods of the compilation unit.
        /// </summary>
        /// <seealso cref="PropertySyntaxGenerator"/>
        public IImmutableDictionary<string, PropertySyntaxGenerator> PropertySyntaxGenerators { get; }

        /// <summary>
        /// Anonymous functions, keyed by an attribute name, that produce the new syntax with which a method is wrapped.
        /// </summary>
        /// <remarks>
        /// The syntax produced by these is place before the call to the wrapped method.
        ///
        /// Together with <see cref="PostExpressionMappers"/>, they wrap the call to the wrapped method.
        /// </remarks>
        /// <seealso cref="MethodSyntaxGenerator"/>
        public IImmutableDictionary<string, IImmutableList<MethodSyntaxGenerator>> PreExpressionMappers { get; }

        /// <summary>
        /// Anonymous functions, keyed by an attribute name, that produce the new syntax with which a method is wrapped.
        /// </summary>
        /// <remarks>
        /// The syntax produced by these is place after the call to the wrapped method.
        ///
        /// Together with <see cref="PreExpressionMappers"/>, they wrap the call to the wrapped method.
        /// </remarks>
        /// <seealso cref="MethodSyntaxGenerator"/>
        public IImmutableDictionary<string, IImmutableList<MethodSyntaxGenerator>> PostExpressionMappers { get; }

        /// <summary>
        /// The path to a directory, as a combination of <see cref="OutputBasePath"/> and
        /// <see cref="OutputDirectoryName"/>, where the rewritten code files will be stored.
        /// </summary>
        public string OutputDirectory => Path.Combine(OutputBasePath, OutputDirectoryName);

        /// <summary>
        /// Selects using declaration syntax nodes from <see cref="Usings"/>, based on a list of attribute names.
        /// </summary>
        /// <param name="attributeNames"></param>
        [Pure]
        public IImmutableList<UsingDirectiveSyntax> SelectUsings(ImmutableHashSet<string> attributeNames) =>
            Usings
                .Where(data => attributeNames.Any(attributeName =>
                    attributeName.Contains(data.Key) || attributeName.Contains(data.Key + Constants.AttributeSuffix)))
                .SelectMany(data => data.Value).ToImmutableList();

        /// <summary>
        /// Selects property declaration generators from <see cref="PropertySyntaxGenerators"/>, based on a list of
        /// attribute names.
        /// </summary>
        /// <param name="attributeNames"></param>
        [Pure]
        public IImmutableList<(string AttributeName, PropertySyntaxGenerator SyntaxGenerator)>
            SelectPropertySyntaxGenerators(ImmutableHashSet<string> attributeNames) =>
            PropertySyntaxGenerators
                .Where(data => attributeNames.Any(attributeName =>
                    attributeName.Contains(data.Key) || attributeName.Contains(data.Key + Constants.AttributeSuffix)))
                .Select(data => (data.Key, data.Value))
                .ToImmutableList();

        /// <summary>
        /// Selects expression syntax fragments from <see cref="PreExpressionMappers"/>, based on a list of attribute
        /// names.
        /// </summary>
        /// <param name="attributeNames"></param>
        [Pure]
        public IImmutableList<(string AttributeName, IImmutableList<MethodSyntaxGenerator> SyntaxGenerators)>
            SelectPreExpressionMappers(ImmutableHashSet<string> attributeNames)
        {
            var mappers = PreExpressionMappers
                .Where(data => attributeNames.Any(attributeName =>
                    attributeName.Contains(data.Key) || attributeName.Contains(data.Key + Constants.AttributeSuffix)))
                .Select(data => (data.Key, data.Value))
                .ToList();

            SortMappers(ref mappers);

            return mappers.ToImmutableList();
        }

        /// <summary>
        /// Selects expression syntax fragments from <see cref="PostExpressionMappers"/>, based on a list of attribute
        /// names.
        /// </summary>
        /// <param name="attributeNames"></param>
        [Pure]
        public IImmutableList<(string AttributeName, IImmutableList<MethodSyntaxGenerator> SyntaxGenerators)>
            SelectPostExpressionMappers(ImmutableHashSet<string> attributeNames)
        {
            var mappers = PostExpressionMappers
                .Where(data => attributeNames.Any(attributeName =>
                    attributeName.Contains(data.Key) || attributeName.Contains(data.Key + Constants.AttributeSuffix)))
                .Select(data => (data.Key, data.Value))
                .ToList();

            SortMappers(ref mappers);

            return mappers.ToImmutableList();
        }

        private void SortMappers(ref List<(string Key, IImmutableList<MethodSyntaxGenerator> Value)> mappers)
        {
            var attributeOrder = _attributeOrder;

            mappers.Sort((a, b) =>
            {
                var (aKey, _) = a;
                var (bKey, _) = b;

                return attributeOrder[aKey].CompareTo(attributeOrder[bKey]);
            });
        }
    }
}
