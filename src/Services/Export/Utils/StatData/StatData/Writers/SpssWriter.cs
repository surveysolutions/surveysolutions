using System;
using System.IO;
using StatData.Core;
using StatData.Writers.Spss;

namespace StatData.Writers
{
    /// <summary>
    /// Writes dataset SPSS's native binary format.
    /// 
    /// 
    /// Writer has the following limitations:
    /// max number of observations:
    ///   in SPSS student version is known to be 1500; http://www.spsstools.net/FAQ.htm#HowManyVariables
    ///   in SPSS-32 known to be 2,147,493,647;
    ///   in SPSS-64 unknown;
    /// http://www-01.ibm.com/support/docview.wss?uid=swg21476061
    /// 
    /// max number of variables: unknown; known to depend on the version of SPSS and data types;
    /// all versions can accommodate 32,767 numeric variables. Number of string variables may be smaller.
    /// SPSS software is known to slow down operating with "thousands of variables".(around 2004).
    /// 
    /// SPSS format allows for strings up to 32,767 according to http://www-01.ibm.com/support/knowledgecenter/SSLVMB_20.0.0/com.ibm.spss.statistics.help/syn_variables_string_variable_formats.htm?lang=en
    /// This writer is not guaranteed to handle more than 10000 characters;
    /// unicode is supported.
    /// 
    /// </summary>
    public class SpssWriter: DatasetWriter, IDatasetWriter
    {
        public override void WriteToStream(Stream stream, IDatasetMeta meta, IDataQuery data)
        {
            SpssDatasetMeta m;
            if (meta is SpssDatasetMeta) m = meta as SpssDatasetMeta;
            else m = new SpssDatasetMeta(meta);

            m.UpdateWithData(data);

            var w = new SpssBinaryWriter(stream);

            m.WriteHeader(w);
            WriteData(w, m, data);

        }

        public override void WriteToStream(Stream stream, IDatasetMeta meta, string[,] data)
        {
            var dq = new StringArrayDataQuery(data);
            WriteToStream(stream, meta, dq);
        }

        private void WriteData(SpssBinaryWriter w, SpssDatasetMeta datasetMeta, IDataQuery data)
        {
            // todo: we only support writing numbers as doubles

            w.Write((Int32) 999);
            w.Write((Int32) 0);

            for (var i = 0; i < datasetMeta.NumObs; i++)
            {
                for (var v = 0; v < datasetMeta.NumVars; v++)
                {
                    {
                        var value = data.GetValue(i, v) ?? String.Empty;

                        if (datasetMeta.Variables[v].VarType == 0)
                        {
                            // write numeric value
                            if (String.IsNullOrEmpty(value) == false && value != SpssConstants.MissChar)
                            {
                                if (datasetMeta.Variables[v].WriteFormat == SpssVarFormat.DefaultDateFormat.Value
                                    || datasetMeta.Variables[v].WriteFormat == SpssVarFormat.DefaultDateTimeFormat.Value)
                                {
                                    // there should be no difference in value between date and time formats
                                    if (value == "##N/A##") w.Write(1.0);
                                    else w.Write((Convert.ToDateTime(value.Replace("T"," ").Replace("Z","")) - SpssConstants.ZeroDay).TotalSeconds);
                                }
                                else
                                {
                                    // todo: this should not be done like this since the values are not fixed
                                    // either 
                                    // a) convert 1-1 using extended missings or 
                                    // b) don't convert and crash on the non-numeric codes.
                                    if (value == "##N/A##") w.Write(-999999999.0);
                                    else w.Write(Util.StringToDouble(value, datasetMeta.Culture));
                                }
                            }
                            else
                                w.Write(SpssConstants.SysMiss);
                        }
                        else
                        {

                            if (datasetMeta.Variables[v].WriteFormat == SpssVarFormat.DefaultDateFormat.Value)
                            {
                                // write date
                                w.Write(0);
                                // todo: check here!
                            }
                            else
                            {
                                // write string value
                                var b = SpssEncoder.GetStringBytes(value, datasetMeta.Variables[v].SpssW*8);
                                var truelen = datasetMeta.Variables[v].VarType;
                                w.WriteLongStringBytes(b, truelen);
                            }
                        }
                    }
                }
                ChangeProgress(100.00*i/datasetMeta.NumObs);
            }
            ChangeProgress(100.00);
        }
    }

    
}
