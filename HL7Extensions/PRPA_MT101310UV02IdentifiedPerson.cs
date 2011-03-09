using HL7TestClient.Interfaces;

namespace HL7TestClient.PersonRegistry
{
    public partial class PRPA_MT101310UV02IdentifiedPerson : IIdentifiedPerson
    {
        IPerson IIdentifiedPerson.identifiedPerson
        {
            get { return identifiedPerson; }
            set { identifiedPerson = (PRPA_MT101310UV02Person)value; }
        }
    }
}
