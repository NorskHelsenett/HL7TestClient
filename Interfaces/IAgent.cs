using HL7TestClient.PersonRegistry;

namespace HL7TestClient.Interfaces
{
    public interface IAgent
    {
        CS[] realmCode { get; set; }
        II typeId { get; set; }
        II[] templateId { get; set; }
        IOrganization representedOrganization { get; set; }
        NullFlavor nullFlavor { get; set; }
        RoleClassAgent classCode { get; set; }
    }
}
