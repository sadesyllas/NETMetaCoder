// ReSharper disable TemplateIsNotCompileTimeConstantProblem
// ReSharper disable LogMessageIsSentenceProblem

using System.Collections.Immutable;
using NETMetaCoder.Abstractions;
using NETMetaCoder.Core;
using NETMetaCoder.SyntaxWrappers;
using Serilog;

namespace NETMetaCoder;

public static class ProjectSyntaxRewriter
{
    public static List<(int index, string filePath)> Process(
        string projectRootDirectory, string[] compilationFilePaths, string outputDirectoryName,
        LogLevel logLevel)
    {
        if (!Directory.Exists(projectRootDirectory))
        {
            throw new ArgumentException(
                $"[NETMetaCoder] \"{projectRootDirectory}\" is not a directory", nameof(projectRootDirectory));
        }

        var compilationUnits = new List<(int, string)>();
        var newCompilationUnits = new List<(int, string)>();

        for (var i = 0; i < compilationFilePaths.Length; i++)
        {
            if (compilationFilePaths[i].EndsWith("AssemblyAttributes.cs") ||
                compilationFilePaths[i].EndsWith("AssemblyInfo.cs"))
            {
                if (logLevel >= LogLevel.Loud)
                {
                    Log.Information($"[NETMetaCoder] Passthrough compilation unit: {compilationFilePaths[i]}");
                }

                newCompilationUnits.Add((i, compilationFilePaths[i]));
            }
            else
            {
                compilationUnits.Add((i, compilationFilePaths[i]));
            }
        }

        var outputBasePath = Path.Combine(projectRootDirectory, "obj");

        var compilationUnitDescriptors = compilationUnits
            .Select(descriptor =>
            {
                var (i, compilationFilePath) = descriptor;
                var filePath = Path.Combine(projectRootDirectory, compilationFilePath);

                return (i, compilationFilePath, filePath);
            })
            .ToImmutableArray();

        IImmutableList<AttributeDescriptor> attributeDescriptors;

        attributeDescriptors = AttributesIndexReader.Read(projectRootDirectory);

        var wrappers = attributeDescriptors.Select(attributeDescriptor =>
            {
                var (usings, propertySyntaxGenerator, statementWrappers) =
                    SyntaxWrappersIndex.WrapperTypes[attributeDescriptor.WrapperType];

                return (attributeDescriptor, (usings, propertySyntaxGenerator, statementWrappers));
            })
            .ToImmutableDictionary(
                kv =>
                {
                    var (a, _) = kv;

                    return a;
                },
                kv =>
                {
                    var (_, b) = kv;

                    return b;
                });

        if (!wrappers.Any())
        {
            Log.Warning(
                "[NETMetaCoder] No attribute names have been configured for wrapping / " +
                "Consider removing the reference to NETMetaCoder");

            return new List<(int index, string filePath)>();
        }

        var codeWrapTransformationOptions = new CodeWrapTransformationOptions(
            projectRootDirectory,
            outputBasePath,
            outputDirectoryName,
            wrappers);

        if (logLevel >= LogLevel.Normal)
        {
            Log.Information(
                "[NETMetaCoder] Using options:\n" +
                $"[NETMetaCoder] \t{nameof(codeWrapTransformationOptions.FileBasePath)}=" +
                $"{codeWrapTransformationOptions.FileBasePath}\n" +
                $"[NETMetaCoder] \t{nameof(codeWrapTransformationOptions.OutputBasePath)}=" +
                $"{codeWrapTransformationOptions.OutputBasePath}\n" +
                $"[NETMetaCoder] \t{nameof(codeWrapTransformationOptions.OutputDirectoryName)}=" +
                $"{codeWrapTransformationOptions.OutputDirectoryName}");

            Log.Information("[NETMetaCoder] Searching for attributes");

            foreach (var attributeName in codeWrapTransformationOptions.AttributeNames)
            {
                Log.Information($"[NETMetaCoder] \t{attributeName}");
            }
        }

        var codeTransformer = new CodeTransformer(codeWrapTransformationOptions);
        var atLeastOneTransformation = false;

        var index = 0;
        foreach (var (i, compilationFilePath, filePath) in compilationUnitDescriptors)
        {
            index++;

            if (logLevel >= LogLevel.Loud)
            {
                Log.Information($"[NETMetaCoder] Checking the code syntax in {compilationFilePath} " +
                                $"(index/total={index}/{compilationUnitDescriptors.Length}, filepath={filePath})");
            }

            CodeTransformationResult codeTransformationResult;

            codeTransformationResult = codeTransformer.Wrap(filePath);

            if (codeTransformationResult.TransformationOccured)
            {
                atLeastOneTransformation = true;

                if (logLevel >= LogLevel.Normal)
                {
                    Log.Information($"[NETMetaCoder] Rewritten the code syntax in {compilationFilePath} " +
                                    $"(index/total={index}/{compilationUnitDescriptors.Length})");
                }

                var mirrorFilePath =
                    PathHelper.GetRelativePath(projectRootDirectory, codeTransformationResult.MirrorFilePath);

                newCompilationUnits.Add((i, mirrorFilePath));

                var companionFilePath =
                    PathHelper.GetRelativePath(projectRootDirectory,
                        codeTransformationResult.CompanionFilePath);

                newCompilationUnits.Add((i, companionFilePath));

                if (logLevel >= LogLevel.Loud)
                {
                    Log.Information(
                        "[NETMetaCoder] Changed compilation units:\n" +
                        $"[NETMetaCoder] \t{mirrorFilePath} ({codeTransformationResult.MirrorFilePath})\n" +
                        $"[NETMetaCoder] \t{companionFilePath} ({codeTransformationResult.CompanionFilePath})");
                }
            }
            else
            {
                if (logLevel >= LogLevel.Loud)
                {
                    Log.Information(
                        $"[NETMetaCoder] Unchanged compilation unit: {compilationFilePath} ({filePath})");
                }

                newCompilationUnits.Add((i, compilationFilePath));
            }
        }

        if (!atLeastOneTransformation)
        {
            Log.Information(
                "[NETMetaCoder] No code syntax transformations were made / " +
                "Consider removing the reference to NETMetaCoder");
        }

        return newCompilationUnits;
    }
}
