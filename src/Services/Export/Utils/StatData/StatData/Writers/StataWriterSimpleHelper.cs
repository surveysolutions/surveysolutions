using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using StatData.Core;

namespace StatData.Writers
{
    class StataWriterSimpleHelper
    {
        internal IDatasetMeta _meta;
        internal IDataQuery _data;

        public UInt16 GetVarCount()
        {
            return (UInt16) _meta.Variables.Length;
        }

        public int GetObsCount()
        {
            return _data.GetRowCount();
        }

        public string GetVarName(int v)
        {
            return _meta.Variables[v].VarName;
        }

        public VariableStorage GetVarStorage(int v)
        {
            return _meta.Variables[v].Storage;
        }

        public void SetAsciiComment(string comment)
        {           
            _meta.SetAsciiComment(comment);
        }

        public string GetAsciiComment()
        {
            return _meta.GetAsciiComment();
        }

        public string GetDatasetLabel()
        {
            return _meta.DataLabel;
        }

        public string GetVarLabel(int v)
        {
            return _meta.Variables[v].VarLabel;
        }

        private ValueSet GetDct(int v)
        {
            var varname = _meta.Variables[v].VarName;
            if (String.IsNullOrEmpty(varname)) return null;

            var d = _meta.GetValueSet(varname);
            return d;
        }

        public int GetDctSize(int v)
        {
            var d = GetDct(v);
            return d == null ? 0 : d.Count;
        }

        public int GetDctCode(int v, int indx)
        {
            // todo: this works fine only if all the keys are integer and fit into an Int32

            var dct = GetDct(v);
            var key = dct.Keys.ToArray()[indx]; // double??

            // ban non-integers
            if (((key % 1) > 0) || ((key % 1) < 0))
            {
                var errMsg = "May not attach a value label to a non-integer value: " +
                             key.ToString(CultureInfo.InvariantCulture);
                throw new OverflowException(errMsg);
            }

            // ban out-of range
            Int32 key32;
            try
            {
                key32 = Convert.ToInt32(key);
            }
            catch(Exception)
            {
                var errMsg = "May not attach a value label to value: " +
                             key.ToString(CultureInfo.InvariantCulture);
                throw new OverflowException(errMsg);
            }

            var result = key32;
            return result;
        }

        public string GetDctLabel(int v, int indx)
        {
            var dct = GetDct(v);
            var key = dct.Keys.ToArray()[indx];

            return dct[key];
        }

        public string GetStringValue(int obs, int v)
        {
            var value = _data.GetValue(obs, v);
            return value;
        }

        public double GetNumericValue(int obs, int v)
        {
            var value = GetStringValue(obs, v);
            return Util.StringToDouble(value, _meta.Culture);
        }

        public bool IsValueMissing(int obs, int v)
        {
            var s = _data.GetValue(obs, v);
            return StataCore.StringRepresentsNumericMissing(s);
        }
        
        public bool IsValueExtendedMissingNumeric(int obs, int v)
        {
            var value = _data.GetValue(obs, v);
            return _meta.ExtendedMissings.IsMissing(value);
        }

        public int ExtendedMissingValueIndex(string value)
        {
            return _meta.ExtendedMissings.IndexOf(value);
        }

        public bool IsValueExtendedMissingString(int obs, int v)
        {
            if (_meta.ExtendedStrMissings.GetList().Count > 0)
            {
                throw new NotImplementedException("Do not supply the extended string missing values. Not supported yet!");
            }
            
            return false;

            /*
            var value = _data.GetValue(obs, v);
            return _meta.ExtendedStrMissings.IsMissing(value);
             */
        }

        public int ExtendedStrMissingValueIndex(string value)
        {
            if (_meta.ExtendedStrMissings.GetList().Count > 0)
            {
                throw new NotImplementedException("Do not supply the extended string missing values. Not supported yet!");
            }

            return -1;

            // return _meta.ExtendedStrMissings.IndexOf(value);
        }

        public DateTime GetTimeStamp()
        {
            return _meta.TimeStamp;
        }



        public ValueSet GetValueSet(string varname)
        {
            return _meta.GetValueSet(varname);
        }

        public IDatasetVariable GetVariable(int v)
        {
            return _meta.Variables[v];
        }

    }
}
