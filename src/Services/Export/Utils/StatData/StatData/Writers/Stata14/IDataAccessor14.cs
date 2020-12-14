using System;
using StatData.Core;

namespace StatData.Writers.Stata14
{
    internal interface IDataAccessor14: StatData.Writers.Stata.IDataAccessorSimple
    {
        // meta
        bool IsVarNumeric(int v);
        bool IsNumericVarInteger(int v);
        UInt16 GetVarTypeEx(int v);
        UInt16 GetStrVarWidth(int v);
        //storage width for variable with index v

        //====CONTENT=====
        string GetStringValue(int obs, int v);
        double GetNumericValue(int obs, int v);

        bool IsValueMissing(int obs, int v);
        bool IsValueExtendedMissingNumeric(int obs, int v);
        int ExtendedMissingValueIndex(string value);

        bool IsValueExtendedMissingString(int obs, int v);
        int ExtendedStrMissingValueIndex(string value);

        ValueSet GetValueSet(string varname);
        IDatasetVariable GetVariable(int v);
    }

}
