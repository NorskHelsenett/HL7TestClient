using HL7TestClient.PersonRegistry;

namespace HL7TestClient.Interfaces
{
    public interface ILocatedEntity
    {
        CS[] realmCode { get; set; }
        II typeId { get; set; }
        II[] templateId { get; set; }
        IPlace location { get; set; }
        NullFlavor nullFlavor { get; set; }
        RoleClassLocatedEntity classCode { get; set; }
    }
}
