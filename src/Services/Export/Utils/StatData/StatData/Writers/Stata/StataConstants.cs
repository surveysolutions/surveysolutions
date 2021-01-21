using System.Collections.Generic;

//! Implementation of Stata writer
namespace StatData.Writers.Stata
{
    internal class StataConstants
    {
        /// <summary>
        /// fixed default page
        /// </summary>
        public const int CodePage = 1252;
        public const byte Specification = 114;
        public const byte Platform = 2;
        public const int VarNameLength = 32;
        public const int VarLabelLength = 80;
        public const int TimeStampWidth = 18;
        public const int FormatWidth = 49;

        public const int StringVarLength = 244;

        public const byte VarTypeDouble = 255;
        public const byte VarTypeFloat = 254;
        public const byte VarTypeLong = 253;
        public const byte VarTypeInt = 252;
        public const byte VarTypeByte = 251;
        
        public const string DefaultFormatStr = "%9s";
        public const string DefaultFormatNum = "%6.2f";
        public const string DefaultFormatInt = "%6.0f";

        public const string DataLabel = "Exported data";
        public const int MaxNumericMissingValues = 26;

        // SAS manual incorrectly lists pi as Stata's reserved name, in fact _pi is reserved:
        // http://support.sas.com/documentation/cdl/en/acpcref/63184/HTML/default/viewer.htm#a003103776.htm
        public static readonly List<string> ReservedNames =
            new List<string>("byte int long float double if in _pi using with _all _b _n _N _cons _coef _rc _skip _pred _weight ".Split());
    }

    internal enum StataVariableTypes
    {
        StataByte,
        StataInt,
        StataLong,
        StataFloat,
        StataDouble,
        StataShortString,
        StataLongString,
        StataUnknown
    }
}
