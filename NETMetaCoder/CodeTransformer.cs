using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace NETMetaCoder
{
    /// <summary>
    /// This class provides the core functionality of the library.
    ///
    /// It's focal point is method <see cref="Wrap"/> which is responsible for running the code wrapping logic for a
    /// compilation unit.
    /// </summary>
    /// <seealso cref="CodeWrapTransformationOptions"/>
    /// <seealso cref="SyntaxScanner"/>
    /// <seealso cref="SyntaxRewriter"/>
    public sealed class CodeTransformer
    {
        private CodeWrapTransformationOptions _options;
        private readonly bool _writeOutput;

        /// <summary>
        /// Creates a new <see cref="CodeTransformer"/> instance.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="writeOutput"></param>
        /// <seealso cref="CodeWrapTransformationOptions"/>
        public CodeTransformer(CodeWrapTransformationOptions options, bool writeOutput = true)
        {
            _options = options;
            _writeOutput = writeOutput;

            if (Directory.Exists(_options.OutputDirectory))
            {
                Directory.Delete(_options.OutputDirectory, true);
            }
        }

        /// <summary>
        /// This function receives a file path and potentially produces a code syntax transformation of that file's
        /// code.
        /// </summary>
        /// <param name="filePath"></param>
        /// <remarks>
        /// This method takes the following steps:
        /// 1. Is parses its syntax tree.
        /// 2. It scans the parsed syntax tree into a <see cref="SyntaxEnvelope"/>, keeping only the parts relevant to
        ///   the functionality of this library.
        /// 3. If the file needs to be rewritten, then the original file is changed so that its code can be wrapped and
        ///   companion file with the wrapping code is created.
        /// </remarks>
        /// <seealso cref="SyntaxScanner"/>
        /// <seealso cref="SyntaxRewriter"/>
        public CodeTransformationResult Wrap(string filePath)
        {
            var relativeFilePath = PathHelper.GetRelativePath(_options.FileBasePath, filePath);
            var code = File.ReadAllText(filePath);
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var syntaxScan = SyntaxScanner.ScanSyntaxTree(syntaxTree, _options.AttributeNames);
            var rewrittenSyntax = SyntaxRewriter.RewriteSyntaxTree(syntaxTree, _options.AttributeNames, syntaxScan);
            var mirrorFilePath = Path.Combine(_options.OutputDirectory, relativeFilePath);

            // ReSharper disable once AssignNullToNotNullAttribute
            var companionFilePath = Path.Combine(
                Path.GetDirectoryName(mirrorFilePath),
                Path.GetFileNameWithoutExtension(mirrorFilePath) +
                ".Companion" +
                Path.GetExtension(mirrorFilePath));

            if (rewrittenSyntax.HasChanges)
            {
                var rewrittenSyntaxCode = rewrittenSyntax.SyntaxTree.GetRoot().ToFullString();

                if (_writeOutput)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(mirrorFilePath) ??
                        throw new DirectoryNotFoundException($"The directory of {mirrorFilePath} does not exist."));

                    File.WriteAllText(mirrorFilePath, rewrittenSyntaxCode);
                }

                var builtSyntax = new SyntaxBuilder(syntaxScan, ref _options).Build();

                var newCode = builtSyntax.GetRoot().NormalizeWhitespace(eol: _options.EndOfLine).ToFullString() +
                    _options.EndOfLine;

                if (_writeOutput)
                {
                    File.WriteAllText(companionFilePath, newCode);
                }
            }

            return new CodeTransformationResult
            {
                TransformationOccured = rewrittenSyntax.HasChanges,
                MirrorFilePath = mirrorFilePath,
                CompanionFilePath = companionFilePath
            };
        }
    }
}
