namespace HL7TestClient.PersonRegistry
{
    /// <summary>
    /// A single part of a given name ("fornavn").
    /// </summary>
    public partial class engiven
    {
        public engiven()
        {
        }

        public engiven(string text)
            : this()
        {
            this.Text = new[] {text};
        }
    }
}
