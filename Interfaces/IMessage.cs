using HL7TestClient.PersonRegistry;

namespace HL7TestClient.Interfaces
{
    public interface IMessage
    {
        CS[] realmCode { get; set; }
        II typeId { get; set; }
        II[] templateId { get; set; }
        II id { get; set; }
        TS creationTime { get; set; }
        ST securityText { get; set; }
        CS versionCode { get; set; }
        II interactionId { get; set; }
        II[] profileId { get; set; }
        CS processingCode { get; set; }
        CS processingModeCode { get; set; }
        CS acceptAckCode { get; set; }
        ED[] attachmentText { get; set; }
        ISenderOrReceiver sender { get; set; }
        ISenderOrReceiver[] receiver { get; set; }
        string ITSVersion { get; set; }
    }
}
