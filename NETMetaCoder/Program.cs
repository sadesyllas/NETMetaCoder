// ReSharper disable TemplateIsNotCompileTimeConstantProblem
// ReSharper disable LogMessageIsSentenceProblem

using System.Text;
using System.Text.RegularExpressions;
using CommandLine;
using NETMetaCoder;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Debug)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

CommandLIneArguments options = new();

var optionsResult = Parser.Default.ParseArguments<CommandLIneArguments>(args).MapResult(
    opts =>
    {
        options = opts;
        return 0;
    },
    _ => 1);

if (optionsResult != 0)
{
    return optionsResult;
}

if (!File.Exists(options.CompilationUnitsInput))
{
    Log.Error($"{options.CompilationUnitsInput} is not a file");

    return 1;
}

var filePaths = Regex.Split(File.ReadAllText(options.CompilationUnitsInput), "\r?\n")
    .Where(line => line.Trim() != string.Empty).ToArray();

var results = ProjectSyntaxRewriter.Process(options.ProjectRootDirectory, filePaths, options.OutputDirectoryName,
    (LogLevel)options.LogLevel);

using (var outputFile = File.Open(options.CompilationUnitsOutput, FileMode.Create))
{
    foreach (var (i, filePath) in results)
    {
        var bytes = Encoding.UTF8.GetBytes($"{i},{filePath}\n");
        outputFile.Write(bytes, 0, bytes.Length);
    }
}

Log.CloseAndFlush();

return 0;
