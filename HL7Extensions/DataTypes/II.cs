namespace HL7TestClient.PersonRegistry
{
    public partial class II : ANY
    {
        public II()
        {
        }

        public II(string root, string extension)
            : this()
        {
            this.root = root;
            this.extension = extension;
        }

        public II(string root, string extension, string assigningAuthorityName)
            : this(root, extension)
        {
            this.assigningAuthorityName = assigningAuthorityName;
        }

        public static II Unknown()
        {
            return new II {nullFlavor = NullFlavor.UNK, nullFlavorSpecified = true};
        }
    }
}
