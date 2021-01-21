using System;
using System.Collections.Generic;

//! Implementation of Stata v14 writer
namespace StatData.Writers.Stata14
{
    class Stata14Constants
    {
        /// <summary>
        /// fixed default page
        /// </summary>
        //public const int CodePage = 1252;
        public const byte Specification = 118;
        public const byte Platform = 2;
        public const int VarNameLength = 32; // in characters
        public const int VarLabelLength = 80;
        public const int TimeStampWidth = 18;
        public const int FormatWidth = 56;

        public const int StringVarLength = 2045; // for strfs, for strls virtually unlimited

        public const UInt16 VarTypeDouble = 65526;
        public const UInt16 VarTypeFloat = 65527;
        public const UInt16 VarTypeLong = 65528;
        public const UInt16 VarTypeInt = 65529;
        public const UInt16 VarTypeByte = 65530;
        
        public const string DefaultFormatStr = "%9s";
        public const string DefaultFormatNum = "%6.2f";
        public const string DefaultFormatInt = "%6.0f";      // %11.0g <= with this format missing values are shown as exact; Stata's default for longs is %12.0g, for ints %8.0g, for bytes: %8.0g
        public const string DefaultFormatDate = "%tdCCYY-NN-DD";
        public const string DefaultFormatDateTime = "%tc";

        public const string DataLabel = "Exported data";

        // SAS manual incorrectly lists pi as Stata's reserved name, in fact _pi is reserved:
        // http://support.sas.com/documentation/cdl/en/acpcref/63184/HTML/default/viewer.htm#a003103776.htm
        public static readonly List<string> ReservedNames =
            new List<string>("byte int long float double if in _pi using with _all _b _n _N _cons _coef _rc _skip _pred _weight ".Split());
    }
}
