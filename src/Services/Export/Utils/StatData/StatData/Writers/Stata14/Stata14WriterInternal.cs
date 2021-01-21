using System;
using System.IO;
using System.Text;
using StatData.Core;

namespace StatData.Writers.Stata14
{
    internal class Stata14WriterInternal
    {
        internal delegate void Progressor(double x);

        private Stream _fs;
        private StreamWriterSpecial _fw;
        private readonly IDataAccessor14 _data;
        private readonly Stata14FileMap _stata14FileMap;
        private readonly UInt16[] _map;

        // Only UTF-8 and no other encoding is applicable for Stata-14 data files
        private static readonly Encoding FileEncoding = Encoding.UTF8; 

        // Buffer size is currently inaccessible
        private readonly long _writeBufferSize;
        internal long WriteBufferSize
        {
            get
            {
                return _writeBufferSize;
            }
        }

        public Stata14WriterInternal(IDataAccessor14 d)
        {
            _writeBufferSize = 0;
            _data = d;
            _map = new UInt16[d.GetVarCount()]; // map of the types
            _stata14FileMap = new Stata14FileMap(); // map of the file
        }   // constructor

        public void Write(Stream s, Progressor p)
        {
            _fs = s;
            _fw = new StreamWriterSpecial(s);

            WriteHeader();
            WriteFileMap();
            WriteVarTypes();
            WriteVarNames();
            WriteSortOrder();
            WriteFormats();
            WriteValueLabelNames();
            WriteVariableLabels();
            WriteAsciiComment();
            WriteData(p);
            WriteStrls();
            WriteValueLabels();
            WriteFooter();
            WriteFreshMap();
            p(100.00);
        }

        private ulong CurrentStreamPosition()
        {
            return (ulong) _fs.Position;
        }

        private void WriteHeader()
        {
            ulong obscount = (ulong)_data.GetObsCount();

            _fw.WriteStr("<stata_dta><header><release>118</release><byteorder>LSF</byteorder>");

            _fw.WriteStr("<K>");
            _fw.Write((UInt16)_data.GetVarCount());
            _fw.WriteStr("</K>");

            _fw.WriteStr("<N>");
            _fw.Write(obscount);
            _fw.WriteStr("</N>");

            var dataLabel = _data.GetDatasetLabel();
            if (dataLabel.Length > Stata14Constants.VarLabelLength)
                dataLabel = dataLabel.Substring(0, Stata14Constants.VarLabelLength);

            var dataLabelBytes = FileEncoding.GetBytes(dataLabel);
            _fw.WriteStr("<label>");
            _fw.Write((UInt16)dataLabelBytes.Length);
            _fw.Write(dataLabelBytes);
            _fw.WriteStr("</label>");

            var dt = StataCore.StataDateTime(_data.GetTimeStamp());

            _fw.WriteStr("<timestamp>");
            _fw.Write(dt);
            _fw.WriteStr("</timestamp>");
            _fw.WriteStr("</header>");
        }

        private void WriteFileMap()
        {
            _stata14FileMap.OffMap = CurrentStreamPosition();
            _stata14FileMap.WriteToStream(_fw);
        }

        private void WriteVarTypes()
        {
            _stata14FileMap.OffVariableTypes = CurrentStreamPosition();

            _fw.WriteStr("<variable_types>");
            for (var v = 0; v < _data.GetVarCount(); v++)
            {
                _map[v] = _data.GetVarTypeEx(v);
                _fw.Write(_map[v]);
            }
            _fw.WriteStr("</variable_types>");
        }

        private void WriteVarNames()
        {
            _stata14FileMap.OffVarnames = CurrentStreamPosition();
            _fw.WriteStr("<varnames>");
            for (var v = 0; v < _data.GetVarCount(); v++)
            {
                var vname = _data.GetVarName(v);
                var vnameBytes = FileEncoding.GetBytes(vname);
                var vnameBytesPad = new byte[Stata14Constants.VarNameLength * 4 + 1];
                Array.Copy(vnameBytes, vnameBytesPad, vnameBytes.Length);
                _fw.Write(vnameBytesPad);
            }
            _fw.WriteStr("</varnames>");
        }

        private void WriteSortOrder()
        {
            _stata14FileMap.OffSortlist = CurrentStreamPosition();
            var varcount = _data.GetVarCount();
            // no information about sort order
            _fw.WriteStr("<sortlist>");
            _fw.Write(new byte[2 * (varcount + 1)]); // sort order
            _fw.WriteStr("</sortlist>");
        }

        private void WriteFormats()
        {
            _stata14FileMap.OffFormats = CurrentStreamPosition();
            _fw.WriteStr("<formats>");
            for (var v = 0; v < _data.GetVarCount(); v++) // formats
            {
                string fmt = PickFormatForVar(_data, v);
                _fw.Write(FileEncoding.GetBytes(fmt)); // all content must be ASCII anyways
                _fw.Write(new byte[Stata14Constants.FormatWidth - fmt.Length + 1]);
                // write padding to have 57-bytes per fields
            }
            _fw.WriteStr("</formats>");
        }

        private string PickFormatForVar(IDataAccessor14 data, int v)
        {
            if (data.IsVarNumeric(v))
                switch (data.GetVarStorage(v))
                {
                    case VariableStorage.DateStorage:
                        return Stata14Constants.DefaultFormatDate;
                    case VariableStorage.DateTimeStorage:
                        return Stata14Constants.DefaultFormatDateTime;
                    default:
                        if (data.IsNumericVarInteger(v))
                            return Stata14Constants.DefaultFormatInt;
                        var d = data.GetVariable(v).FormatDecimals;
                        if (d.HasValue) return StataCore.GetNumericFormatWithDecimals(d.Value);
                        return Stata14Constants.DefaultFormatNum;
                }
            return Stata14Constants.DefaultFormatStr;
        }

        private void WriteValueLabelNames()
        {
            _stata14FileMap.OffValueLabelNames = CurrentStreamPosition();
            _fw.WriteStr("<value_label_names>");
            for (var v = 0; v < _data.GetVarCount(); v++)
            {
                if (_data.GetDctSize(v) > 0)
                {
                    var dctName = _data.GetVarName(v);
                    var dctNameBytes = FileEncoding.GetBytes(dctName);
                    _fw.Write(dctNameBytes);
                    _fw.Write(new byte[Stata14Constants.VarNameLength * 4 + 1 - dctNameBytes.Length]); // padding
                }
                else
                {
                    _fw.Write(new byte[Stata14Constants.VarNameLength * 4 + 1]);
                }
            }
            _fw.WriteStr("</value_label_names>");
        }

        private void WriteVariableLabels()
        {
            _stata14FileMap.OffVariableLabels = CurrentStreamPosition();

            _fw.WriteStr("<variable_labels>");
            for (var v = 0; v < _data.GetVarCount(); v++) // variable labels
            {
                var lbl = _data.GetVarLabel(v);

                if (lbl.Length > Stata14Constants.VarLabelLength)
                    lbl = lbl.Substring(0, Stata14Constants.VarLabelLength);
                var varLabelBytes = FileEncoding.GetBytes(lbl);

                _fw.Write(varLabelBytes);
                _fw.Write(new byte[Stata14Constants.VarLabelLength * 4 + 1 - varLabelBytes.Length]);
            }
            _fw.WriteStr("</variable_labels>");
        }

        private void WriteAsciiComment()
        {
            _stata14FileMap.OffCharacteristics = CurrentStreamPosition();
            var asciiComment = _data.GetAsciiComment();
            if (String.IsNullOrEmpty(asciiComment))
            {
                _fw.WriteStr("<characteristics></characteristics>");
            }
            else
            {
                _fw.WriteStr("<characteristics><ch>");
                _fw.Write((UInt16)asciiComment.Length + 129 + 129 + 1);
                _fw.WriteStr("_dta".PadRight(129, '\0'));
                _fw.WriteStr("comment".PadRight(129, '\0'));
                _fw.WriteStr(asciiComment + '\0');
                _fw.WriteStr("</ch></characteristics>");
            }
        }

        private void WriteValueLabels()
        {
            _stata14FileMap.OffValueLabels = CurrentStreamPosition();
            _fw.WriteStr("<value_labels>");

            for (var v = 0; v < _data.GetVarCount(); v++)
            {
                var schema = new StataXValueSet(_data, v, FileEncoding);
                if (schema.Lbls.Count == 0) continue; // #### ATTENTION HERE, MAY NOT BE EXACTLY EQUIVALENT TO NULL LABEL!!!

                schema.Construct();
                _fw.WriteStr("<lbl>");
                _fw.Write(schema.ToBytes(Stata14Constants.VarNameLength * 4));
                _fw.WriteStr("</lbl>");
            }

            _fw.WriteStr("</value_labels>");
        }

        private void WriteData(Progressor p)
        {
            _stata14FileMap.OffData = CurrentStreamPosition();
            _fw.WriteStr("<data>");

            var varcount = _data.GetVarCount();
            var obscount = _data.GetObsCount();

            // cache types update observation width
            var numericTypes = new bool[varcount];
            var dataWidth = 0;
            for (var v = 0; v < varcount; v++)
            {
                numericTypes[v] = (_data.IsVarNumeric(v));
                dataWidth = dataWidth + Stata14Variable.GetVarWidth(_map[v]);
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
                    p(0.00);
                    for (var i = 0; i < (int)obscount; i++)
                    {
                        for (var v = 0; v < varcount; v++)
                        {
                            if (numericTypes[v])
                            {
                                // numeric
                                if (_data.GetVarStorage(v) == VariableStorage.DateStorage)
                                {
                                    var strv = _data.GetStringValue(i, v);
                                    if (_data.IsValueExtendedMissingString(i, v))
                                    {
                                        var mindex = _data.ExtendedStrMissingValueIndex(strv);

                                        memoryWriter.Write(
                                            Stata14Missing.GetBytes(
                                                _map[v],
                                                StataCore.GetMissingByIndex(mindex)));
                                    }
                                    else
                                    {
                                        memoryWriter.Write(StataCore.GetDaysDate(strv));
                                    }
                                    continue;
                                }

                                if (_data.GetVarStorage(v) == VariableStorage.DateTimeStorage)
                                {
                                    var strv = _data.GetStringValue(i, v);
                                    if (_data.IsValueExtendedMissingString(i, v))
                                    {
                                        var mindex = _data.ExtendedStrMissingValueIndex(strv);

                                        memoryWriter.Write(
                                            Stata14Missing.GetBytes(
                                                _map[v],
                                                StataCore.GetMissingByIndex(mindex)));
                                    }
                                    else
                                    {
                                        double msec = StataCore.GetMsecTime(strv);
                                        memoryWriter.Write(msec);
                                    }
                                    continue;
                                }


                                if (_data.IsValueMissing(i, v))
                                {
                                    // write system missing value
                                    memoryWriter.Write(Stata14Missing.GetBytes(_map[v], "."));
                                }
                                else
                                {
                                    if (_data.IsValueExtendedMissingNumeric(i, v))
                                    {
                                        // write extended missing value
                                        var mindex = _data.ExtendedMissingValueIndex(_data.GetStringValue(i, v));
                                        mindex = Math.Min(mindex, 26);
                                        memoryWriter.Write(
                                            Stata14Missing.GetBytes(
                                                _map[v],
                                                StataCore.GetMissingByIndex(mindex)));
                                        continue;
                                    }

                                    var x = _data.GetNumericValue(i, v);
                                    switch (_map[v])
                                    {
                                        case Stata14Constants.VarTypeDouble:
                                            memoryWriter.Write(x);
                                            break;
                                        case Stata14Constants.VarTypeFloat:
                                            memoryWriter.Write(Convert.ToSingle(x));
                                            break;
                                        case Stata14Constants.VarTypeLong:
                                            memoryWriter.Write(Convert.ToInt32(x));
                                            break;
                                        case Stata14Constants.VarTypeInt:
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
                                var w = _data.GetStrVarWidth(v);
                                var strv = _data.GetStringValue(i, v);
                                var strBytes = FileEncoding.GetBytes(strv);

                                if (strBytes.Length >= w)
                                {
                                    // cannot write W bytes because this may be an invalid Unicode string: memoryWriter.Write(strBytes, 0, w);
                                    strv = LimitByteLength2(strv, w);
                                    strBytes = FileEncoding.GetBytes(strv);
                                    memoryWriter.Write(strBytes);
                                }
                                else
                                {
                                    // todo: inspect whether this is necessary???
                                    strv = strv.PadRight(w, Convert.ToChar(0));

                                    memoryWriter.Write(strBytes);
                                    memoryWriter.Write(
                                        FileEncoding.GetBytes("".PadRight(w - strBytes.Length, Convert.ToChar(0))));
                                }
                            }
                        }

                        p(100.00 * i / obscount);

                        if (bufWrite)
                        {
                            bufOcc++;
                            if (bufOcc != bufCap) continue;
                            bufOcc = 0;
                        }
                        _fw.Write(buffer);
                        memoryWriter.BaseStream.Seek(0, SeekOrigin.Begin);
                    }
                }

                // write the remainder of the buffer
                if (bufWrite)
                {
                    if (bufOcc > 0)
                    {
                        _fw.Write(buffer, 0, dataWidth * bufOcc);
                    }
                }
            }

            _fw.WriteStr("</data>");
        }

        private void WriteStrls()
        {
            _stata14FileMap.OffStrls = CurrentStreamPosition();
            _fw.WriteStr("<strls></strls>");
        }

        private void WriteFreshMap()
        {
            _fs.Seek((long)_stata14FileMap.OffMap, SeekOrigin.Begin);
            _stata14FileMap.WriteToStream(_fw);
        }

        private void WriteFooter()
        {
            _stata14FileMap.OffStataDataEnd = CurrentStreamPosition();
            _fw.WriteStr("</stata_dta>");
            _stata14FileMap.OffEndOfFile = CurrentStreamPosition();
        }

        private static String LimitByteLength2(String input, Int32 maxLength)
        {
            for (Int32 i = input.Length - 1; i >= 0; i--)
            {
                if (FileEncoding.GetByteCount(input.Substring(0, i + 1)) <= maxLength)
                {
                    return input.Substring(0, i + 1);
                }
            }

            return String.Empty;
        }
    }

}
