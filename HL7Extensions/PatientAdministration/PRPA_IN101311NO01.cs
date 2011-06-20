using System.Linq;
using HL7TestClient.Interfaces;

namespace HL7TestClient.PersonRegistry
{
    public partial class PRPA_IN101311NO01 : IRequestMessage
    {
        ISenderOrReceiver IMessage.sender
        {
            get { return sender; }
            set { sender = (MCCI_MT000100UV01Sender)value; }
        }

        ISenderOrReceiver[] IMessage.receiver
        {
            get { return receiver; }
            set { receiver = value == null ? null : value.Cast<MCCI_MT000100UV01Receiver>().ToArray(); }
        }
    }
}
