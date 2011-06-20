using HL7TestClient.Interfaces;

namespace HL7TestClient.PersonRegistry
{
    public partial class MCCI_MT000300UV01LocatedEntity : ILocatedEntity
    {
        IPlace ILocatedEntity.location
        {
            get { return locationField; }
            set { locationField = (MCCI_MT000300UV01Place)value; }
        }
    }
}
