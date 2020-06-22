using System;

namespace NETMetaCoder.Abstractions
{
    /// <summary>
    /// The flavor of the <see cref="ObsoleteAttribute"/> found in a method declaration, if any.
    /// </summary>
    public enum MethodObsoletion : byte
    {
        /// <summary>
        /// No <see cref="ObsoleteAttribute"/> found.
        /// </summary>
        NoObsoletion,

        /// <summary>
        /// An <see cref="ObsoleteAttribute"/> attribute was found and it has been set to emit a warning.
        /// </summary>
        ObsoleteWithWarning,

        /// <summary>
        /// An <see cref="ObsoleteAttribute"/> attribute was found and it has been set to emit an error.
        /// </summary>
        ObsoleteWithError
    }
}
