using System;
using System.Linq;
using System.Runtime.CompilerServices;
using StatData.Core;

[assembly: InternalsVisibleTo("TestProject")]
namespace StatData.Writers.Stata
{
    internal class StataWriterHelper: StataWriterSimpleHelper, IDataAccessor
    {
        private readonly byte[] _types;
        internal int DataWidth;

        public string FileName()
        {
            return _data.GetName();
        }

        public StataWriterHelper(IDatasetMeta meta, IDataQuery data)
        {
            _meta = meta;
            _data = data;

            CheckVarNames();
            _types = DetectTypes(_data, _meta);
        }

        internal void CheckVarNames()
        {
            for (var v = 0; v < _meta.Variables.Length; v++)
            {
                var vname = _meta.Variables[v].VarName;
                if (StataVariable.IsInvalidVarName(vname))
                    throw new ArgumentException("Invalid variable name: " + vname);
            }
        }

        internal static byte[] DetectTypes(IDataQuery data)
        {
            return DetectTypes(data, DatasetMeta.FromData(data));
        }

        internal static byte[] DetectTypes(IDataQuery data, IDatasetMeta meta)
        {
            var varCount = meta.Variables.Length;
            var result = new byte[varCount];
            var width = new Int32[varCount];
            var measuredType = new byte[varCount];
            var allNumeric = new bool[varCount];

            for (var v = 0; v < varCount; v++)
            {
                if (meta.Variables[v].Storage == VariableStorage.NumericStorage) continue;

                width[v] = 1; // minimum width of the string variable
                measuredType[v] = StataConstants.VarTypeByte;

                if (v > data.GetColCount())
                {
                    result[v] = measuredType[v]; // if asked about the type of non-existing column - answer byte
                }
                else
                {
                    allNumeric[v] = true;
                    if (meta.Variables[v].Storage == VariableStorage.StringStorage)
                        allNumeric[v] = false;
                }
            }

            for (var i = 0; i < data.GetRowCount(); i++)
            {
                for (var v = 0; v < varCount; v++)
                {
                    if (meta.Variables[v].Storage == VariableStorage.NumericStorage) continue;

                    var value = data.GetValue(i, v);
                    var length = value.Length;
                    if (length > width[v]) width[v] = length;

                    if (!allNumeric[v] || StataCore.StringRepresentsNumericMissing(value)) continue;

                    double val;
                    if (Util.TryStringToDouble(value, meta.Culture, out val) == false)
                    {
                        allNumeric[v] = false;
                    }
                    else
                    {
                        // probe what value is this: byte, int, long, or double?
                        var t = StataVariable.DetectNumericType(val);
                        if (t > measuredType[v]) measuredType[v] = t;
                    }
                }
            }

            for (var v = 0; v < varCount; v++)
            {
                switch (meta.Variables[v].Storage)
                {
                    case (VariableStorage.NumericStorage):
                        // forced numeric: double is used regardless of the 
                        // actual values, meaning no types optimization
                        result[v] = StataConstants.VarTypeDouble;
                        break;
                    case (VariableStorage.StringStorage):
                        // forced string
                        if (width[v] > StataConstants.StringVarLength) width[v] = StataConstants.StringVarLength;
                        result[v] = (byte) width[v];
                        break;
                    default:
                        result[v] = allNumeric[v]
                                        ? measuredType[v]
                                        : (byte) ((width[v] > StataConstants.StringVarLength)
                                                      ? StataConstants.StringVarLength
                                                      : width[v]);
                        break;
                }
            }
            return result;
        }

        

        #region Implementation of IDataAccessor


        public bool IsVarNumeric(int v)
        {
            return StataVariable.IsVarNumeric(_types[v]);
        }

        public byte GetVarTypeEx(int v)
        {
            return _types[v];
        }

        public bool IsNumericVarInteger(int v)
        {
            return StataVariable.IsNumericVarInteger(_types[v]);
        }

        public byte GetVarWidth(int v)
        {
            if (IsVarNumeric(v))
            {
                throw new ArgumentException("Variable is numeric");
            }

            return _types[v];
        }


        

        public int DesiredCodepage()
        {
            var script = _meta.AppropriateScript;
            if (script == DatasetScript.Other)
                return StataConstants.CodePage;

            return DatasetEncoding.CodePage(script);
        }

        

        #endregion
    }
}
