using System;
using System.Text.RegularExpressions;

namespace StatData.Writers.Stata14
{
    class Stata14Variable
    {
        internal static UInt16 DetectNumericType(double val)
        {
            if (val != Math.Round(val)) return Stata14Constants.VarTypeDouble;
            if (val > 2147483620) return Stata14Constants.VarTypeDouble;
            if (val < -2147483647) return Stata14Constants.VarTypeDouble;
            if (val < -32767) return Stata14Constants.VarTypeLong;
            if (val > 32740) return Stata14Constants.VarTypeLong;
            if (val < -127) return Stata14Constants.VarTypeInt;
            if (val > 100) return Stata14Constants.VarTypeInt;
            return Stata14Constants.VarTypeByte;
        }

        internal static bool IsInvalidVarName(string name)
        {
            // too short
            if (String.IsNullOrEmpty(name)) return true;

            // too long
            if (name.Length > Stata14Constants.VarNameLength)
                return true;

            // contains invalid char
            var nameU = name.ToUpper();

            const string validchars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ_";
            foreach (var c in nameU)
                if (validchars.IndexOf(c) < 0) return true;

            // starts or ends with an underscore
            if (name.StartsWith("_") | name.EndsWith("_")) return true;

            // starts with a digit
            if ("0123456789".Contains(name.Substring(0, 1))) return true;

            // is a reserved name
            if (Stata14Constants.ReservedNames.Contains(name)) return true;

            // is a strNNN name (old expression: str(\d)$   )
            if (Regex.Match(name, @"^str[0-9]+$").Success) return true;

            return false;

        }

        internal static bool IsVarTypeNumeric(UInt16 t)
        {
            switch (t)
            {
                case Stata14Constants.VarTypeDouble:
                case Stata14Constants.VarTypeFloat: 
                case Stata14Constants.VarTypeLong:
                case Stata14Constants.VarTypeInt:
                case Stata14Constants.VarTypeByte: return true;
                default: return false;
            }
        }

        internal static int GetVarWidth(UInt16 t)
        {
            switch (t)
            {
                case Stata14Constants.VarTypeDouble:
                    return 8;
                case Stata14Constants.VarTypeFloat:
                    return 4;
                case Stata14Constants.VarTypeLong:
                    return 4;
                case Stata14Constants.VarTypeInt:
                    return 2;
                case Stata14Constants.VarTypeByte:
                    return 1;
                default:
                    return t; // todo: need to revise this for strings
            }
        }

        internal static bool IsNumericVarInteger(UInt16 t)
        {
            switch (t)
            {
                case Stata14Constants.VarTypeByte:
                    return true;
                case Stata14Constants.VarTypeInt:
                    return true;
                case Stata14Constants.VarTypeLong:
                    return true;
                case Stata14Constants.VarTypeFloat:
                    return false;
                case Stata14Constants.VarTypeDouble:
                    return false;
                default:
                    throw new ArgumentException("Query not applicable for string variables");
            }
        }
    }
}
