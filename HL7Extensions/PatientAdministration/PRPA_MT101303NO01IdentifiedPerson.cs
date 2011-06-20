using HL7TestClient.Interfaces;

namespace HL7TestClient.PersonRegistry
{
    public partial class PRPA_MT101303NO01IdentifiedPerson : IIdentifiedPerson
    {
        IPerson IIdentifiedPerson.identifiedPerson
        {
            get { return identifiedPerson; }
            set { identifiedPerson = (PRPA_MT101303NO01Person)value; }
        }
    }
}
