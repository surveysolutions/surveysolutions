using System.Collections.Generic;
using System.Globalization;

namespace ddidotnet
{
    // ??? there appears to be no difference in DDI XML output for nominal and ordinal numeric variables
    // IsTimeVariable for numeric and for date variables as well. Not sure what would be an example of numeric time variable???
    // Width seems to be for dynamic strings only! for reg strings specify actual length. This is never used in SS.

    /// <summary>
    ///     Describes all of the features of a single variable in a social science data file.
    ///     There are a number of attributes that allow to specify items such as the name of
    ///     the variable, what the weight variable is, etc.
    /// </summary>
    public class DdiVariable : IDdiVariable
    {
        private const int RecSegNo = 1; // PROBABLY ALWAYS HAVE TO BE 1 IN OUR CONTEXT
        private readonly DdiDataFile _dataFile;

        private Dictionary<decimal, string> _valueLabels;

        /// <summary>
        ///     Data type
        ///     Tells what is the type of content the variable stores.
        /// </summary>
        public DdiDataType DataType;

        // Properties below are applicable only to numeric variables
        /// <summary>
        ///     Number of decimals
        ///     \note Decimals seems to be limited to 0..16 regardless of the width
        /// </summary>
        public int Decimals;

        /// <summary>
        ///     Specific instructions to the individual conducting an interview.
        /// </summary>
        public string IvuInstr;

        /// <summary>
        ///     A short description of the parent element. In the variable label, the length
        ///     of this phrase may depend on the statistical analysis system used (e.g., some
        ///     versions of SAS permit 40-character labels, while some versions of SPSS
        ///     permit 120 characters), although the DDI itself imposes no restrictions on
        ///     the number of characters allowed.
        /// </summary>
        public string Label;

        internal int LastIndex = -1;

        /// <summary>
        ///     The attribute "name" usually contains the so-called "short label" for the
        ///     variable, limited to eight characters in many statistical analysis systems
        ///     such as SAS or SPSS.
        /// </summary>
        public string Name;

        /// <summary>
        ///     Text of the actual, literal question asked.
        /// </summary>
        public string QstnLit;

        /// <summary>
        ///     Indicates the type of measurement corresponding to this variable.
        /// </summary>
        public DdiVariableScale? VariableScale;

        /// <summary>
        ///     Width for string variables
        /// </summary>
        public int Width;

        internal DdiVariable(DdiDataType dt, DdiDataFile file)
        {
            DataType = dt;
            _dataFile = file;
        }

        /// <summary>
        ///     Adds an individual value label to the variable
        /// </summary>
        /// <param name="value">Numeric value being labelled.</param>
        /// <param name="label">Label for the specified value.</param>
        /// Value labels are kind of dictionaries that may be applied 
        /// to coded variables to make it easier for humans to work 
        /// with the codes.
        /// 
        /// \image html img\ValueLabels.png "Value labels" width=6cm
        /// \image latex img\ValueLabels.png "Value labels" width=6cm
        /// 
        /// \b Example Adding value labels
        /// \snippet Example2.cs Adding value labels
        public void AddValueLabel(decimal value, string label)
        {
            if (_valueLabels == null)
                _valueLabels = new Dictionary<decimal, string>();

            _valueLabels.Add(value, label);
        }

        /// <summary>
        ///     Assigns variable to a group.
        ///     If group with specified name does not exist it will be created.
        /// </summary>
        /// <param name="groupName">Name of the group to assign the variable to.</param>
        /// \image html img\DdiVarGroups2.png "Variable groups" width=8cm
        /// \image latex img\DdiVarGroups2.png "Variable groups" width=8cm
        /// 
        /// \sa \ref DdiDataFile.AssignVariableToGroup
        public void AssignToGroup(string groupName)
        {
            _dataFile.AssignVariableToGroup(groupName, this);
        }

        internal void WriteToXml(XmlDocumentExt doc, string dataFileId)
        {
            LastIndex = doc.VarIndex;
            doc.VarIndex++;

            // discrete if nominal or ordinal; contin if scale
            var intrvl = VariableScale == DdiVariableScale.Nominal || VariableScale == DdiVariableScale.Ordinal ||
                         VariableScale == null
                ? "discrete"
                : "contin";

            var vi = doc.CreateElement("var");
            vi.SetAttribute("ID",
                "V" + LastIndex.ToString(CultureInfo.InvariantCulture)); // this seems to be always the same V+index
            vi.SetAttribute("name", Name); // INPUT!
            vi.SetAttribute("files", dataFileId); // INPUT!
            vi.SetAttribute("intrvl", intrvl);
            doc.DataDscr.AppendChild(vi);

            var location = doc.CreateElement("location");
            if (DataType == DdiDataType.DynString) location.SetAttribute("width", "255");

            if (DataType == DdiDataType.FxString)
                location.SetAttribute("width", Width.ToString(CultureInfo.InvariantCulture));

            location.SetAttribute("RecSegNo", RecSegNo.ToString(CultureInfo.InvariantCulture));
            vi.AppendChild(location);

            doc.InsertValueC(vi, "labl", Label); // INPUT!

            var qstn = doc.CreateElement("qstn");
            vi.AppendChild(qstn);

            if (!string.IsNullOrEmpty(QstnLit)) doc.InsertValueC(qstn, "qstnLit", QstnLit); // INPUT!
            if (!string.IsNullOrEmpty(IvuInstr)) doc.InsertValueC(qstn, "ivuInstr", IvuInstr); // INPUT!

            if (_valueLabels != null)
                if (_valueLabels.Count > 0)
                    foreach (var vl in _valueLabels)
                    {
                        var catgry = doc.CreateElement("catgry");
                        vi.AppendChild(catgry);

                        doc.InsertValue(catgry, "catValu", vl.Key.ToString(CultureInfo.InvariantCulture));
                        doc.InsertValueC(catgry, "labl", vl.Value);
                    }

            var varFormat = doc.CreateElement("varFormat");
            switch (DataType)
            {
                case DdiDataType.DynString:
                case DdiDataType.FxString:
                    varFormat.SetAttribute("type", "character");
                    break;
                default:
                    varFormat.SetAttribute("type", "numeric");
                    break;
            }

            varFormat.SetAttribute("schema", "other");
            vi.AppendChild(varFormat);
        }
    }
}
