using System;
using System.Globalization;
using System.IO;
using System.Text;
using StatData.Core;
using StatData.Writers.Stata;
using System.Collections.Generic;

namespace StatData.Writers
{
    /// <summary>
    /// Writes dataset in Stata's [version 12] native binary format
    /// 
    /// Writer has the following limitations:
    /// max number of observations 2,147,483,647;
    /// max number of variables 32,767; not all Stata flavors can open more than 2,048 variables;
    /// string variables not to exceed 244 bytes;
    /// unicode is not supported.
    /// 
    /// Variable name requirements (from Stata 13 manual, section 11.3 "Naming Conventions"):
    /// A name is a sequence of one to 32 letters (A–Z and a–z), digits (0–9), and underscores (_).
    /// The first character of a name must be a letter
    /// Variable names with an underscore are technically allowed in Stata, but are not desirable, and hence not allowed by this writer.
    /// Stata respects case; that is, myvar, Myvar, and MYVAR are three distinct names.
    /// The following names are reserved and hence disallowed for variable names:
    /// _all float _n _skip _b if _N str# byte in _pi strL _coef int _pred using _cons long _rc with double;
    /// where str# is any name of the type str1, str200, str24746894
    /// </summary>
    public class Stata12Writer : DatasetWriter, IDatasetWriter
    {
        internal long _writeBufferSize;

        internal long WriteBufferSize
        {
            get { return _writeBufferSize; }
        }

        #region Overrides of DatasetWriter

        public override void WriteToStream(Stream stream, IDatasetMeta meta, string[,] data)
        {
            var dq = new StringArrayDataQuery(data);
            WriteToStream(stream, meta, dq);
        }

        public override void WriteToStream(Stream stream, IDatasetMeta meta, IDataQuery data)
        {
            var helper = new StataWriterHelper(meta, data);
            SaveToStream(stream, helper);
        }
        
        #endregion

        internal void SaveToStream(Stream fs, IDataAccessor data)
        {
            var varcount = data.GetVarCount();
            var obscount = data.GetObsCount();

            var encoding = Encoding.GetEncoding(data.DesiredCodepage());

            var fw = new StreamWriterSpecial(fs);

            fw.Write(StataConstants.Specification);
            fw.Write(StataConstants.Platform);
            fw.Write((byte) 1);
            fw.Write((byte) 0);
            fw.Write((Int16) varcount);
            fw.Write((Int32) obscount);

            var dataLabel = data.GetDatasetLabel();
            if (dataLabel.Length > StataConstants.VarLabelLength)
                dataLabel = dataLabel.Substring(0, StataConstants.VarLabelLength);

            fw.Write(encoding.GetBytes(dataLabel));
            fw.Write(new byte[StataConstants.VarLabelLength + 1 - dataLabel.Length]);

            var dt = StatData.Writers.StataCore.StataDateTime(data.GetTimeStamp());
            fw.Write(encoding.GetBytes(dt));
            fw.Write(new byte[StataConstants.TimeStampWidth - dt.Length]);

            var map = new byte[varcount];
            for (var v = 0; v < varcount; v++)
                map[v] = data.GetVarTypeEx(v);
            fw.Write(map);

            // write the variable descriptors
            for (var v = 0; v < varcount; v++)
            {
                var vname = data.GetVarName(v);
                fw.Write(encoding.GetBytes(vname));
                fw.Write(new byte[StataConstants.VarNameLength + 1 - vname.Length]);
            }

            fw.Write(new byte[2*(varcount + 1)]); // sort order

            for (var v = 0; v < varcount; v++) // formats
            {
                string fmt;
                if (!data.IsVarNumeric(v)) fmt = StataConstants.DefaultFormatStr;
                else
                    fmt = data.IsNumericVarInteger(v)
                              ? StataConstants.DefaultFormatInt
                              : StataConstants.DefaultFormatNum;

                fw.Write(encoding.GetBytes(fmt));
                fw.Write(new byte[StataConstants.FormatWidth - fmt.Length]);
            }

            for (var v = 0; v < varcount; v++)
            {
                if (data.GetDctSize(v) > 0)
                {
                    var dctName = data.GetVarName(v);
                    fw.Write(encoding.GetBytes(dctName));
                    fw.Write(new byte[StataConstants.VarNameLength + 1 - dctName.Length]);
                }
                else
                {
                    fw.Write(new byte[StataConstants.VarNameLength + 1]);
                }
            }

            for (var v = 0; v < varcount; v++) // variable labels
            {
                var lbl = data.GetVarLabel(v);

                if (lbl.Length > StataConstants.VarLabelLength)
                    lbl = lbl.Substring(0, StataConstants.VarLabelLength);

                fw.Write(encoding.GetBytes(lbl));
                fw.Write(new byte[StataConstants.VarLabelLength + 1 - lbl.Length]);
            }

            //


            
            var asciiComment = data.GetAsciiComment();
            if (String.IsNullOrEmpty(asciiComment)) {
                fw.Write(new byte[] { 0, 0, 0, 0, 0 }); // expansion fields
            }
            else {
                fw.Write((byte)1);
                fw.Write((UInt32)asciiComment.Length+33+33+1);
                fw.WriteStr("_dta".PadRight(33, '\0'));
                fw.WriteStr("comment".PadRight(33, '\0'));
                fw.WriteStr(asciiComment+'\0');
                fw.Write(new byte[] { 0, 0, 0, 0, 0 }); // expansion fields
            }                   


            // Write data
            WriteData(fw, data, map, encoding);

            // Write value labels here!
            var valuelabels = GetValueLabels(data);
            foreach (var vl in valuelabels.Values)
                fw.Write(vl.ToBytes(Stata.StataConstants.VarNameLength));

            ChangeProgress(100.00);
        }



        private void WriteData(BinaryWriter fw, IDataAccessor data, 
            byte[] map, Encoding encoding)
        {
            var varcount = data.GetVarCount();
            var obscount = data.GetObsCount();

            // cache types update observation width
            var numericTypes = new bool[varcount];
            var dataWidth = 0;
            for (var v = 0; v < varcount; v++)
            {
                numericTypes[v] = (data.IsVarNumeric(v));
                dataWidth = dataWidth + StataVariable.GetVarWidth(map[v]);
            }

            var bufWrite = (WriteBufferSize > 0);
            byte[] buffer;
            var bufCap = 1;
            var bufOcc = 0;

            if (bufWrite)
            {
                bufCap = Math.Max((int)Math.Floor((double)WriteBufferSize / dataWidth), 1);
                buffer = new byte[bufCap * dataWidth];
            }
            else
            {
                buffer = new byte[dataWidth];
            }

            using (var memoryStream = new MemoryStream(buffer, true))
            {
                using (var memoryWriter = new BinaryWriter(memoryStream))
                {
                    ChangeProgress(0.00);
                    for (var i = 0; i < obscount; i++)
                    {
                        for (var v = 0; v < varcount; v++)
                        {
                            if (numericTypes[v])
                            {
                                // numeric

                                if (data.IsValueMissing(i, v))
                                {
                                    // write missing value
                                    memoryWriter.Write(StataMissing.GetBytes(map[v]));
                                }
                                else
                                {
                                    var x = data.GetNumericValue(i, v);
                                    switch (map[v])
                                    {
                                        case StataConstants.VarTypeDouble:
                                            memoryWriter.Write(x);
                                            break;
                                        case StataConstants.VarTypeFloat:
                                            memoryWriter.Write(Convert.ToSingle(x));
                                            break;
                                        case StataConstants.VarTypeLong:
                                            memoryWriter.Write(Convert.ToInt32(x));
                                            break;
                                        case StataConstants.VarTypeInt:
                                            memoryWriter.Write(Convert.ToInt16(x));
                                            break;
                                        default:
                                            memoryWriter.Write(Convert.ToSByte(x));
                                            break;
                                    }
                                }
                            }
                            else
                            {
                                // string
                                var w = data.GetVarWidth(v);
                                var strv = data.GetStringValue(i, v);
                                if (strv.Length > w)
                                    strv = strv.Substring(0, w);
                                strv = strv.PadRight(w, Convert.ToChar(0));
                                memoryWriter.Write(encoding.GetBytes(strv));
                            }
                        }

                        ChangeProgress(100.00 * i / obscount);

                        if (bufWrite)
                        {
                            bufOcc++;
                            if (bufOcc != bufCap) continue;
                            bufOcc = 0;
                        }
                        fw.Write(buffer);
                        memoryWriter.BaseStream.Seek(0, SeekOrigin.Begin);
                    }
                }

                // write the remainder of the buffer
                if (bufWrite)
                {
                    if (bufOcc > 0)
                    {
                        fw.Write(buffer, 0, dataWidth * bufOcc);
                    }
                }
            }
        }

        private static Dictionary<string, StataXValueSet> GetValueLabels(IDataAccessor data)
        {
            var result = new Dictionary<string, StataXValueSet>();

            for (var v = 0; v < data.GetVarCount(); v++)
            {
                /*
                var varname = data.GetVarName(v);
                var vs = data.GetValueSet(varname);
                var schema = StataCore.GetSchema(vs, varname, Encoding.GetEncoding(data.DesiredCodepage()));
                */
                
                //var schema = StataCore.GetSchema(data, v, Encoding.GetEncoding(data.DesiredCodepage()));
                // if (schema == null) continue;

                var schema = new StataXValueSet(data, v, Encoding.GetEncoding(data.DesiredCodepage()));
                if (schema.Lbls.Count == 0) continue; // #### ATTENTION HERE, MAY NOT BE EXACTLY EQUIVALENT TO NULL LABEL!!!

                result.Add(schema.Name, schema);
                schema.Construct();
            }

            return result;
        }

    }
}
