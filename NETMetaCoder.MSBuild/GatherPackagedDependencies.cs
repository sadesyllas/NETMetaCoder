// ReSharper disable UnusedAutoPropertyAccessor.Global

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace NETMetaCoder.MSBuild
{
    /// <summary>
    /// An MSBuild task that gathers dependencies that are necessary for bundling this library.
    /// </summary>
    public sealed class GatherPackagedDependencies : Task
    {
        private static readonly string[] TargetDlls = new[]
        {
            "Microsoft.CodeAnalysis.dll",
            "Microsoft.CodeAnalysis.CSharp.dll",
            "Newtonsoft.Json.dll",
            "NETMetaCoder.dll",
            "NETMetaCoder.Abstractions.dll",
            "NETMetaCoder.SyntaxWrappers.dll"
        };

        /// <summary>
        /// A <c>;</c> separated list of paths to DLLs that are referenced by this library.
        /// </summary>
        [Required]
        public string ReferencedDlls { get; set; }

        /// <summary>
        /// The DLLs to package as dependencies of this library.
        /// </summary>
        [Output]
        public ITaskItem[] DllsToPackage { get; set; }

        /// <inheritdoc cref="Task.Execute"/>
        public override bool Execute()
        {
            var dllPathsToPackage = new List<ITaskItem>();

            foreach (var path in ReferencedDlls.Split(';').Select(path => path.Trim()))
            {
                if (TargetDlls.Contains(Path.GetFileName(path)))
                {
                    dllPathsToPackage.Add(new TaskItem(path));
                }
            }

            DllsToPackage = dllPathsToPackage.ToArray();

            return true;
        }
    }
}
