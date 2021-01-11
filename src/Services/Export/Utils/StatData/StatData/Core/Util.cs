using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace StatData.Core
{
    internal class Util
    {
        internal const NumberStyles Styles = NumberStyles.Number | NumberStyles.AllowExponent;
        internal static readonly CultureInfo DefaultCulture = CultureInfo.InvariantCulture;

        internal static double StringToDouble(string s, CultureInfo cultureInfo)
        {
            var result = Double.Parse(s, Styles, cultureInfo);
            return result;
        }

        internal static Boolean TryStringToDouble(string s, CultureInfo cultureInfo, out double value)
        {
            double v;
            var result = Double.TryParse(s, Styles, cultureInfo, out v);
            value = v;
            return result;
        }

        internal static Boolean ValueSetsAreIdentical(ValueSet s1, ValueSet s2)
        {
            if (s1.Count != s2.Count) return false;
            foreach (var k in s1.Keys)
            {
                try
                {
                    if (s1[k] != s2[k]) return false;
                }
                catch(Exception exception)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
