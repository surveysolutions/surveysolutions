using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using StatData.Core;
using System.Collections.Generic;

namespace StatData.Writers.Spss
{
    /// <summary>
    /// Represents all meta information controlling how the data should be written to an SPSS file
    /// </summary>
    internal class SpssDatasetMeta
    {
        /// <summary>
        /// Number of variables in the SPSS file (read-only)
        /// </summary>
        internal int NumVars
        {
            get { return Math.Min(NumCols, Variables.Length); }
        }

        internal int NumCols { get; set; }
        internal int NumObs { get; set; }

        internal CultureInfo Culture
        {
            get
            {
                if (_coreMeta != null)
                    return _coreMeta.Culture;
                return Util.DefaultCulture;
            }
        }

        /// <summary>
        /// Array of variables. At least one variable must be defined.
        /// </summary>
        internal SpssVariable[] Variables { get { return _variables; } }

        private readonly SpssVariable[] _variables;

        private readonly IDatasetMeta _coreMeta;

        /// <summary>
        /// Instantiates meta information controlling output to SPSS file from a generic meta information
        /// </summary>
        /// <param name="meta">Generic meta information</param>
        public SpssDatasetMeta(IDatasetMeta meta)
        {
            _coreMeta = meta;
            var l = meta.Variables.Length;
            _variables = new SpssVariable[l];
            for (var v = 0; v < l; v++)
                _variables[v] = new SpssVariable(meta.Variables[v].VarName); // validation only, if necessary link to meta.Variables[v] as a shadow
        }

        internal void UpdateWithData(IDataQuery data)
        {
            Trace.WriteLine(DateTime.Now + "Update with data began");

            if (data == null)
            {
                throw new ArgumentNullException("data", "Data must be defined");
            }
            
            NumObs = data.GetRowCount();
            NumCols = data.GetColCount();
            if (NumVars == 0)
            {
                throw new ArgumentException("There must be at least one variable to export.");
            }

            var multi = SpssEncoder.GetEncodingMulti();
            
            var allNumeric = new bool[NumVars];
            var maxlen = new int[NumVars];
            for (var v = 0; v < NumVars; v++)
            {
                // todo: this looks illogical at this point
                if (SpssVariable.InvalidVariableName(_coreMeta.Variables[v].VarName.ToUpper()))
                    throw new Exception("Invalid variable name: " + _coreMeta.Variables[v].VarName);
                allNumeric[v] = _coreMeta.Variables[v].Storage != VariableStorage.StringStorage;
                maxlen[v] = 0;


                if (_coreMeta.Variables[v].Storage == VariableStorage.DateStorage)
                {
                    Variables[v].PrintFormat = SpssVarFormat.DefaultDateFormat.Value;
                    Variables[v].WriteFormat = SpssVarFormat.DefaultDateFormat.Value;
                }
                else if (_coreMeta.Variables[v].Storage == VariableStorage.DateTimeStorage)
                {
                    Variables[v].PrintFormat = SpssVarFormat.DefaultDateTimeFormat.Value;
                    Variables[v].WriteFormat = SpssVarFormat.DefaultDateTimeFormat.Value;
                }
            }

            for (Int64 i = 0; i < NumObs; i++)
            {
                for (var v = 0; v < NumVars; v++)
                {
                    // if we are told this is a variable of a particular kind, don't touch it
                    var value = data.GetValue(i, v);
                    if (string.IsNullOrEmpty(value)) continue;

                    var l = Math.Min(value.Length * multi, SpssConstants.MaxStrWidth);
                    if (l > maxlen[v])
                        maxlen[v] = l;

                    if (_coreMeta.Variables[v].Storage != VariableStorage.UnknownStorage)
                    {
                        continue;
                    }

                    double trash;
                    if (value != SpssConstants.MissChar)
                        if (!_coreMeta.ExtendedMissings.IsMissing(value))
                            if (Util.TryStringToDouble(value, Culture, out trash) == false)
                                allNumeric[v] = false;
                }
            }

            var vindex = 1;
            for (var v = 0; v < NumVars; v++)
            {
                if (allNumeric[v] == false) 
                    if (maxlen[v] == 0) 
                        maxlen[v] = 1;
                _variables[v].VarIndex = vindex;
                vindex = vindex + MoveIndex(v,_variables[v], allNumeric[v], maxlen[v]); // _variables[v].MoveIndex(allNumeric[v], maxlen[v]);
            }

            UpdateExtendedMissings();

            Trace.WriteLine(DateTime.Now + "Update with data completed");
        }

        private int MoveIndex(int vindex, SpssVariable v, bool allNumeric, int maxlen)
        {
            // return v.MoveIndex(allNumeric, maxlen);
            if (v.WriteFormat == SpssVarFormat.DefaultDateFormat.Value
                || v.WriteFormat == SpssVarFormat.DefaultDateTimeFormat.Value)
                return v.MoveIndexDateTime();

            if (allNumeric)
                return v.MoveIndexNumeric(_coreMeta.Variables[vindex].FormatDecimals);

            return v.MoveIndexString(maxlen);
        }


        private void UpdateExtendedMissings()
        {
            UpdateExtendedNumMissings();
            // UpdateExtendedStrMissings();
            // there is nothing apparently wrong with the handling of the extended 
            // string missing values, but they should be delayed until the issues 
            // with writing of the long strings is resolved.

        }

        private void UpdateExtendedNumMissings()
        {
            var extMiss = _coreMeta.ExtendedMissings.GetList();
            if (extMiss.Count <= 0) return;
            
            // extended missing values were specified
            var extMissDoubles = new double[extMiss.Count];
            var c = 0;
            foreach (var m in extMiss)
            {
                extMissDoubles[c] = Util.StringToDouble(m.MissingValue, Culture);
                c++;
                if (c >= SpssConstants.MaxNumericMissingValues) break;
            }

            for (var v = 0; v < NumVars; v++)
            {
                if (_variables[v].VarType == 0)
                {
                    // record them for numeric variables
                    _variables[v].ExtendedMissingValues = extMissDoubles;
                }
            }
        }

        private void UpdateExtendedStrMissings()
        {
            // KEEP! DO NOT REMOVE!
            // This is currently not called, but is prepared for the future expansion.
            // KEEP! DO NOT REMOVE!

            var extMiss = _coreMeta.ExtendedStrMissings.GetList();
            if (extMiss.Count <= 0) return;

            // extended missing values were specified
            var extMissStrings = new string[extMiss.Count];
            var c = 0;
            foreach (var m in extMiss)
            {
                var s = m.MissingValue;
                if (s.Length>8) s = s.Substring(0, 8);
                extMissStrings[c] = s.PadRight(8, ' '); // todo: limit is 8 bytes, not 8 chars!
                c++;
                if (c >= SpssConstants.MaxStringMissingValues) break;
            }

            for (var v = 0; v < NumVars; v++)
            {
                if (_variables[v].VarType != 0)
                {
                    // record them for string variables
                    _variables[v].ExtendedStrMissingValues = extMissStrings;
                }
            }
        }

        /*
        internal void UpdateWithDataOld(IDataQuery data)
        {
            Trace.WriteLine(DateTime.Now + "Update with data began");

            if (data == null)
            {
                throw new ArgumentNullException("data", "Data must be defined");
            }

            NumObs = data.GetRowCount();
            NumCols = data.GetColCount();
            if (NumVars == 0)
            {
                throw new ArgumentException("There must be at least one variable to export.");
            }

            var multi = SpssEncoder.GetEncodingMulti();
            var vindex = 1;

            for (var v = 0; v < NumVars; v++)
            {
                // todo: this looks illogical at this point
                if (SpssVariable.InvalidVariableName(_coreMeta.Variables[v].VarName.ToUpper()))
                    throw new Exception("Invalid variable name: " + _coreMeta.Variables[v].VarName);

                var allNumeric = _coreMeta.Variables[v].Storage != VariableStorage.StringStorage;

                var maxlen = 0;
                for (Int64 i = 0; i < NumObs; i++)
                {
                    var value = data.GetValue(i, v);

                    if (string.IsNullOrEmpty(value)) continue;

                    double trash;

                    if (value != SpssConstants.MissChar)
                        if (Util.TryStringToDouble(value, Culture, out trash) == false)
                            allNumeric = false;

                    var l = Math.Min(value.Length * multi, SpssConstants.MaxStrWidth);

                    if (l > maxlen)
                        maxlen = l;
                }

                if (allNumeric == false)
                    if (maxlen == 0) maxlen = 1;

                _variables[v].VarIndex = vindex;
                vindex = vindex + _variables[v].MoveIndex(allNumeric, maxlen);
            }

            Trace.WriteLine(DateTime.Now + "Update with data completed");
        }
        */
        internal void WriteT1(SpssBinaryWriter sw)
        {
            var datetime = SpssTimeStamp.GetDateInfo(_coreMeta.TimeStamp);

            sw.Write(Encoding.ASCII.GetBytes(SpssConstants.Signature)); // always ASCII??
            sw.Write(Encoding.ASCII.GetBytes(SpssConstants.Product.PadRight(SpssConstants.ProductWidth)));
                // always ASCII??
            sw.WriteInt32(new Int32[] {2, -1, 0, 0, NumObs});
            sw.Write((Double) SpssConstants.CompressionBias);
            sw.Write(Encoding.ASCII.GetBytes(datetime.Date)); // always ASCII??
            sw.Write(Encoding.ASCII.GetBytes(datetime.Time)); // always ASCII??
            sw.Write(SpssEncoder.GetStringBytes(_coreMeta.DataLabel, SpssConstants.DataLabelWidth));
            sw.Write((Byte) 0);
            sw.Write((Byte) 0);
            sw.Write((Byte) 0);
        }

        internal void WriteT2(SpssBinaryWriter sw)
        {
            Int32 idx = 1; // running variable index that takes into account SPSS data width
            
            for (var i = 0; i < NumVars; i++)
            {
                Variables[i].WriteVarInfo(sw, i + 1, _coreMeta.Variables[i]);
                Variables[i].VarIndex = idx;
                idx = idx + Variables[i].SpssW;
            }

            // write the actual width of the observation instead of earlier declared -1
            const Int32 spssWidthOffset = 68;
            var position = sw.BaseStream.Position;
            sw.BaseStream.Seek(spssWidthOffset, SeekOrigin.Begin);
            sw.WriteInt32(new Int32[] {idx - 1});
            sw.BaseStream.Seek(position, SeekOrigin.Begin);
        }

        internal void WriteT3T4(SpssBinaryWriter sw)
        {
            var labelledVariables = _coreMeta.GetLabelledVariables();
            var allVariables = _coreMeta.Variables;
            var emvSet = _coreMeta.ExtendedMissings.GetList();

            foreach (var v in allVariables)
            {
                var varname = v.VarName;

                var varindex = FindVariable(varname);
                if (varindex < 0) continue;

                if (Variables[varindex].VarType != 0) continue; // string var
                if (emvSet.Count==0 
                    && !labelledVariables.Contains(varname)) continue; // no labels or ext missings

                WriteT3(sw, varname);
                WriteT4(sw, varindex);
            }
        }

        private void WriteT3(SpssBinaryWriter sw, string varname)
        {
            // one or both are expected to be non-empty
            var valueSet = _coreMeta.GetValueSet(varname);
            var emv = _coreMeta.ExtendedMissings.GetList();

            // append extended missing values
            var together = new Dictionary<double, string>();

            if (valueSet != null)
                foreach (var p in valueSet)
                    together.Add(p.Key, p.Value);

            if (emv != null)
                foreach (var p in emv)
                {
                    // this assumes that extended missing is denoted as a number!
                    var z = Util.StringToDouble(p.MissingValue, Culture);
                    if (!together.ContainsKey(z))
                        together.Add(z, p.Label);
                }

            sw.WriteInt32(new Int32[] {3, together.Count});
            foreach (var pair in together)
            {
                var v = pair.Value;
                if (v.Length > SpssConstants.ValueLabelWidth) v = v.Substring(0, SpssConstants.ValueLabelWidth); // this check must be done for total number of bytes!!

                var lb = SpssEncoder.GetStringBytes(v); // actual bytes of the label
                var alen = (Byte) (Math.Ceiling((lb.Length + 1)/8.00)*8.00 - 1);

                sw.Write((Double) pair.Key);
                sw.Write((Byte)lb.Length); //was alen now lb.Length
                sw.Write(SpssEncoder.GetStringBytes(v, alen));
            }
        }

        private void WriteT4(SpssBinaryWriter sw, int varindex)
        {
            sw.WriteInt32(new Int32[] {4, 1, Variables[varindex].VarIndex});
        }

        private void WriteT6(SpssBinaryWriter sw)
        {
            var w = SpssConstants.SpssCommentLineWidth;
            var asciiComment = _coreMeta.GetAsciiComment();
            if (String.IsNullOrEmpty(asciiComment)) asciiComment = SpssConstants.Product;

            if (asciiComment.Length % w != 0)
            {
                var padw = (int)(Math.Ceiling((double)asciiComment.Length / w) * w);
                asciiComment = asciiComment.PadRight(padw, Convert.ToChar(SpssConstants.FillerByte));
            }            

            Int32 nLines = asciiComment.Length / w;
            sw.WriteInt32(new Int32[] { 6, nLines });
            for (var i = 0; i < nLines; i++)
                sw.Write(Encoding.ASCII.GetBytes(asciiComment.Substring(i * 80, w)));
        }

        private int FindVariable(string varname)
        {
            var varindex = -1;
            for (var i = 0; i < NumVars; i++)
                if (_coreMeta.Variables[i].VarName == varname)
                {
                    varindex = i;
                    break;
                }
            return varindex;
        }

        internal void WriteT7S13(SpssBinaryWriter sw)
        {
            // R7.13: LONG VARIABLE NAMES

            var sb = new StringBuilder();
            for (var v = 0; v < NumVars; v++)
            {
                if (v > 0) sb.Append('\t');
                var vn = SpssVariable.GetVarname(v + 1);
                sb.Append(vn + "=" + _coreMeta.Variables[v].VarName);
            }

            var b = SpssEncoder.GetStringBytes(sb.ToString());
            sw.WriteInt32(new Int32[] {7, 13, 1, b.Length});
            sw.Write(b);
        }

        internal void WriteT7S14(SpssBinaryWriter sw)
        {
            // R7.14 : LONG STRING CONCATENATION VARIABLES

            var cult = CultureInfo.InvariantCulture;
            var c = 0;
            var sb = new StringBuilder();
            for (var v = 0; v < NumVars; v++)
            {
                if (Variables[v].VarType <= 0) continue;

                var vlen = Variables[v].VarLength;
                if (vlen > 255)
                {
                    sb.Append(String.Format("{0}={1}\0\t", SpssVariable.GetVarname(v + 1), vlen.ToString(cult))); // Recommended "00000" but doesn't seem to matter.
                    c++;
                }
            }

            if (c > 0)
            {
                var content = sb.ToString();
                sw.WriteInt32(new Int32[] {7, 14, 1, content.Length});
                sw.Write(Encoding.ASCII.GetBytes(content));
            }
        }

        internal void WriteT7S16(SpssBinaryWriter sw)
        {
            // R7.16: 64-BIT NUMBER OF CASES
            sw.WriteInt32(new Int32[] {7, 16, 8, 2});
            sw.Write((Int64) 1);
            sw.Write((Int64) NumObs);
        }

        internal void WriteT7S20(SpssBinaryWriter sw)
        {
            // R7.20: Encoding name
            // Write encoding record, in our case always 1252 or UTF-8
            var encodingName = SpssEncoder.GetSpssEncodingName();
            sw.WriteInt32(new Int32[] {7, 20, 1, encodingName.Length});
            sw.Write(Encoding.ASCII.GetBytes(encodingName)); // always ASCII??
        }

        internal void WriteT7S22(SpssBinaryWriter sw)
        {
            // R7.22 Long string missing values
            return;

            /*var sb = new MemoryStream();
            for (var v=0; v<NumVars; v++)
            {
                if (Variables[v].VarType!=0)
                {
                    var vn = _coreMeta.Variables[v].VarName;
                    // add an extended string missing value to each string variable regardless of length
                    sb.Write(BitConverter.GetBytes(vn.Length), 0, 4); // length of varname
                    var vnBytes = SpssEncoder.GetStringBytes(vn);
                    sb.Write(vnBytes, 0, vnBytes.Length);
                    sb.Write(BitConverter.GetBytes((Byte)1), 0, 1); // number of extended missing values
                    sb.Write(BitConverter.GetBytes((Int32)8), 0, 4); // length of the missing value (always 8 for now)
                    sb.Write(SpssEncoder.GetStringBytes("##N/A## "), 0, "##N/A## ".Length); // must always be 8 bytes long
                }
            }
            sw.WriteInt32(new Int32[] {7, 22, 1, (Int32) sb.Length});
            sb.WriteTo(sw.BaseStream);*/
        }

        internal void WriteT7S3(SpssBinaryWriter sw)
        {
            // R7.3
            sw.WriteInt32(new Int32[] {7, 3, 4, 8, 16, 0, 0, (2*256 + 13*16), 1, 1, 2});
            sw.Write((UInt32) SpssConstants.EncodingCode);
        }

        internal void WriteT7S4(SpssBinaryWriter sw)
        {
            sw.WriteInt32(new Int32[] {7, 4, 8, 3});
            sw.Write(SpssConstants.V1);
            sw.Write(SpssConstants.V2);
            sw.Write(SpssConstants.V3);
        }
        /*
        internal void WriteT7S11(SpssBinaryWriter sw)
        {
            // DUMMY FOR DEBUGGING ONLY
            sw.WriteInt32(new Int32[] { 7, 11, 4, 36 });
            for (var i = 1; i <= 12; i++)
            {
                sw.WriteInt32(new Int32[] {1,8,0});
            }
        }

        internal void WriteT7S18(SpssBinaryWriter sw)
        {
            // DUMMY FOR DEBUGGING ONLY

            var ttt = "short:$@Role('0')";
            sw.WriteInt32(new Int32[] { 7, 18, 1, ttt.Length });
            sw.Write(Encoding.ASCII.GetBytes(ttt)); // always ASCII??
        }*/

        internal void WriteHeader(SpssBinaryWriter w)
        {
            WriteT1(w);    // 01   Header
            WriteT2(w);    // 02   Variable entry
            WriteT3T4(w);  // 03   Variable and value labels
            WriteT6(w);    // 06   Embedded document
            WriteT7S3(w);  // 0703 Machine integer info record
            WriteT7S4(w);  // 0704 Machine floating point info record
            //WriteT7S11(w); // XYZ XYZ XYZ XYZ XYZ XYZ XYZ XYZ XYZ
            WriteT7S13(w); // 070D Long variable names
            WriteT7S14(w); // 070E Long strings concatenation
            WriteT7S16(w); // 0710 64-bit number of cases
            //riteT7S18(w); // DEBUG DEBUG DEBUG DEBUG DEBUG DEBUG DEBUG
            WriteT7S20(w); // 0714 Encoding name
            WriteT7S22(w); // 0716 Long string missing values
        }
        
    }
}
