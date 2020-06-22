// ReSharper disable UnusedAutoPropertyAccessor.Global

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NETMetaCoder.Abstractions;
using NETMetaCoder.SyntaxWrappers;

namespace NETMetaCoder.MSBuild
{
    /// <summary>
    /// An MSBuild task that scans a project that depends on this library and rewrites the syntax where necessary.
    /// </summary>
    /// <seealso cref="AttributesIndexReader"/>
    /// <seealso cref="CodeTransformer"/>
    public sealed class RewriteProjectSyntax : Task
    {
        /// <summary>
        /// The path to the root directory of the project that this library is a dependency of.
        /// </summary>
        [Required]
        public string ProjectRootDirectory { get; set; }

        /// <summary>
        /// The path to the directory where the rewritten syntax will be stored.
        /// </summary>
        [Required]
        public string OutputDirectoryName { get; set; }

        /// <summary>
        /// An array of paths to the code files that are to be compiled, before this library processes any files.
        /// </summary>
        /// <remarks>
        /// This library works by redirecting MSBuild to compile processed files, instead of the original files in the
        /// codebase.
        /// </remarks>
        [Required]
        public ITaskItem[] CompilationUnits { get; set; }

        /// <summary>
        /// The logging level to apply when executing this MSBuild task.
        /// </summary>
        /// <seealso cref="NETMetaCoder.MSBuild.LogLevel"/>
        public byte LogLevel { get; set; }

        /// <summary>
        /// An array of paths to the code files that are to be compiled, after this library has processed a project's
        /// files.
        /// </summary>
        /// <remarks>
        /// This library works by redirecting MSBuild to compile processed files, instead of the original files in the
        /// codebase.
        /// </remarks>
        [Output]
        public ITaskItem[] NewCompilationUnits { get; set; }

        private LogLevel EffectiveLogLevel => (LogLevel) LogLevel;

        /// <inheritdoc cref="Task.Execute"/>
        public override bool Execute()
        {
            try
            {
                if (!Directory.Exists(ProjectRootDirectory))
                {
                    throw new ArgumentException(
                        $"[NETMetaCoder] \"{ProjectRootDirectory}\" is not a directory.", nameof(ProjectRootDirectory));
                }

                var compilationUnits = new List<ITaskItem>();
                var newCompilationUnits = new List<ITaskItem>();

                foreach (var compilationUnit in CompilationUnits)
                {
                    if (compilationUnit.ItemSpec.EndsWith("AssemblyAttributes.cs") ||
                        compilationUnit.ItemSpec.EndsWith("AssemblyInfo.cs"))
                    {
                        if (EffectiveLogLevel >= MSBuild.LogLevel.Loud)
                        {
                            Log.LogMessage(MessageImportance.High,
                                $"[NETMetaCoder] Passthrough compilation unit: {compilationUnit.ItemSpec}.");
                        }

                        newCompilationUnits.Add(compilationUnit);
                    }
                    else
                    {
                        compilationUnits.Add(compilationUnit);
                    }
                }

                var outputBasePath = Path.Combine(ProjectRootDirectory, "obj");

                var compilationUnitDescriptors = compilationUnits
                    .Select(compilationUnit =>
                    {
                        var filePath = Path.Combine(ProjectRootDirectory, compilationUnit.ItemSpec);

                        return (compilationUnit, filePath);
                    })
                    .ToImmutableList();

                IImmutableList<AttributeDescriptor> attributeDescriptors;

                try
                {
                    attributeDescriptors = AttributesIndexReader.Read(ProjectRootDirectory);
                }
                catch (Exception exception)
                {
                    Log.LogErrorFromException(exception, true);

                    return false;
                }

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
                    Log.LogWarning(
                        "[NETMetaCoder] No attribute names have been configured for wrapping. " +
                        "Consider removing the reference to NETMetaCoder.");

                    return true;
                }

                var codeWrapTransformationOptions = new CodeWrapTransformationOptions(
                    ProjectRootDirectory,
                    outputBasePath,
                    OutputDirectoryName,
                    wrappers);

                if (EffectiveLogLevel >= MSBuild.LogLevel.Normal)
                {
                    Log.LogMessage(MessageImportance.High,
                        "[NETMetaCoder] Using options:\n" +
                        $"\t{nameof(codeWrapTransformationOptions.FileBasePath)}=" +
                        $"{codeWrapTransformationOptions.FileBasePath}\n" +
                        $"\t{nameof(codeWrapTransformationOptions.OutputBasePath)}=" +
                        $"{codeWrapTransformationOptions.OutputBasePath}\n" +
                        $"\t{nameof(codeWrapTransformationOptions.OutputDirectoryName)}=" +
                        $"{codeWrapTransformationOptions.OutputDirectoryName}");

                    Log.LogMessage(MessageImportance.High, "[NETMetaCoder] Searching for attributes");

                    foreach (var attributeName in codeWrapTransformationOptions.AttributeNames)
                    {
                        Log.LogMessage(MessageImportance.High, $"\t{attributeName}");
                    }
                }

                var codeTransformer = new CodeTransformer(codeWrapTransformationOptions);
                var atLeastOneTransformation = false;

                foreach (var (compilationUnit, filePath) in compilationUnitDescriptors)
                {
                    if (EffectiveLogLevel >= MSBuild.LogLevel.Loud)
                    {
                        Log.LogMessage(MessageImportance.High,
                            $"[NETMetaCoder] Checking the code syntax in {compilationUnit.ItemSpec} ({filePath}).");
                    }

                    CodeTransformationResult codeTransformationResult;

                    try
                    {
                        codeTransformationResult = codeTransformer.Wrap(filePath);
                    }
                    catch (NETMetaCoderException exception)
                    {
                        Log.LogError($"{filePath}: {exception.Message}");

                        return false;
                    }

                    if (codeTransformationResult.TransformationOccured)
                    {
                        atLeastOneTransformation = true;

                        if (EffectiveLogLevel >= MSBuild.LogLevel.Normal)
                        {
                            Log.LogMessage(MessageImportance.High,
                                $"[NETMetaCoder] Rewritten the code syntax in {compilationUnit.ItemSpec}.");
                        }

                        var mirrorFilePathItemSpec =
                            PathHelper.GetRelativePath(ProjectRootDirectory, codeTransformationResult.MirrorFilePath);

                        newCompilationUnits.Add(new TaskItem(compilationUnit) {ItemSpec = mirrorFilePathItemSpec});

                        var companionFilePathItemSpec =
                            PathHelper.GetRelativePath(ProjectRootDirectory,
                                codeTransformationResult.CompanionFilePath);

                        newCompilationUnits.Add(new TaskItem(compilationUnit) {ItemSpec = companionFilePathItemSpec});

                        if (EffectiveLogLevel >= MSBuild.LogLevel.Loud)
                        {
                            Log.LogMessage(MessageImportance.High,
                                "[NETMetaCoder] Changed compilation units:\n" +
                                $"\t{mirrorFilePathItemSpec} ({codeTransformationResult.MirrorFilePath})\n" +
                                $"\t{companionFilePathItemSpec} ({codeTransformationResult.CompanionFilePath})");
                        }
                    }
                    else
                    {
                        if (EffectiveLogLevel >= MSBuild.LogLevel.Loud)
                        {
                            Log.LogMessage(MessageImportance.High,
                                $"[NETMetaCoder] Unchanged compilation unit: {compilationUnit.ItemSpec} ({filePath}).");
                        }

                        newCompilationUnits.Add(compilationUnit);
                    }
                }

                NewCompilationUnits = newCompilationUnits.ToArray();

                if (!atLeastOneTransformation)
                {
                    Log.LogWarning(
                        "[NETMetaCoder] No code syntax transformations were made. " +
                        "Consider removing the reference to NETMetaCoder.");
                }

                return true;
            }
            catch (Exception exception)
            {
                Log.LogErrorFromException(exception, true);

                return false;
            }
        }
    }
}
