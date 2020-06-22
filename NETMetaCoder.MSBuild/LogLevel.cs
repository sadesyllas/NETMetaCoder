namespace NETMetaCoder.MSBuild
{
    /// <summary>
    /// The logging level to use when printing messages produced by this library.
    /// </summary>
    public enum LogLevel : byte
    {
        /// <summary>
        /// No informational logs are printed.
        /// </summary>
        Quiet,

        /// <summary>
        /// Only information in the form of a summary and once-off messages are logged.
        /// </summary>
        Normal,

        /// <summary>
        /// Messages produced by looping over collections and detailed descriptions of this library's actions are
        /// logged.
        /// </summary>
        Loud
    }
}
