using HL7TestClient.PersonRegistry;

namespace HL7TestClient.Interfaces
{
    public interface IPlace
    {
        CS[] realmCode { get; set; }
        II typeId { get; set; }
        II[] templateId { get; set; }
        II[] id { get; set; }
        EN[] name { get; set; }
        TEL[] telecom { get; set; }
        NullFlavor nullFlavor { get; set; }
        EntityClassPlace classCode { get; set; }
        EntityDeterminerSpecific determinerCode { get; set; }
    }
}
