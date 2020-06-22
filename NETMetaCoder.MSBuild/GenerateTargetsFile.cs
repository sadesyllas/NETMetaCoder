// ReSharper disable UnusedAutoPropertyAccessor.Global

using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace NETMetaCoder.MSBuild
{
    /// <summary>
    /// An MSBuild task that reads file <c>NETMetaCoder.MSBuild-template.targets</c> and produces file
    /// <c>NETMetaCoder.MSBuild.targets</c>.
    ///
    /// The generated file is used by projects that depend on this library's NuGet package to load the necessary MSBuild
    /// tasks.
    /// </summary>
    public sealed class GenerateTargetsFile : Task
    {
        /// <summary>
        /// The file path to <c>NETMetaCoder.MSBuild-template.targets</c>.
        /// </summary>
        [Required]
        public string TargetsFilePath { get; set; }

        /// <summary>
        /// The name of this library's package.
        /// </summary>
        [Required]
        public string PackageId { get; set; }

        /// <summary>
        /// This library's package version.
        /// </summary>
        [Required]
        public string Version { get; set; }

        /// <summary>
        /// The target framework moniker of this library.
        /// </summary>
        [Required]
        public string TargetFrameworkMoniker { get; set; }

        /// <inheritdoc cref="Task.Execute"/>
        public override bool Execute()
        {
            if (!File.Exists(TargetsFilePath))
            {
                throw new FileNotFoundException($"\"{TargetsFilePath}\" is not a file.");
            }

            File.WriteAllText(
                TargetsFilePath.Replace("-template", ""),
                File.ReadAllText(TargetsFilePath)
                    .Replace("${PACKAGE_ID}", PackageId.ToLowerInvariant())
                    .Replace("${VERSION}", Version)
                    .Replace("${TARGET_FRAMEWORK_MONIKER}", TargetFrameworkMoniker));

            return true;
        }
    }
}
