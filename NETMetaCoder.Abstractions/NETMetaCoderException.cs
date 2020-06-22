using System;

namespace NETMetaCoder.Abstractions
{
    /// <summary>
    /// An exception meant to be thrown for errors that occur while <c>NETMetaCoder</c> processes a compilation unit.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public sealed class NETMetaCoderException : Exception
    {
        /// <summary>
        /// Constructs a new <see cref="NETMetaCoderException"/> instance.
        /// </summary>
        /// <param name="message"></param>
        public NETMetaCoderException(string message) : base(message)
        {
        }
    }
}
