namespace NETMetaCoder.Core
{
    /// <summary>
    /// The result of making a code transformation pass through a compilation unit.
    /// </summary>
    public ref struct CodeTransformationResult
    {
        /// <summary>
        /// True is the code in the processed compilation unit was transformed.
        /// </summary>
        public bool TransformationOccured { get; set; }

        /// <summary>
        /// The file path to the rewritten code file, which holds the original code of the compilation unit.
        /// </summary>
        public string MirrorFilePath { get; set; }

        /// <summary>
        /// The file path to the companion code file, which holds the newly produced code, that serves as a proxy to the
        /// functionality of the compilation unit.
        /// </summary>
        public string CompanionFilePath { get; set; }
    }
}
