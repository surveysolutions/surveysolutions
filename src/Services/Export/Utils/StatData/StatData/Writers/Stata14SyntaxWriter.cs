using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using StatData.Core;
using StatData.Writers.Stata;

namespace StatData.Writers
{
    public class Stata14SyntaxWriter : DatasetWriter, IDatasetWriter
    {
        private IDatasetMeta _meta = new DatasetMeta();

        #region Overrides of DatasetWriter

        public override void WriteToStream(Stream stream, IDatasetMeta meta, string[,] data)
        {
            _meta = meta;
            var dq = new StringArrayDataQuery(data);
            WriteToStream(stream, meta, dq);
        }

        public override void WriteToStream(Stream stream, IDatasetMeta meta, IDataQuery data)
        {
            _meta = meta;
            var helper = new StataWriterHelper(meta, data);
            SaveToStream(stream, helper);
        }

        #endregion

        internal void SaveToStream(Stream fs, IDataAccessor data)
        {
            // keep this internal or extend IDataAccessor to also provide access to meta
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("version 14.0");
            sb.AppendLine("clear");
            sb.AppendLine(String.Format("insheet using \"${{SUSODATA}}{0}\", tab case",data.FileName()));
            sb.AppendLine();

            for (var v = 0; v < data.GetVarCount(); v++)
            {
                var vn = data.GetVarName(v);

                var varLabel = data.GetVarLabel(v);
                if (!string.IsNullOrWhiteSpace(varLabel))
                    sb.AppendLine(String.Format("label variable {0} `\"{1}\"'", vn, varLabel));
                
                if (data.IsVarNumeric(v))
                {
                    // check if need to write value labels dictionary
                    // Construct dictionary
                    var dctSize = data.GetDctSize(v);
                    if (dctSize > 0)
                    {
                        sb.AppendLine(String.Format("label define {0} ///", vn));
                        for (var i = 0; i < dctSize; i++)
                        {
                            sb.AppendLine(
                                String.Format(
                                    "\t{0} `\"{1}\"' ///",
                                    data.GetDctCode(v, i).ToString(CultureInfo.InvariantCulture),
                                    data.GetDctLabel(v, i)));
                        }
                        sb.AppendLine();
                        sb.AppendLine(String.Format("label values {0} {0}", vn));
                    }
                    // Recode extended missing values
                    var ml = _meta.ExtendedMissings.GetList();
                    if (ml.Count > 0)
                    {
                        for (var mi = 0; mi < ml.Count; mi++)
                        {
                            sb.AppendLine(
                                String.Format(
                                    "replace {0}={1} if {0}=={2}",
                                    vn,
                                    StataCore.GetMissingByIndex(mi),
                                    ml[mi].MissingValue));
                        }
                    }
                }
                sb.AppendLine();
            }
            sb.AppendLine();

            var b = Encoding.UTF8.GetBytes(sb.ToString());
            fs.Write(b, 0, b.Length);
        }


    }
}
