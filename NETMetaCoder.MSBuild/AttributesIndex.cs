using System.Collections.Generic;
using Newtonsoft.Json;

namespace NETMetaCoder.MSBuild
{
    /// <summary>
    /// A type that represents the expected JSON format in an attributes index file.
    ///
    /// The attributes index file maps attribute names to wrapper types, along with metadata.
    ///
    /// These attribute names must match the attribute names used on methods which are expected to be wrapped by
    /// <c>NETMetaCoder</c>'s generated code.
    /// </summary>
    public sealed class AttributesIndex
    {
        /// <summary>
        /// The collection of <see cref="Attribute"/> as expected to be found in the attributes index file.
        /// </summary>
        [JsonProperty("attributes", Required = Required.Always)]
        public IEnumerable<Attribute> Attributes { get; set; }

        /// <summary>
        /// The type representing a single attribute description, as expected to be found in the attributes index file.
        /// </summary>
        public sealed class Attribute
        {
            /// <summary>
            /// The attribute name, as it is expected to match in the code that is being rewritten.
            /// </summary>
            [JsonProperty("name", Required = Required.Always)]
            public string Name { get; set; }

            /// <summary>
            /// The order with which to wrap the code, as defined by this attribute's <see cref="Wrapper"/>.
            ///
            /// This has an effect only when target multiple attributes the generate wrapper code.
            /// </summary>
            [JsonProperty("order", Required = Required.Always)]
            public int Order { get; set; }

            /// <summary>
            /// The name of the wrapper code which defines how the wrapper syntax will be generated for this attribute.
            /// </summary>
            [JsonProperty("wrapper", Required = Required.Always)]
            public string Wrapper { get; set; }
        }
    }
}
