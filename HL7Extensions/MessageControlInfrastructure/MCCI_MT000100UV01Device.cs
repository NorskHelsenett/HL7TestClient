using HL7TestClient.Interfaces;

namespace HL7TestClient.PersonRegistry
{
    public partial class MCCI_MT000100UV01Device : IDevice
    {
        ILocatedEntity[] IDevice.asLocatedEntity
        {
            get { return asLocatedEntityField; }
            set { asLocatedEntityField = (MCCI_MT000100UV01LocatedEntity[])value; }
        }

        IAgent IDevice.asAgent
        {
            get { return asAgentField; }
            set { asAgentField = (MCCI_MT000100UV01Agent)value; }
        }
    }
}
