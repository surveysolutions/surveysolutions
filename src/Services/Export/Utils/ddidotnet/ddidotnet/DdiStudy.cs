namespace ddidotnet
{
    /// <summary>
    ///     Properties describing the study
    /// </summary>
    public class DdiStudy
    {
        /// <summary>
        ///     Unique producer's or archive's number.
        ///     May consist of the following characters: letters, numbers, MINUS and DOT.
        ///     Must start with a letter.
        /// </summary>
        public string Idno;

        /// <summary>
        ///     Title of the study
        /// </summary>
        public string Title;

        internal void WriteToXml(XmlDocumentExt doc)
        {
            var citation = doc.CreateElement("citation");
            doc.StdyDscr.AppendChild(citation);

            var titlStmt = doc.CreateElement("titlStmt");
            citation.AppendChild(titlStmt);

            doc.InsertValue(titlStmt, "titl", Title); // INPUT!
            doc.InsertValue(titlStmt, "IDNO", Idno); // INPUT!

            var prodStmt = doc.CreateElement("prodStmt");
            citation.AppendChild(prodStmt);

            var software = doc.CreateElement("software");
            prodStmt.AppendChild(software);
            software.SetAttribute("version", DdiConstants.VerStr);
            software.SetAttribute("date", DdiConstants.DateStr);
            software.AppendChild(doc.CreateTextNode(DdiConstants.SoftId)); // HARDWIRED
        }
    }
}
