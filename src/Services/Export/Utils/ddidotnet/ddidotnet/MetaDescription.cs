using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ddidotnet
{
    /// <summary>
    ///     Collection of properties describing the datasets and variables.
    /// </summary>
    public class MetaDescription : IMetaDescription
    {
        private const string Str1 = "http://www.icpsr.umich.edu/DDI"; // xmlns
        private const string Str2 = "http://www.w3.org/2001/XMLSchema-instance"; // xmlns:xsi

        private const string
            Str3 =
                "http://www.icpsr.umich.edu/DDI http://www.icpsr.umich.edu/DDI/Version1-2-2.xsd"; // xsi:SchemaLocation

        /// <summary>
        ///     Group of properties related to the whole DDI document
        /// </summary>
        private readonly DdiDocument _document;

        /// <summary>
        ///     Group of properties related to the study
        /// </summary>
        private readonly DdiStudy _study;

        internal List<DdiDataFile> DataFiles;

        /// <summary>
        ///     Optional file template.
        ///     Normally output creates a complete file from properties determined
        ///     in the meta description, and this property can be left empty. But
        ///     if this property is non-empty, it must contain a properly XML-formatted
        ///     content, which is reused by the library in the output.
        ///     The template must: contain a properly formatted header declaration
        ///     (_xml_, _codeBook_), document (_docDscr_) and study (_stdyDscr_)
        ///     sections. The template may contain an empty data description section
        ///     (_dataDscr_) or no such section at all.
        ///     If the template is specified, no values are written to the document
        ///     (_docDscr_) or study (_stdyDscr_) sections and their original content
        ///     is retained intact regardless of the properties of the meta description.
        ///     \b Example Using a template
        ///     \snippet Example2.cs Using a template
        /// </summary>
        public string TemplateXml;

        /// <summary>
        ///     Constructor
        ///     \b Example Creating new data description
        ///     \snippet Example2.cs Creating new data description
        /// </summary>
        public MetaDescription()
        {
            _document = new DdiDocument();
            _study = new DdiStudy();
            DataFiles = new List<DdiDataFile>();
        }

        /// <summary>
        ///     Group of properties related to the whole DDI document
        /// </summary>
        public DdiDocument Document => _document;

        /// <summary>
        ///     Group of properties related to the study
        /// </summary>
        public DdiStudy Study => _study;


        /// <summary>
        ///     Add a new file to the DDI description
        /// </summary>
        /// <param name="name">Name of the file being added</param>
        /// <returns>Reference to the newly created data file descriptor.</returns>
        /// \b Example Adding files to DDI output
        /// \snippet Example2.cs Adding files to DDI output
        public DdiDataFile AddDataFile(string name)
        {
            var dataFile = new DdiDataFile(DataFiles.Count)
            {
                Name = name
            };

            DataFiles.Add(dataFile);
            return dataFile;
        }

        /// <summary>
        ///     Writes DDI to a stream
        /// </summary>
        /// <param name="stream">Stream to be written to.</param>
        /// \b Example Writing to a stream
        /// \snippet Example2.cs Writing to a stream
        public void WriteXml(Stream stream)
        {
            var xml = CreateXml(TemplateXml);
            if (string.IsNullOrEmpty(TemplateXml))
            {
                Document.WriteToXml(xml);
                Study.WriteToXml(xml);
            }

            //xml.DataDscr = xml.CreateElement("dataDscr");
            foreach (var file in DataFiles) file.WriteToXml(xml);
            //xml.CodeBook.AppendChild(xml.DataDscr);
            xml.Save(stream);
        }

        /// <summary>
        ///     Writes DDI to a file
        /// </summary>
        /// <param name="filename">File name to be written to. If file exists it will be overwritten.</param>
        /// \b Example Writing to a file
        /// \snippet Example2.cs Writing to a file
        public void WriteXml(string filename)
        {
            using (var fs = new FileStream(filename, FileMode.Create))
            {
                WriteXml(fs);
                fs.Close();
            }
        }


        internal static XmlDocumentExt CreateXml(string templateContent)
        {
            if (!string.IsNullOrEmpty(templateContent))
            {
                var doc = new XmlDocumentExt();
                doc.LoadXml(templateContent);

                doc.CodeBook = doc.DocumentElement;
                if (doc.CodeBook == null) return CreateXmlBlank();
                if (doc.CodeBook.Name != "codeBook") return CreateXmlBlank();
                doc.DataDscr = doc.CodeBook["dataDscr"];
                if (doc.DataDscr == null)
                {
                    doc.DataDscr = doc.CreateElement("dataDscr");
                    doc.CodeBook.AppendChild(doc.DataDscr);
                }

                return doc;
            }

            return CreateXmlBlank();
        }

        internal static XmlDocumentExt CreateXmlBlank()
        {
            var doc = new XmlDocumentExt();
            var xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            var root = doc.DocumentElement;
            doc.InsertBefore(xmlDeclaration, root);

            var nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("xsi", Str2);

            var codeBook = doc.CreateElement("codeBook");
            codeBook.SetAttribute("version", "1.2.2");
            codeBook.SetAttribute("ID", "QIDANDREV");
            codeBook.SetAttribute("xml-lang", "en");
            codeBook.SetAttribute("xmlns", Str1);
            var att = doc.CreateAttribute("xsi", "SchemaLocation", Str2);
            att.Value = Str3;
            codeBook.SetAttributeNode(att);
            doc.AppendChild(codeBook);

            doc.CodeBook = codeBook;

            doc.DocDscr = doc.CreateElement("docDscr");
            codeBook.AppendChild(doc.DocDscr);

            doc.StdyDscr = doc.CreateElement("stdyDscr");
            doc.CodeBook.AppendChild(doc.StdyDscr);

            doc.DataDscr = doc.CreateElement("dataDscr");
            doc.CodeBook.AppendChild(doc.DataDscr);

            return doc;
        }
    }
}
