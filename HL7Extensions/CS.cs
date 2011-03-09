namespace HL7TestClient.PersonRegistry
{
    /// <summary>
    /// Represents a value that typically is drawn from a code system, and might also identify the code system.
    /// </summary>
    public partial class CS
    {
        public CS()
        {
        }

        public CS(string code)
        {
            this.code = code;
        }
    }
}
