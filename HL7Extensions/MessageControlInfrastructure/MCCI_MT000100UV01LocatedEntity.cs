using HL7TestClient.Interfaces;

namespace HL7TestClient.PersonRegistry
{
    public partial class MCCI_MT000100UV01LocatedEntity : ILocatedEntity
    {
        IPlace ILocatedEntity.location
        {
            get { return locationField; }
            set { locationField = (MCCI_MT000100UV01Place)value; }
        }
    }
}
