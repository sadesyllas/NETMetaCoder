// ReSharper disable UnusedAutoPropertyAccessor.Global

using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace NETMetaCoder.MSBuild
{
    /// <summary>
    /// An MSBuild task that copies assembly files to a given destination, to help with bundling this library's
    /// resources.
    /// </summary>
    public sealed class CopyAssemblyFiles : Task
    {
        /// <summary>
        /// The assembly files to copy.
        /// </summary>
        [Required]
        public ITaskItem[] AssemblyFilePaths { get; set; }

        /// <summary>
        /// The destination directory where the assembly files are to be copied.
        /// </summary>
        [Required]
        public string DestinationDirectory { get; set; }

        /// <inheritdoc cref="Task.Execute"/>
        public override bool Execute()
        {
            try
            {
                foreach (var assemblyFilePath in AssemblyFilePaths)
                {
                    if (!File.Exists(assemblyFilePath.ItemSpec))
                    {
                        throw new ArgumentException($"\"{assemblyFilePath.ItemSpec}\" is not a file.");
                    }

                    var destinationFilePath =
                        Path.Combine(DestinationDirectory, Path.GetFileName(assemblyFilePath.ItemSpec));

                    if (File.Exists(destinationFilePath))
                    {
                        File.Delete(destinationFilePath);
                    }

                    // ReSharper disable once AssignNullToNotNullAttribute
                    File.Copy(assemblyFilePath.ItemSpec, destinationFilePath);
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
