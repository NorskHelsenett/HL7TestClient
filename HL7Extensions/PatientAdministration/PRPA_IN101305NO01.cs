using System.Linq;
using HL7TestClient.Interfaces;

namespace HL7TestClient.PersonRegistry
{
    public partial class PRPA_IN101305NO01 : IRequestMessage
    {
        /// <summary>
        /// Returns the ParameterList that is contained within this message.
        /// Caution: Does not check any objects along the path for null.
        /// </summary>
        public PRPA_MT101306NO01ParameterList ParameterList { get { return controlActProcess.queryByParameter.parameterList; } }

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
