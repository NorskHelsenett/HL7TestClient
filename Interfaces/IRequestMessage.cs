using HL7TestClient.PersonRegistry;

namespace HL7TestClient.Interfaces
{
    public interface IRequestMessage : IMessage
    {
        INT sequenceNumber { get; set; }
        MCCI_MT000100UV01RespondTo[] respondTo { get; set; }
        MCCI_MT000100UV01AttentionLine[] attentionLine { get; set; }
    }
}
