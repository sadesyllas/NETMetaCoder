// ReSharper disable UnusedAutoPropertyAccessor.Global

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

// ReSharper disable MemberCanBePrivate.Global

namespace NETMetaCoder.MSBuild
{
    /// <summary>
    /// An MSBuild task that scans a project that depends on this library and rewrites the syntax where necessary.
    /// </summary>
    /// <remarks>
    /// Also check <c>NETMetaCoder.AttributesIndexReader</c> and <c>NETMetaCoder.Core.CodeTransformer</c>.
    /// </remarks>
    public sealed class RewriteProjectSyntax : Task
    {
        /// <summary>
        /// The directory containing the targets file, from which this <see cref="Task"/> has been called.
        /// </summary>
        [Required]
        public string TargetsFileDirectory { get; set; }

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
        /// <remarks>
        /// It corresponds to values from the <c>NETMetaCoder.LogLevel</c>Z enumeration.
        /// </remarks>
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

        /// <inheritdoc />
        public override bool Execute()
        {
            if (!Directory.Exists(TargetsFileDirectory))
            {
                Log.LogError("[NETMetaCoder] The provided directory does not exist");

                return false;
            }

#if DEBUG
            var executablePath =
                Path.Combine(TargetsFileDirectory, "..", "..", "..", "..", "NETMetaCoder", "bin", "Debug", "net6.0",
                    "NETMetaCoder");
#else
            var executablePath =
                Environment.GetEnvironmentVariable("NETMETACODER_PATH") ??
                Path.Combine(Path.GetPathRoot(Environment.CurrentDirectory), "NETMetaCoder", "NETMetaCoder");
#endif

            if (!File.Exists(executablePath))
            {
                var exeExecutablePath = $"{executablePath}.exe";

                if (File.Exists(exeExecutablePath))
                {
                    executablePath = exeExecutablePath;
                }
                else
                {
                    Log.LogError("[NETMetaCoder] The path to the NETMetaCoder executable could not be found at: " +
                                 $"{executablePath}(.exe)");

                    return false;
                }
            }

            if (!Directory.Exists(ProjectRootDirectory))
            {
                Log.LogError($"[NETMetaCoder] \"{ProjectRootDirectory}\" is not a directory.");

                return false;
            }

            var inputFilePath = Path.Combine(ProjectRootDirectory, "obj", "NETMetaCoder_input.txt");

            using (var inputFile = File.Open(inputFilePath, FileMode.Create))
            {
                foreach (var compilationUnit in CompilationUnits)
                {
                    var bytes = Encoding.UTF8.GetBytes($"{compilationUnit.ItemSpec}\n");
                    inputFile.Write(bytes, 0, bytes.Length);
                }
            }

            Log.LogMessage(MessageImportance.High,
                $"[NETMetaCoder] Written {CompilationUnits.Length} compilation units in {inputFilePath}");

            var outputFilePath = Path.Combine(ProjectRootDirectory, "obj", "NETMetaCoder_output.txt");

            try
            {
                var args = $"-p \"{ProjectRootDirectory}\" -o \"{OutputDirectoryName}\" -u {inputFilePath} " +
                           $"-U {outputFilePath} -v {LogLevel}";

                Log.LogMessage(MessageImportance.High,
                    $"[NETMetaCoder] Running NETMetaCoder with command: \"{executablePath}\" {args}");

                var proc = Process.Start(new ProcessStartInfo
                {
                    FileName = executablePath,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                });

                if (proc == null)
                {
                    Log.LogError($"[NETMetaCoder] Could not run NETMetaCoder executable: {executablePath}");

                    return false;
                }

                Log.LogMessage(MessageImportance.High,
                    "[NETMetaCoder] Reading NETMetaCoder output through stdout and stderr");

                var stdout = proc.StandardOutput.ReadToEnd();
                var stderr = proc.StandardError.ReadToEnd();

                proc.WaitForExit();

                if (!string.IsNullOrWhiteSpace(stdout))
                {
                    Log.LogMessage(MessageImportance.High, stdout);
                }

                if (!string.IsNullOrWhiteSpace(stderr))
                {
                    Log.LogError(stderr);
                }


                if (proc.ExitCode != 0)
                {
                    Log.LogError(
                        $"[NETMetaCoder] The NETMetaCoder executable did not run successfully (exit code: {proc.ExitCode})");

                    return false;
                }

                Log.LogMessage(MessageImportance.High,
                    $"[NETMetaCoder] Reading new compilation units from {outputFilePath}");

                if (!File.Exists(outputFilePath))
                {
                    Log.LogError(($"[NETMetaCoder] Output path {outputFilePath} does not exist"));

                    return false;
                }

                var newCompilationUnits = new List<ITaskItem>();

                foreach (var line in Regex.Split(File.ReadAllText(outputFilePath), "\r?\n")
                             .Where(line => !line.StartsWith("[NETMetaCoder]") && line.Trim() != string.Empty))
                {
                    var parts = line.Split(new[] { ',' }, 2);

                    var index = ulong.Parse(parts[0]);
                    var filePath = parts[1];

                    newCompilationUnits.Add(new TaskItem(CompilationUnits[index]) { ItemSpec = filePath });
                }

                NewCompilationUnits = newCompilationUnits.ToArray();

                Log.LogMessage(MessageImportance.High,
                    $"[NETMetaCoder] Read {NewCompilationUnits.Length} new compilation units from {outputFilePath}");

                return true;
            }
            catch (Exception exception)
            {
                Log.LogErrorFromException(exception, true, true, null);

                return false;
            }
            finally
            {
                if (File.Exists(inputFilePath))
                {
                    Log.LogMessage(MessageImportance.High,
                        $"[NETMetaCoder] Removing the compilation units input file: {inputFilePath}");

                    File.Delete(inputFilePath);
                }

                if (File.Exists(outputFilePath))
                {
                    Log.LogMessage(MessageImportance.High,
                        $"[NETMetaCoder] Removing the compilation units output file: {outputFilePath}");

                    File.Delete(outputFilePath);
                }
            }
        }
    }
}
