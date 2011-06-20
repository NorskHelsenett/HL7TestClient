using HL7TestClient.Interfaces;

namespace HL7TestClient.PersonRegistry
{
    public partial class MCCI_MT000100UV01Agent : IAgent
    {
        IOrganization IAgent.representedOrganization
        {
            get { return representedOrganizationField; }
            set { representedOrganizationField = (MCCI_MT000100UV01Organization) value; }
        }
    }
}
