using HL7TestClient.PersonRegistry;

namespace HL7TestClient.Constants
{
    /// <summary>
    /// A HL7 system always runs in one of the following modes.
    /// Each incoming message must declare what the expected mode of the receiving system is (in a sense, what the intent of the message is).
    /// The system will reject messages that indicate a mode that is different from the system's mode.
    /// </summary>
    public static class ProcessingCode
    {
        public static CS Test() { return new CS("T"); }
        public static CS Debugging() { return new CS("D"); }
        public static CS Production() { return new CS("P"); }
    }
}
