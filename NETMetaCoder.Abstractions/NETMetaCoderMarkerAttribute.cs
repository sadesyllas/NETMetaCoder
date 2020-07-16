// ReSharper disable UnusedAutoPropertyAccessor.Global

using System;

namespace NETMetaCoder.Abstractions
{
    /// <summary>
    /// A marker attribute that is applied by <c>NETMetaCoder</c> to generated wrapper methods.
    ///
    /// This attribute is meant to hold the instance of the wrapper attribute which caused the original method to be
    /// wrapped.
    ///
    /// There is one instance of a <see cref="NETMetaCoderMarkerAttribute"/> for each such wrapper attribute.
    ///
    /// If needed, this instance is meant to be initialized in the attribute's <see cref="NETMetaCoderAttribute.Init"/>
    /// implementation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    // ReSharper disable once InconsistentNaming
    public sealed class NETMetaCoderMarkerAttribute : Attribute
    {
        private object _wrapperAttribute;
        private object _wrapperAttributeLock = new object();

        /// <summary>
        /// The instance of the attribute which caused the original method to be wrapped.
        ///
        /// If needed, this is meant to be initialized in the attribute's <see cref="NETMetaCoderAttribute.Init"/>
        /// implementation.
        /// </summary>
        public object WrapperAttribute
        {
            get => _wrapperAttribute;
            set
            {
                if (_wrapperAttribute == null)
                {
                    lock (_wrapperAttributeLock)
                    {
                        if (_wrapperAttribute == null)
                        {
                            _wrapperAttribute = value;
                        }
                    }
                }
            }
        }
    }
}
