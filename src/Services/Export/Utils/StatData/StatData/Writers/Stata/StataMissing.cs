using System;

namespace StatData.Writers.Stata
{
    internal class StataMissing
    {
        public static readonly string[] StataExtendedMissings =
            new[]
                {
                    ".a", ".b", ".c", ".d", ".e", ".f", ".g", ".h",
                    ".i", ".j", ".k", ".l", ".m", ".n", ".o", ".p",
                    ".q", ".r", ".s", ".t", ".u", ".v", ".w", ".x",
                    ".y", ".z"
                };

        public static byte[] GetBytes(byte varType)
        {
            switch (varType)
            {
                case StataConstants.VarTypeByte:
                    return StataCore.MissByte;
                case StataConstants.VarTypeInt:
                    return StataCore.MissInt;
                case StataConstants.VarTypeLong:
                    return StataCore.MissLong;
                case StataConstants.VarTypeFloat:
                    return StataCore.MissFloat;
                case StataConstants.VarTypeDouble:
                    return StataCore.MissDouble;
                default:
                    throw new ArgumentException("Unknown or non-numeric type");
            }
        }
    }
}
