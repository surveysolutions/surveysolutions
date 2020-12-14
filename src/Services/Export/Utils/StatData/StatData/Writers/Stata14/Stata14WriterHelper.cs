using System;
using System.Text;
using System.Runtime.CompilerServices;
using StatData.Core;

[assembly: InternalsVisibleTo("TestProject")]
namespace StatData.Writers.Stata14
{
    internal class Stata14WriterHelper : StataWriterSimpleHelper, IDataAccessor14
    {
        private readonly UInt16[] _types;
        internal int DataWidth;

        public Stata14WriterHelper(IDatasetMeta meta, IDataQuery data)
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
                if (Stata14Variable.IsInvalidVarName(vname))
                    throw new ArgumentException("Invalid variable name: " + vname);
            }
        }

        internal static UInt16[] DetectTypes(IDataQuery data)
        {
            return DetectTypes(data, DatasetMeta.FromData(data));
        }

        internal static UInt16[] DetectTypes(IDataQuery data, IDatasetMeta meta)
        {
            var varCount = meta.Variables.Length;
            var result = new UInt16[varCount];
            var width = new Int32[varCount];
            var measuredType = new UInt16[varCount];
            var allNumeric = new bool[varCount];

            for (var v = 0; v < varCount; v++)
            {
                if (meta.Variables[v].Storage == VariableStorage.NumericStorage) continue;

                width[v] = 1; // minimum width of the string variable
                measuredType[v] = Stata14Constants.VarTypeByte;

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
                    if (meta.Variables[v].Storage == VariableStorage.DateStorage) continue;
                    if (meta.Variables[v].Storage == VariableStorage.DateTimeStorage) continue;

                    var value = data.GetValue(i, v);
                    var length = Encoding.UTF8.GetBytes(value).Length;
                    if (length > width[v]) width[v] = length; // we care about bytes, not characters

                    if (!allNumeric[v] 
                        || StataCore.StringRepresentsNumericMissing(value)
                        || meta.ExtendedMissings.IsMissing(value)
                        ) continue;

                    double val;
                    if (Util.TryStringToDouble(value, meta.Culture, out val) == false)
                    {
                        allNumeric[v] = false;
                    }
                    else
                    {
                        // probe what value is this: byte, int, long, or double?
                        var t = Stata14Variable.DetectNumericType(val);
                        if (t < measuredType[v])
                            measuredType[v] = t; // for Stata14 wider types have lower numeric type codes
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
                        result[v] = Stata14Constants.VarTypeDouble;
                        break;
                    case (VariableStorage.StringStorage):
                        // forced string
                        if (width[v] > Stata14Constants.StringVarLength) width[v] = Stata14Constants.StringVarLength;
                        result[v] = (ushort) width[v];
                        break;
                    case (VariableStorage.DateStorage) :
                        result[v] = Stata14Constants.VarTypeLong;
                        break;
                    case (VariableStorage.DateTimeStorage) :
                        result[v] = Stata14Constants.VarTypeDouble;
                        break;
                    default:
                        result[v] = allNumeric[v]
                                        ? measuredType[v]
                                        : (UInt16) ((width[v] > Stata14Constants.StringVarLength)
                                                      ? Stata14Constants.StringVarLength
                                                      : width[v]);
                        break;
                }
            }
            return result;
        }

        

        #region Implementation of IDataAccessor

        
        public bool IsVarNumeric(int v)
        {
            return Stata14Variable.IsVarTypeNumeric(_types[v]);
        }
        
        public UInt16 GetVarTypeEx(int v)
        {
            return _types[v];
        }
        
        public bool IsNumericVarInteger(int v)
        {
            return Stata14Variable.IsNumericVarInteger(_types[v]);
        }
        
        public UInt16 GetStrVarWidth(int v)
        {
            if (IsVarNumeric(v) )
            {
                throw new ArgumentException("Variable is numeric");
            }

            return _types[v];
        }
       
        #endregion
    }
}
