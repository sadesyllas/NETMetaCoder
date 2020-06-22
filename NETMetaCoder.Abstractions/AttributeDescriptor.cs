using System;

namespace NETMetaCoder.Abstractions
{
    /// <summary>
    /// This type describes an attribute which is expected to be applied on a method declaration.
    ///
    /// It describes how the method is to be wrapped by the <c>NETMetaCoder</c> library.
    /// </summary>
    public sealed class AttributeDescriptor : IEquatable<AttributeDescriptor>
    {
        /// <summary>
        /// Constructs a new <see cref="AttributeDescriptor"/> instance.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="order"></param>
        /// <param name="wrapperType"></param>
        public AttributeDescriptor(string name, int order, string wrapperType)
        {
            Name = name;
            Order = order;
            WrapperType = wrapperType;
        }

        /// <summary>
        /// The name of the attribute to target.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The order with which to apply the syntax rewriting rules which accompany this
        /// <see cref="AttributeDescriptor"/>.
        /// </summary>
        public int Order { get; }

        /// <summary>
        /// The name of a wrapper type as defined in the <c>NETMetaCoder.SyntaxWrappers</c> namespace.
        /// </summary>
        public string WrapperType { get; }

        /// <inheritdoc/>
        public bool Equals(AttributeDescriptor other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Name == other.Name;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is AttributeDescriptor other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
