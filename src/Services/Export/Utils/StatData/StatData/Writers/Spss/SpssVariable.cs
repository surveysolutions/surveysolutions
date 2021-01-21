using System;
using System.Text;
using StatData.Core;

namespace StatData.Writers.Spss
{
    /// <summary>
    /// Represents an SPSS variable
    /// </summary>
    internal class SpssVariable
    {
        // http://www-01.ibm.com/support/knowledgecenter/SSLVMB_20.0.0/com.ibm.spss.statistics.help/syn_variables_variable_names.htm

        internal Int32 VarType { get; set; }
        internal Int32 PrintFormat { get; set; } // decimals, width, format, unused
        internal Int32 WriteFormat { get; set; } // decimals, width, format, unused
        internal Int32 VarLength { get; set; }
        internal Int32 VarIndex { get; set; }
        internal Int32 PadLength { get; set; }
        internal Int32 SpssW { get; set; }
        internal double[] ExtendedMissingValues { get; set; }
        internal string[] ExtendedStrMissingValues { get; set; }
        
        /// <summary>
        /// Represents one SPSS variable
        /// </summary>
        /// <param name="name">mandatory parameter name of the SPSS variable</param>
        internal SpssVariable(string name)
        {
            if (InvalidVariableName(name))
                throw new ArgumentException("Invalid variable name: " + name);

            ExtendedMissingValues = new double[0];
        }

        internal static string GetVarname(int index)
        {
            //var indexStr = index.ToString("0000");
            //var indexStr = GetFormattedIndex36(index, 5);
            var indexStr = index.ToString(SpssConstants.Cult);
            return string.Format(DatasetVariable.TemplVar, indexStr);
        }

        private static string GetFormattedIndex36(long value, int w)
        {
            const int toBase = 36;
            const string characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            StringBuilder builder = new StringBuilder();

            while (value > 0)
            {
                int remainder = (int)(value % toBase);

                builder.Insert(0, characters[remainder]);

                value /= toBase;
            }

            return builder.ToString().PadLeft(w, '0');
        }





        internal static string GetSubVarname(int index, int segment)
        {
            
            var vshort = String.Format(DatasetVariable.TemplSubVar,
                index.ToString(SpssConstants.Cult) , 
                (segment - 2).ToString(SpssConstants.Cult));
            

            /*
            var vshort = String.Format(DatasetVariable.TemplSubVar,
                index.ToString("0000") , 
                (segment - 2).ToString("00"));*/

    //        vshort = String.Format(DatasetVariable.TemplSubVar,
    //            /*Convert.ToString(index, 16).ToUpper(SpssConstants.Cult).PadLeft(4, '0')*/
    //            GetFormattedIndex36(index, 5), GetFormattedIndex36(segment-2, 1)
    //            /*Convert.ToString(segment - 2, 16).ToUpper(SpssConstants.Cult).PadLeft(2, '0')*/);

            return vshort;
        }


        /// <summary>
        /// Validator of variable names
        /// </summary>
        /// <param name="name">candidate name</param>
        /// <returns>TRUE if candidate name is INVALID!</returns>
        internal static bool InvalidVariableName(string name)
        {
            // too short
            if (String.IsNullOrEmpty(name)) return true;

            // too long
            var b = SpssEncoder.GetStringBytes(name);
            if (b.Length > 64) return true;

            // contains invalid char
            var nameU = name.ToUpper(SpssConstants.Cult);

            const string validchars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ_";
            foreach (var c in nameU)
                if (validchars.IndexOf(c) < 0) return true;

            // starts or ends with an underscore
            if (name.StartsWith("_") | name.EndsWith("_")) return true;

            // starts with a digit
            if ("0123456789".Contains(name.Substring(0, 1))) return true;

            // is a reserved name
            if (SpssConstants.ReservedNames.Contains(nameU)) return true;

            return false;
        }


        internal void WriteVarInfo(SpssBinaryWriter sw, Int32 index, IDatasetVariable variable)
        {
            if (VarType == 0)
            {
                WriteNumericVarInfo(sw, index, variable);
            }
            else
            {
                WriteStringVarInfo(sw, index, variable);
            }
        }

        internal void WriteNumericVarInfo(SpssBinaryWriter sw, Int32 index, IDatasetVariable variable)
        {
            var varLabel = variable.VarLabel;

            var vlp = (Byte)(String.IsNullOrEmpty(varLabel) ? 0 : 1);

            var mvToBeWritten = Math.Min(
                SpssConstants.MaxNumericMissingValues,
                ExtendedMissingValues.Length);

            sw.WriteInt32(new Int32[]
                                {
                                    2, VarType, vlp,
                                    mvToBeWritten,
                                    PrintFormat, WriteFormat
                                });
            
            sw.Write(Encoding.ASCII.GetBytes((GetVarname(index)).PadRight(8))); // always ASCII

            if (vlp == 1)
                WriteVariableLabel(sw, varLabel);


            SpssW = 1; // count chunks instead of bytes
            
            // Now writing the extended numeric missing values here.
            var nEmv = 0;
            foreach (var missingValue in ExtendedMissingValues)
            {
                sw.Write(missingValue);
                nEmv = nEmv + 1;
                if (nEmv >= mvToBeWritten)
                    break; // write no more than max allowed missing values
            }
        }

        internal void WriteStringVarInfo(SpssBinaryWriter sw, Int32 index, IDatasetVariable variable)
        {
            var varLabel = variable.VarLabel;

            // for strings additionally override formats
            var w = (Byte)(VarType >= 255 ? 255 : VarLength);
            SetFormat(new SpssVarFormat(0, w, 1));

            var vlp = (Byte)(String.IsNullOrEmpty(varLabel) ? 0 : 1);
            /*
            var mvToBeWritten = Math.Min(
                SpssConstants.MaxStringMissingValues,
                ExtendedStrMissingValues.Length);*/

            var mvToBeWritten = 0;

            if (VarType <= 255)
            {
                var fmt = new SpssVarFormat(0, (Byte)VarType, 1).Value;
                sw.WriteInt32(new Int32[] { 2, VarType, vlp, mvToBeWritten, fmt, fmt });
            }
            else
            {
                sw.WriteInt32(new Int32[] { 2, 255, vlp, mvToBeWritten, PrintFormat, WriteFormat });
            }

            sw.Write(Encoding.ASCII.GetBytes((GetVarname(index)).PadRight(8))); // always ASCII

            if (vlp == 1)
                WriteVariableLabel(sw, varLabel);

            SpssW = 1; // count chunks instead of bytes


            // -----------------------------------------------------------------------------

            /*
            // Now writing the extended string missing values here. -- APPARENTLY SHOULDN'T BE USED AT ALL, USE R7.22 INSTEAD
            var nEmv = 0;
            foreach (var missingValue in ExtendedStrMissingValues)
            {
                nEmv = nEmv + 1;
                if (nEmv > mvToBeWritten)
                    break; // write no more than max allowed missing values

                sw.Write(SpssEncoder.GetStringBytes(missingValue));
            }*/

            // -----------------------------------------------------------------------------


            Int32 nseg = VarLength > 255 ? (Int32)Math.Ceiling(VarLength / 252.0) : 1;

            var ncont = (Int32)((VarLength <= 255)
                                     ? Math.Ceiling(VarLength / 8.0) - 1
                                     : Math.Ceiling(255 / 8.0) - 1);
            WriteChunk(sw, ncont);
            SpssW = SpssW + ncont;

            // if necessary write additional segments
            var cseg = 2;
            var lastseg = VarLength - (nseg - 1) * 252;
            while (cseg <= nseg)
            {
                var spsstype = 255;

                if (cseg == nseg)
                {
                    spsstype = lastseg;
                    // pf and wf must be different for the last segment as well
                    var lastfmt = new SpssVarFormat(0, (byte)lastseg, 1);
                    PrintFormat = lastfmt.Value;
                    WriteFormat = lastfmt.Value;
                }

                sw.WriteInt32(new Int32[] { 2, spsstype, 0, 0, PrintFormat, WriteFormat });

                var vshort = GetSubVarname(index, cseg);
                sw.Write(Encoding.ASCII.GetBytes((vshort).PadRight(8))); // always ASCII

                SpssW = SpssW + 1;
                ncont = (Int32)(Math.Ceiling(spsstype / 8.0) - 1);
                WriteChunk(sw, ncont);
                SpssW = SpssW + ncont;
                cseg++;
            }
        }

        private void WriteVariableLabel(SpssBinaryWriter sw, string varLabel)
        {
            var vl0 = SpssEncoder.GetStringBytes4(varLabel);
            var vl4 = SpssEncoder.GetStringBytes(varLabel);
            sw.Write((Int32)vl4.Length);
            sw.Write(vl0); // limit 120 characters!
        }

        internal int MoveIndexDateTime()
        {
            VarType = 0;
            return 1;
        }

        internal int MoveIndexNumeric(int? decimals)
        {
            VarType = 0;
            SetFormat(decimals.HasValue
                          ? SpssVarFormat.NumericDecimalsFormat(decimals.Value)
                          : SpssVarFormat.DefaultNumericFormat);
            return 1;
        }

        internal int MoveIndexString(int maxlen)
        {
            VarType = maxlen;
            SetFormat(new SpssVarFormat(0, (Byte)maxlen, 1));
            VarLength = maxlen;
            PadLength = (Int32)Math.Ceiling(VarLength / 8.00) * 8;
            return (Int32)Math.Ceiling(maxlen / 8.00);            
        }


        /*
        internal int MoveIndex(bool allNumeric, int maxlen)
        {
            // if the format is already set to a date format trust it
            if (WriteFormat == SpssVarFormat.DefaultDateFormat.Value
                || WriteFormat==SpssVarFormat.DefaultDateTimeFormat.Value)
            {
                VarType = 0;
                return 1;
            }

            // returns increment to vindex
            if (allNumeric)
            {
                // numeric var
                VarType = 0;
                SetFormat(SpssVarFormat.DefaultNumericFormat);
                return 1;
            }
            else
            {
                // string var
                VarType = maxlen;
                SetFormat(new SpssVarFormat(0, (Byte)maxlen, 1));
                VarLength = maxlen;
                PadLength = (Int32) Math.Ceiling(VarLength/8.00)*8;

                return (Int32) Math.Ceiling(maxlen/8.00);
            }
        }*/

        private void SetFormat(SpssVarFormat fmt)
        {
            PrintFormat = fmt.Value;
            WriteFormat = fmt.Value;
        }

        private void WriteChunk(SpssBinaryWriter sw, int ncont)
        {
            for (var c = 1; c <= ncont; c++)
            {
                sw.WriteInt32(new Int32[] {2, -1, 0, 0});
                sw.Write(new byte[] {1, 29, 1, 0, 1, 29, 1, 0, 32, 32, 32, 32, 32, 32, 32, 32}); // SPSS seems to write this way
                //sw.Write(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }); // PSPP seems to write this way
            }
        }
    }
}
