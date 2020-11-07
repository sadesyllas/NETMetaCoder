using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace NETMetaCoder.MSBuild
{
    /// <summary>
    /// An MSBuild task that resolves the paths of the required NETMetaCoder libraries.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public sealed class ResolveNETMetaCoderLibraryPaths : Task
    {
        private static readonly IImmutableSet<string> TargetDlls = new HashSet<string>
        {
            "Microsoft.CodeAnalysis.dll",
            "Microsoft.CodeAnalysis.CSharp.dll",
            "Newtonsoft.Json.dll",
            "NETMetaCoder.dll",
            "NETMetaCoder.Abstractions.dll",
            "NETMetaCoder.SyntaxWrappers.dll"
        }.ToImmutableHashSet();

        /// <summary>
        /// The path to the directory where the caller resides.
        /// </summary>
        [Required]
        // ReSharper disable once InconsistentNaming
        public string MSBuildTargetsDirectoryPath { get; set; }

        /// <summary>
        /// The version of the libraries, for which to find the paths.
        /// </summary>
        [Required]
        public string Version { get; set; }

        /// <summary>
        /// The target framework moniker to search for, when searching for NETMetaCoder libraries.
        /// </summary>
        [Required]
        public string TargetFrameworkMoniker { get; set; }

        /// <summary>
        /// The paths to the required NETMetaCoder libraries.
        /// </summary>
        [Output]
        // ReSharper disable once InconsistentNaming
        public ITaskItem[] NETMetaCoderLibraryPaths { get; set; }

        /// <inheritdoc cref="Task.Execute"/>
        public override bool Execute()
        {
            if (!Directory.Exists(MSBuildTargetsDirectoryPath))
            {
                Log.LogError($"{nameof(MSBuildTargetsDirectoryPath)} is not a valid directory.");

                return false;
            }

            var msBuildTargetsDirectoryParentPath = Directory
                .GetParent(MSBuildTargetsDirectoryPath.TrimEnd(Path.DirectorySeparatorChar))
                .ToString();

            var libraryDirectoryPathExample = Path.GetDirectoryName(Directory
                .GetFiles(msBuildTargetsDirectoryParentPath, "NETMetaCoder.MSBuild.dll", SearchOption.AllDirectories)
                .FirstOrDefault());

            if (libraryDirectoryPathExample == null)
            {
                Log.LogError("Could not get a library directory path example.");

                return false;
            }

            var currentPath = libraryDirectoryPathExample.TrimEnd(Path.DirectorySeparatorChar);
            string nuGetPackagesDirectoryPath;

            while (true)
            {
                var currentPathDirectoryName = Path.GetFileName(currentPath);

                if (currentPathDirectoryName.ToLowerInvariant().Equals("packages"))
                {
                    nuGetPackagesDirectoryPath = currentPath;

                    break;
                }

                try
                {
                    var parentPath = Directory.GetParent(currentPath)?.ToString();

                    if (parentPath == null || parentPath == currentPath)
                    {
                        Log.LogError("Could not resolve the path to the NuGet packages directory.");

                        return false;
                    }

                    currentPath = parentPath;
                }
                catch (Exception exception)
                {
                    Log.LogErrorFromException(exception, true);

                    return false;
                }
            }

            NETMetaCoderLibraryPaths = Directory
                .GetFiles(nuGetPackagesDirectoryPath, "*.dll", SearchOption.AllDirectories)
                .Where(path => TargetDlls.Contains(Path.GetFileName(path)))
                .Select(GetNormalizedRootedPath)
                .Where(path => !path.ToLowerInvariant().Contains("netmetacoder.msbuild"))
                .Where(path =>
                    !path.Contains("NETMetaCoder") || path.Contains($"{Version}{Path.DirectorySeparatorChar}"))
                .Where(path => path.Contains(
                    $"{Path.DirectorySeparatorChar}{TargetFrameworkMoniker}{Path.DirectorySeparatorChar}"))
                .GroupBy(Path.GetFileName)
                .Select(pathGroup =>
                    pathGroup.OrderByDescending(path => Assembly.LoadFile(path).GetName().Version).First())
                .ToImmutableHashSet()
                .Select(path => (ITaskItem) new TaskItem(path))
                .ToArray();

            return true;
        }

        /// <summary>
        /// Turn <paramref name="path"/> into a rooted path and replace all <c>..</c> fragments by removing the path
        /// fragment that each <c>..</c> path fragment refers to.
        /// </summary>
        /// <param name="path"></param>
        /// <exception cref="ArgumentNullException"></exception>
        private static string GetNormalizedRootedPath(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            var pathParts = Path.GetFullPath(path).Split(Path.PathSeparator);

            for (var i = 0; i < pathParts.Length; i++)
            {
                if (pathParts[i] == "..")
                {
                    pathParts[i] = null;
                    pathParts[i - 1] = null;
                }
            }

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            return Path.Combine(pathParts.Where(pathPart => pathPart != null).ToArray());
        }
    }
}
