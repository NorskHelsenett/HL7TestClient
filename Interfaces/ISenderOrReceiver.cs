using HL7TestClient.PersonRegistry;

namespace HL7TestClient.Interfaces
{
    public interface ISenderOrReceiver
    {
        CS[] realmCode { get; set; }
        II typeId { get; set; }
        II[] templateId { get; set; }
        TEL telecom { get; set; }
        IDevice device { get; set; }
        CommunicationFunctionType typeCode { get; set; }
    }
}
