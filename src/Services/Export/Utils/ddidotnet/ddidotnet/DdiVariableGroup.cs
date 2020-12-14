using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ddidotnet
{
    internal class DdiVariableGroup
    {
        private readonly List<DdiVariable> _variables = new List<DdiVariable>();

        /// <summary>
        ///     Variables group label
        /// </summary>
        public string Label;

        /// <summary>
        ///     Adds a specified variable to the group
        /// </summary>
        /// <param name="variable">Variable to be added</param>
        internal void AddVariable(DdiVariable variable)
        {
            _variables.Add(variable);
        }

        internal void WriteToXml(XmlDocumentExt doc)
        {
            var sb = new StringBuilder();
            foreach (var variable in _variables)
                sb.Append("V" + variable.LastIndex.ToString(CultureInfo.InvariantCulture) + " ");

            var varlist = sb.ToString().Trim();

            var varGrp = doc.CreateElement("varGrp");
            varGrp.SetAttribute("ID", "VG" + doc.GroupIndex.ToString(CultureInfo.InvariantCulture));
            doc.GroupIndex++;
            varGrp.SetAttribute("type", "multipleresp");
            varGrp.SetAttribute("var", varlist);
            doc.DataDscr.AppendChild(varGrp);
            doc.InsertValue(varGrp, "labl", Label);
        }
    }
}
