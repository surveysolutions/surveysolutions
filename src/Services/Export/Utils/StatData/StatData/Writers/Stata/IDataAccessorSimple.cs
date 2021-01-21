using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StatData.Core;

namespace StatData.Writers.Stata
{
    internal interface IDataAccessorSimple
    {
        string GetVarLabel(int v);
        DateTime GetTimeStamp();
        UInt16 GetVarCount();
        int GetObsCount();
        string GetDatasetLabel();
        string GetVarName(int v);
        VariableStorage GetVarStorage(int v);
        Int32 GetDctSize(int v);
        Int32 GetDctCode(int v, int indx);
        string GetDctLabel(int v, int indx);

        void SetAsciiComment(string comment);
        string GetAsciiComment();
    }
}
