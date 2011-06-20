using HL7TestClient.Interfaces;

namespace HL7TestClient.PersonRegistry
{
    public partial class MCCI_MT000300UV01Device : IDevice
    {
        public MCCI_MT000300UV01Device()
        {
        }

        public MCCI_MT000300UV01Device(EntityClassDevice classCode, EntityDeterminerSpecific determinerCode, II id)
            : this()
        {
            this.classCode = classCode;
            this.determinerCode = determinerCode;
            this.id = new[] {id};
        }

        ILocatedEntity[] IDevice.asLocatedEntity
        {
            get { return asLocatedEntityField; }
            set { asLocatedEntityField = (MCCI_MT000300UV01LocatedEntity[])value; }
        }

        IAgent IDevice.asAgent
        {
            get { return asAgentField; }
            set { asAgentField = (MCCI_MT000300UV01Agent)value; }
        }
    }
}
