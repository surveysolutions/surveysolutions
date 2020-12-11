using System;
using System.Text.RegularExpressions;

namespace StatData.Writers.Stata
{
    class StataVariable
    {
        internal static byte DetectNumericType(double val)
        {
            if (val != Math.Round(val)) return StataConstants.VarTypeDouble;
            if (val > 2147483620) return StataConstants.VarTypeDouble;
            if (val < -2147483647) return StataConstants.VarTypeDouble;
            if (val < -32767) return StataConstants.VarTypeLong;
            if (val > 32740) return StataConstants.VarTypeLong;
            if (val < -128) return StataConstants.VarTypeInt;
            if (val > 100) return StataConstants.VarTypeInt;
            return StataConstants.VarTypeByte;
        }

        internal static bool IsInvalidVarName(string name)
        {
            // too short
            if (String.IsNullOrEmpty(name)) return true;

            // too long
            if (name.Length > StataConstants.VarNameLength)
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
            if (StataConstants.ReservedNames.Contains(name)) return true;

            // is a strNNN name (old pattern @"str(\d)$")
            if (Regex.Match(name, @"^str[0-9]+$").Success) return true;

            return false;

        }

        internal static bool IsVarNumeric(byte t)
        {
            switch (t)
            {
                case StataConstants.VarTypeDouble:
                case StataConstants.VarTypeFloat:
                case StataConstants.VarTypeLong:
                case StataConstants.VarTypeInt:
                case StataConstants.VarTypeByte: return true;
                default: return false;
            }
        }

        internal static int GetVarWidth(byte t)
        {
            switch (t)
            {
                case StataConstants.VarTypeDouble:
                    return 8;
                case StataConstants.VarTypeFloat:
                    return 4;
                case StataConstants.VarTypeLong:
                    return 4;
                case StataConstants.VarTypeInt:
                    return 2;
                case StataConstants.VarTypeByte:
                    return 1;
                default:
                    return t;
            }
        }

        internal static bool IsNumericVarInteger(byte t)
        {
            switch (t)
            {
                case StataConstants.VarTypeByte:
                    return true;
                case StataConstants.VarTypeInt:
                    return true;
                case StataConstants.VarTypeLong:
                    return true;
                case StataConstants.VarTypeFloat:
                    return false;
                case StataConstants.VarTypeDouble:
                    return false;
                default:
                    throw new ArgumentException("Query not applicable for string variables");
            }
        }
    }
}
