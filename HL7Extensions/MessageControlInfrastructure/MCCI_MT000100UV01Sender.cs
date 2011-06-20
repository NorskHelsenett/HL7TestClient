using HL7TestClient.Interfaces;

namespace HL7TestClient.PersonRegistry
{
    public partial class MCCI_MT000100UV01Sender : ISenderOrReceiver
    {
        IDevice ISenderOrReceiver.device
        {
            get { return deviceField; }
            set { deviceField = (MCCI_MT000100UV01Device)value; }
        }
    }
}
