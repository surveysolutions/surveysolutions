using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using StatData.Core;
using StatData.Writers.Spss;

namespace StatData.Writers
{
    public class SpssSyntaxWriter : DatasetWriter, IDatasetWriter
    {
        #region Overrides of DatasetWriter

        public override void WriteToStream(Stream stream, IDatasetMeta meta, string[,] data)
        {
            throw new NotImplementedException();
        }

        public override void WriteToStream(Stream stream, IDatasetMeta meta, IDataQuery data)
        {
            // inspection for types -- same as in binary SPSS writer
            SpssDatasetMeta m =
                meta is SpssDatasetMeta
                    ? meta as SpssDatasetMeta
                    : new SpssDatasetMeta(meta);

            m.UpdateWithData(data);
            // --------------------------------
            string vlist = "";
            for(var v=0; v<m.Variables.Length;v++)
            {
                var vt = m.Variables[v].VarType;

                vlist = vlist
                        + " \r\n     "
                        + meta.Variables[v].VarName + " "
                        + ((vt == 0)
                               ? "F20"
                               : "A" + vt.ToString(CultureInfo.InvariantCulture));
            }
            // --------------------------------

            var sb = new StringBuilder();
            sb.AppendLine();
            //----------------------------

            //sb.AppendLine("CD 'SOMEFOLDER'"); // may be required for some systems, but don't know where the data will be saved to.

            sb.AppendLine("GET DATA");
            sb.AppendLine("   /TYPE=TXT");
            sb.AppendLine(String.Format("   /FILE='{0}'", data.GetName()));
            sb.AppendLine("   /DELCASE=LINE");
            sb.AppendLine("   /DELIMITERS=\"\\t\"");
            sb.AppendLine("   /ARRANGEMENT=DELIMITED");
            sb.AppendLine("   /FIRSTCASE=2 ");
            sb.AppendLine(String.Format("   /VARIABLES={0}", vlist + " ."));
            sb.AppendLine();

            WriteVariableLabels(sb, meta);
            WriteValueLabels(sb, meta);

            //----------------------------
            sb.AppendLine();

            var b = Encoding.UTF8.GetBytes(sb.ToString());
            stream.Write(b, 0, b.Length);
        }

        private void WriteVariableLabels(StringBuilder sb, IDatasetMeta meta)
        {
            var lastIndex = -1;
            var variablesWithLabels = new List<int>(); // list of indexes of labelled variables
            for (var v = 0; v < meta.Variables.Length; v++)
                if (!string.IsNullOrWhiteSpace(meta.Variables[v].VarLabel))
                {
                    variablesWithLabels.Add(v);
                    lastIndex = v;
                }

            if (variablesWithLabels.Count > 0)
            {
                sb.AppendLine("VARIABLE LABELS");
                foreach (var vi in variablesWithLabels)
                {
                    var vn = meta.Variables[vi].VarName;
                    var vl = meta.Variables[vi].VarLabel;
                    var vt = (vi == lastIndex) ? "." : "";

                    if (!string.IsNullOrWhiteSpace(vl))
                        sb.AppendLine(String.Format("{0} '{1}' {2}", vn, vl, vt));
                }
                sb.AppendLine();
            }
        }


        private void WriteValueLabels(StringBuilder sb, IDatasetMeta meta)
        {
            var labelledVars = meta.GetLabelledVariables();
            var lastVar = labelledVars[labelledVars.Count - 1];

            if (labelledVars.Count <= 0) return;

            sb.AppendLine("VALUE LABELS");

            foreach (var v in labelledVars)
            {
                var vsfx = (v == lastVar) ? " ." : "";
                sb.AppendLine(String.Format(" {0}", v));

                var dct = meta.GetValueSet(v);
                var items = dct.ToArray();

                for (var i = 0; i < items.Length;i++ )
                {
                    var item = items[i];

                    var sfx = (i == items.Length - 1) ? " /" + vsfx : "";

                    sb.AppendLine(
                        String.Format(
                            "     {0} '{1}'{2}",
                            item.Key.ToString(CultureInfo.InvariantCulture),
                            item.Value,
                            sfx));
                }
            }

        }

        #endregion
    }
}
