using System;

namespace StatData.Writers.Stata14
{
    internal class Stata14Missing
    {
        public static byte[] GetBytes(UInt16 varType, string mv)
        {
            switch (varType)
            {
                case Stata14Constants.VarTypeByte:
                    return StataCore.GetBytes("B" + mv);
                case Stata14Constants.VarTypeInt:
                    return StataCore.GetBytes("I" + mv);
                case Stata14Constants.VarTypeLong:
                    return StataCore.GetBytes("L" + mv);
                case Stata14Constants.VarTypeFloat:
                    return StataCore.GetBytes("F" + mv);
                case Stata14Constants.VarTypeDouble:
                    return StataCore.GetBytes("D" + mv);
                default:
                    throw new ArgumentException("Unknown or non-numeric type");
            }
        }
    }
}
