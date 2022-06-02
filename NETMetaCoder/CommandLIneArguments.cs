using CommandLine;

namespace NETMetaCoder;

public sealed class CommandLIneArguments
{
    [Option('p', "project", Required = true, HelpText = "The compiled project's root directory.")]
    public string ProjectRootDirectory { get; set; } = string.Empty;

    [Option('o', "output", Required = true,
        HelpText = "The output directory, where the rewritten files will be stored.")]
    public string OutputDirectoryName { get; set; } = string.Empty;

    [Option('u', "units-input", Required = true,
        HelpText = "The path to the input file where the compilation units will be read from.")]
    public string CompilationUnitsInput { get; set; } = string.Empty;

    [Option('U', "units-output", Required = true,
        HelpText = "The path to the output file where the new compilation units will be written to.")]
    public string CompilationUnitsOutput { get; set; } = string.Empty;

    [Option('v', "verbose", Required = false,
        HelpText = "The output directory, where the rewritten files will be stored.")]
    public byte LogLevel { get; set; }
}
