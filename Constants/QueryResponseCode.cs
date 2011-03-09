namespace NHN.HL7.Constants
{
    /// <summary>
    /// Valid values for the 'code' attribute of the 'queryResponseCode' element, as per section 4.4.2 of http://hl7.ihelse.net/Dokument/HL7v3_ImplementationGuide_3.0c.doc.
    /// </summary>
    public static class QueryResponseCode
    {
        public const string Ok = "OK";
        public const string NoResultsFound = "NF";
        public const string QueryParameterError = "QE";
    }
}
