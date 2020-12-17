using System;
using System.Collections.Generic;
using System.Globalization;

//! Implementation of SPSS writer
namespace StatData.Writers.Spss
{
    internal class SpssConstants
    {
        public const UInt32 EncodingCode = 65001;
        public const byte FillerByte = 32;
        public const string Signature = "$FL2";
        public const string Signature2 = "@(#) SPSS DATA FILE"; // this is not written in this version, but can be used as a signature starter
        public const string MissChar = ".";
        public static readonly byte[] SysMiss = new byte[] {255, 255, 255, 255, 255, 255, 239, 255};
        public static readonly byte[] V1 = new byte[] {255, 255, 255, 255, 255, 255, 239, 255};
        public static readonly byte[] V2 = new byte[] {255, 255, 255, 255, 255, 255, 239, 127};
        public static readonly byte[] V3 = new byte[] {254, 255, 255, 255, 255, 255, 239, 255};

        public const int ProductWidth = 60;
        public const int ValueLabelWidth = 60; // according to http://www-01.ibm.com/support/docview.wss?uid=swg21478834 this limit may be as big as 120 bytes, but not clear from which version.
        public const int DataLabelWidth = 64;
        public const double CompressionBias = 100.0;
        public const int MaxStrWidth = 32767;
        public const int SpssCommentLineWidth = 80;

        public static readonly DateTime ZeroDay = new DateTime(1582, 10, 14);

        public static string Product = StatData.Core.Info.Name;

        public static readonly List<string> ReservedNames = 
            new List<string>("ALL AND BY EQ GE GT LE LT NE NOT OR TO WITH".Split());

        public static readonly CultureInfo Cult = CultureInfo.InvariantCulture;

        public const int MaxNumericMissingValues = 3;
        public const int MaxStringMissingValues = 3;
    }
}
