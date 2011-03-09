using HL7TestClient.PersonRegistry;

namespace HL7TestClient.Constants
{
    public static class ProcessingCode
    {
        public static CS Test() { return new CS("T"); }
        public static CS Debugging() { return new CS("D"); }
        public static CS Production() { return new CS("P"); }
    }
}
