using HL7TestClient.Interfaces;

namespace HL7TestClient.PersonRegistry
{
    public partial class MCCI_MT000300UV01Receiver : ISenderOrReceiver
    {
        IDevice ISenderOrReceiver.device
        {
            get { return deviceField; }
            set { deviceField = (MCCI_MT000300UV01Device)value; }
        }
    }
}
