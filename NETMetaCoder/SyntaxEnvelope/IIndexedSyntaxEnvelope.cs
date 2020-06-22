namespace NETMetaCoder.SyntaxEnvelope
{
    /// <summary>
    /// An interface to denote that a syntax envelope also has an index property.
    /// </summary>
    /// <seealso cref="SyntaxEnvelope"/>
    /// <seealso cref="NamespaceSyntaxEnvelope"/>
    /// <seealso cref="ClassOrStructSyntaxEnvelope"/>
    /// <seealso cref="MethodSyntaxEnvelope"/>
    public interface IIndexedSyntaxEnvelope
    {
        /// <summary>
        /// An index to be used when scanning a compilation unit, in order to build a tree of the unit's structure.
        ///
        /// This index gives an identity to a specific syntax node within the built tree.
        /// </summary>
        /// <seealso cref="SyntaxEnvelope"/>
        ushort NodeIndex { get; set; }
    }
}
