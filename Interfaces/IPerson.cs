using HL7TestClient.PersonRegistry;

namespace HL7TestClient.Interfaces
{
    /// <summary>
    /// This interface abstracts out the common parts of the nearly-equal PRPA_MT101303UV02Person and PRPA_MT101310UV02Person classes.
    /// </summary>
    public interface IPerson
    {
        CS[] realmCode { get; set; }
        II typeId { get; set; }
        II[] templateId { get; set; }
        II[] id { get; set; }
        PN[] name { get; set; }
        ED desc { get; set; }
        TEL[] telecom { get; set; }
        CE administrativeGenderCode { get; set; }
        TS birthTime { get; set; }
        BL deceasedInd { get; set; }
        TS deceasedTime { get; set; }
        BL multipleBirthInd { get; set; }
        INT multipleBirthOrderNumber { get; set; }
        BL organDonorInd { get; set; }
        AD[] addr { get; set; }
        CE maritalStatusCode { get; set; }
        NullFlavor nullFlavor { get; set; }
        EntityClass classCode { get; set; }
        EntityDeterminer determinerCode { get; set; }
    }
}
