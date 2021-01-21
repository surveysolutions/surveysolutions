using System;
using System.IO;
using StatData.Core;

namespace StatData.Writers.Stata
{
    internal interface IDataAccessor : IDataAccessorSimple
    {
        // meta
        bool IsVarNumeric(int v);
        bool IsNumericVarInteger(int v);
        //storage: numeric integer => long; numeric fractional => double; string => str244
        byte GetVarTypeEx(int v);
        byte GetVarWidth(int v);
        //storage width for variable with index v

        //====CONTENT=====
        string FileName();

        string GetStringValue(int obs, int v);
        double GetNumericValue(int obs, int v);

        bool IsValueMissing(int obs, int v);
        bool IsValueExtendedMissingNumeric(int obs, int v);
        int DesiredCodepage();

        ValueSet GetValueSet(string varname);
    }
        
}
