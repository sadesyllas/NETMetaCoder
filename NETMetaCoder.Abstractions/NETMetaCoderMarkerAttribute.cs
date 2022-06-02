// ReSharper disable UnusedAutoPropertyAccessor.Global

using System;

namespace NETMetaCoder.Abstractions
{
    /// <summary>
    /// A marker attribute that is applied by <c>NETMetaCoder</c> to generated wrapper methods.
    ///
    /// Each instance of this attribute holds a unique id.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    // ReSharper disable once InconsistentNaming
    public sealed class NETMetaCoderMarkerAttribute : Attribute
    {
        /// <summary>
        /// Constructs a <see cref="NETMetaCoderMarkerAttribute"/> instance with a new <see cref="Guid"/>.
        /// </summary>
        public NETMetaCoderMarkerAttribute(string id)
        {
            Id = Guid.Parse(id);
        }

        /// <summary>
        /// A <see cref="Guid"/> which identifies a single <see cref="NETMetaCoderMarkerAttribute"/> instance.
        /// </summary>
        public Guid Id { get; }
    }
}
