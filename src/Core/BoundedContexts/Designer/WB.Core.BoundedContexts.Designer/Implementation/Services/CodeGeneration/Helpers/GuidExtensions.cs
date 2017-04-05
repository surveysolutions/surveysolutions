using System;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Helpers
{
    public static class GuidExtensions
    {
        public static string AsBytesString(this Guid guid)
        {
            var byteArray = guid.ToByteArray();

            var a = (byteArray[3] << 24) | (byteArray[2] << 16) | (byteArray[1] << 8) | byteArray[0];
            var b = (short) ((byteArray[5] << 8) | byteArray[4]);
            var c = (short) ((byteArray[7] << 8) | byteArray[6]);
            var d = byteArray[8];
            var e = byteArray[9];
            var f = byteArray[10];
            var g = byteArray[11];
            var h = byteArray[12];
            var i = byteArray[13];
            var j = byteArray[14];
            var k = byteArray[15];

            return $"new System.Guid({a}, {b}, {c}, {d}, {e}, {f}, {g}, {h}, {i}, {j}, {k}) /* {guid} */";
        }
    }
}