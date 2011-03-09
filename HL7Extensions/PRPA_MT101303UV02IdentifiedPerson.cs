using HL7TestClient.Interfaces;

namespace HL7TestClient.PersonRegistry
{
    public partial class PRPA_MT101303UV02IdentifiedPerson : IIdentifiedPerson
    {
        IPerson IIdentifiedPerson.identifiedPerson
        {
            get { return identifiedPerson; }
            set { identifiedPerson = (PRPA_MT101303UV02Person)value; }
        }
    }
}
