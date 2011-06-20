using HL7TestClient.PersonRegistry;

namespace HL7TestClient.Interfaces
{
    public interface IDevice
    {
        CS[] realmCode { get; set; }
        II typeId { get; set; }
        II[] templateId { get; set; }
        II[] id { get; set; }
        EN[] name { get; set; }
        ED desc { get; set; }
        IVL_TS existenceTime { get; set; }
        TEL[] telecom { get; set; }
        SC manufacturerModelName { get; set; }
        SC softwareName { get; set; }
        IAgent asAgent { get; set; }
        ILocatedEntity[] asLocatedEntity { get; set; }
        EntityClassDevice classCode { get; set; }
        EntityDeterminerSpecific determinerCode { get; set; }
    }
}
