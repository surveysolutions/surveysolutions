using System.Xml;

namespace ddidotnet
{
    internal class XmlDocumentExt : XmlDocument
    {
        internal XmlElement CodeBook;
        internal XmlElement DataDscr;
        internal XmlElement DocDscr;
        internal int GroupIndex;
        internal XmlElement StdyDscr;

        internal int VarIndex;

        public XmlDocumentExt()
        {
            VarIndex = 1;
            GroupIndex = 1;
        }

        public XmlElement InsertValue(XmlElement node, string what, string value)
        {
            var tmp = CreateElement(what);
            node.AppendChild(tmp);
            tmp.AppendChild(CreateTextNode(value));
            return tmp;
        }

        public XmlElement InsertValueC(XmlElement node, string what, string value)
        {
            var tmp = CreateElement(what);
            node.AppendChild(tmp);
            tmp.AppendChild(CreateCDataSection(value));
            return tmp;
        }
    }
}
