namespace HL7TestClient.PersonRegistry
{
    /// <summary>
    /// A single part of a family name ("etternavn").
    /// </summary>
    public partial class enfamily
    {
        public enfamily()
        {
        }

        public enfamily(string text)
            : this()
        {
            this.Text = new[] {text};
        }

        public enfamily(string text, bool isMiddleName)
            : this(text)
        {
            if (isMiddleName)
                this.qualifier = new[] {EntityNamePartQualifier.MID};
        }
    }
}
