using HL7TestClient.PersonRegistry;

namespace HL7TestClient.Interfaces
{
    public interface IResponseMessage : IMessage
    {
        //TODO: Many of these properties can also be moved into IMessage, provided that we create interfaces like IReceiver, ISender etc.
        MCCI_MT000300UV01RespondTo[] respondTo { get; set; }
        MCCI_MT000300UV01AttentionLine[] attentionLine { get; set; }
        MCCI_MT000300UV01Acknowledgement[] acknowledgement { get; set; }
    }
}
