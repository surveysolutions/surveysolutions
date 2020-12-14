namespace ddidotnet
{
    /// <summary>
    ///     Properties that pertain to the DDI document in general.
    /// </summary>
    public class DdiDocument
    {
        /// <summary>
        ///     Copyright statement for the work at the appropriate level.
        /// </summary>
        public string Copyright;

        /// <summary>
        ///     Full authoritative title for the work at the appropriate level: marked-up document; marked-up document source;
        ///     study; other material(s) related to study description; other material(s) related to study. The study title will in
        ///     most cases be identical to the title for the marked-up document. A full title should indicate the geographic scope
        ///     of the data collection as well as the time period covered.
        /// </summary>
        public string Title;


        internal void WriteToXml(XmlDocumentExt doc)
        {
            var citation = doc.CreateElement("citation");
            doc.DocDscr.AppendChild(citation);

            var titlStmt = doc.CreateElement("titlStmt");
            citation.AppendChild(titlStmt);
            doc.InsertValue(titlStmt, "titl", Title); // INPUT!

            var prodStmt = doc.CreateElement("prodStmt");
            citation.AppendChild(prodStmt);

            doc.InsertValue(prodStmt, "copyright", Copyright); // INPUT!

            var software = doc.CreateElement("software");
            prodStmt.AppendChild(software);
            software.SetAttribute("version", DdiConstants.VerStr);
            software.SetAttribute("date", DdiConstants.DateStr);
            software.AppendChild(doc.CreateTextNode(DdiConstants.SoftId)); // HARDWIRED


            /*
            * SOME FILES MAY HAVE THIS BUT DON'T SEE HOW THIS MAY BE USED
            var fileTxt = doc.CreateElement("fileTxt");
            docDscr.AppendChild(fileTxt);

            var fileCont = doc.CreateElement("fileCont");
            fileTxt.AppendChild(fileCont);
            fileCont.AppendChild(doc.CreateTextNode("1978 Automobile Data")); // INPUT!
            
            var dimensns = doc.CreateElement("dimensns");
            fileTxt.AppendChild(dimensns);

            doc.InsertValue(dimensns, "caseQnty", "0"); // INPUT!
            doc.InsertValue(dimensns, "varQnty", "3"); // INPUT!
            doc.InsertValue(fileTxt, "fileType", "Nesstar 200801"); // HARDWIRED OR UNKNOWN!
            */
            var notes = doc.CreateElement("notes");
            doc.DocDscr.AppendChild(notes);
            notes.AppendChild(doc.CreateTextNode("")); // POTENTIALLY ADD NOTES
        }
    }
}
