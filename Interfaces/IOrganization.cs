using HL7TestClient.PersonRegistry;

namespace HL7TestClient.Interfaces
{
    public interface IOrganization
    {
        CS[] realmCode { get; set; }
        II typeId { get; set; }
        II[] templateId { get; set; }
        II[] id { get; set; }
        EN[] name { get; set; }
        TEL[] telecom { get; set; }
        COCT_MT040203UV09NotificationParty notificationParty { get; set; }
        NullFlavor nullFlavor { get; set; }
        EntityClassOrganization classCode { get; set; }
        EntityDeterminerSpecific determinerCode { get; set; }
    }
}
