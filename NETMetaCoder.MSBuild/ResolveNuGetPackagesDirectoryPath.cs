using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace NETMetaCoder.MSBuild
{
    /// <summary>
    /// An MSBuild task that resolves the path to the NuGet packages directory.
    /// </summary>
    public sealed class ResolveNuGetPackagesDirectoryPath : Task
    {
        /// <summary>
        /// The path to the directory where the caller resides.
        /// </summary>
        [Required]
        public string SearchStartDirectory { get; set; }

        /// <summary>
        /// The path to the directory where the NuGet packages are stored.
        /// </summary>
        [Output]
        public ITaskItem[] NuGetPackagesDirectoryPath { get; set; }

        /// <inheritdoc cref="Task.Execute"/>
        public override bool Execute()
        {
            if (!Directory.Exists(SearchStartDirectory))
            {
                Log.LogError($"{nameof(SearchStartDirectory)} is not a valid directory.");

                return false;
            }

            var currentPath = SearchStartDirectory.TrimEnd(Path.DirectorySeparatorChar);

            while (true)
            {
                var currentPathDirectoryName = Path.GetFileName(currentPath);

                if (currentPathDirectoryName.ToLowerInvariant().Equals("packages"))
                {
                    NuGetPackagesDirectoryPath = new ITaskItem[] {new TaskItem(currentPath)};

                    return true;
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
        }
    }
}
