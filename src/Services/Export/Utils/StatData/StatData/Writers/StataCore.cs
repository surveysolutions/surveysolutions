using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StatData.Writers.Stata;

namespace StatData.Writers
{
    internal class StataCore
    {
        public static byte[] GetBytes(string value)
        {
            return MissingValues[value];
        }

        public static readonly Dictionary<string, byte[]> MissingValues
            = new Dictionary<string, byte[]>
                  {
                      // ===== BYTE TYPE ================================
                      // System missing value
                      {"B.", new byte[] {101}},
                      // Extended missing values
                      {"B.a", new byte[] {102}},
                      {"B.b", new byte[] {103}},
                      {"B.c", new byte[] {104}},
                      {"B.d", new byte[] {105}},
                      {"B.e", new byte[] {106}},
                      {"B.f", new byte[] {107}},
                      {"B.g", new byte[] {108}},
                      {"B.h", new byte[] {109}},
                      {"B.i", new byte[] {110}},
                      {"B.j", new byte[] {111}},
                      {"B.k", new byte[] {112}},
                      {"B.l", new byte[] {113}},
                      {"B.m", new byte[] {114}},
                      {"B.n", new byte[] {115}},
                      {"B.o", new byte[] {116}},
                      {"B.p", new byte[] {117}},
                      {"B.q", new byte[] {118}},
                      {"B.r", new byte[] {119}},
                      {"B.s", new byte[] {120}},
                      {"B.t", new byte[] {121}},
                      {"B.u", new byte[] {122}},
                      {"B.v", new byte[] {123}},
                      {"B.w", new byte[] {124}},
                      {"B.x", new byte[] {125}},
                      {"B.y", new byte[] {126}},
                      {"B.z", new byte[] {127}},

                      // ===== INTEGER TYPE ============================
                      // System missing value
                      {"I.", new byte[] {229, 127}},
                      // Extended missing values
                      {"I.a", new byte[] {230, 127}},
                      {"I.b", new byte[] {231, 127}},
                      {"I.c", new byte[] {232, 127}},
                      {"I.d", new byte[] {233, 127}},
                      {"I.e", new byte[] {234, 127}},
                      {"I.f", new byte[] {235, 127}},
                      {"I.g", new byte[] {236, 127}},
                      {"I.h", new byte[] {237, 127}},
                      {"I.i", new byte[] {238, 127}},
                      {"I.j", new byte[] {239, 127}},
                      {"I.k", new byte[] {240, 127}},
                      {"I.l", new byte[] {241, 127}},
                      {"I.m", new byte[] {242, 127}},
                      {"I.n", new byte[] {243, 127}},
                      {"I.o", new byte[] {244, 127}},
                      {"I.p", new byte[] {245, 127}},
                      {"I.q", new byte[] {246, 127}},
                      {"I.r", new byte[] {247, 127}},
                      {"I.s", new byte[] {248, 127}},
                      {"I.t", new byte[] {249, 127}},
                      {"I.u", new byte[] {250, 127}},
                      {"I.v", new byte[] {251, 127}},
                      {"I.w", new byte[] {252, 127}},
                      {"I.x", new byte[] {253, 127}},
                      {"I.y", new byte[] {254, 127}},
                      {"I.z", new byte[] {255, 127}},


                      // ===== LONG TYPE ====================================
                      // System missing value
                      {"L.", new byte[] {229, 255, 255, 127}},
                      // Extended missing values
                      {"L.a", new byte[] {230, 255, 255, 127}},
                      {"L.b", new byte[] {231, 255, 255, 127}},
                      {"L.c", new byte[] {232, 255, 255, 127}},
                      {"L.d", new byte[] {233, 255, 255, 127}},
                      {"L.e", new byte[] {234, 255, 255, 127}},
                      {"L.f", new byte[] {235, 255, 255, 127}},
                      {"L.g", new byte[] {236, 255, 255, 127}},
                      {"L.h", new byte[] {237, 255, 255, 127}},
                      {"L.i", new byte[] {238, 255, 255, 127}},
                      {"L.j", new byte[] {239, 255, 255, 127}},
                      {"L.k", new byte[] {240, 255, 255, 127}},
                      {"L.l", new byte[] {241, 255, 255, 127}},
                      {"L.m", new byte[] {242, 255, 255, 127}},
                      {"L.n", new byte[] {243, 255, 255, 127}},
                      {"L.o", new byte[] {244, 255, 255, 127}},
                      {"L.p", new byte[] {245, 255, 255, 127}},
                      {"L.q", new byte[] {246, 255, 255, 127}},
                      {"L.r", new byte[] {247, 255, 255, 127}},
                      {"L.s", new byte[] {248, 255, 255, 127}},
                      {"L.t", new byte[] {249, 255, 255, 127}},
                      {"L.u", new byte[] {250, 255, 255, 127}},
                      {"L.v", new byte[] {251, 255, 255, 127}},
                      {"L.w", new byte[] {252, 255, 255, 127}},
                      {"L.x", new byte[] {253, 255, 255, 127}},
                      {"L.y", new byte[] {254, 255, 255, 127}},
                      {"L.z", new byte[] {255, 255, 255, 127}},

                      // ===== FLOAT TYPE ===========================================
                      // System missing value
                      {"F.", new byte[] {0, 0, 0, 127}},
                      // Extended missing values (TO BE CONFIRMED!!!!!!!!!!!)
                      {"F.a", new byte[] {0, 8, 0, 127}},
                      {"F.b", new byte[] {0, 16, 0, 127}},
                      {"F.c", new byte[] {0, 24, 0, 127}},
                      {"F.d", new byte[] {0, 32, 0, 127}},
                      {"F.e", new byte[] {0, 40, 0, 127}},
                      {"F.f", new byte[] {0, 48, 0, 127}},
                      {"F.g", new byte[] {0, 56, 0, 127}},
                      {"F.h", new byte[] {0, 64, 0, 127}},
                      {"F.i", new byte[] {0, 72, 0, 127}},
                      {"F.j", new byte[] {0, 80, 0, 127}},
                      {"F.k", new byte[] {0, 88, 0, 127}},
                      {"F.l", new byte[] {0, 96, 0, 127}},
                      {"F.m", new byte[] {0, 104, 0, 127}},
                      {"F.n", new byte[] {0, 112, 0, 127}},
                      {"F.o", new byte[] {0, 120, 0, 127}},
                      {"F.p", new byte[] {0, 128, 0, 127}},
                      {"F.q", new byte[] {0, 136, 0, 127}},
                      {"F.r", new byte[] {0, 144, 0, 127}},
                      {"F.s", new byte[] {0, 152, 0, 127}},
                      {"F.t", new byte[] {0, 160, 0, 127}},
                      {"F.u", new byte[] {0, 168, 0, 127}},
                      {"F.v", new byte[] {0, 176, 0, 127}},
                      {"F.w", new byte[] {0, 184, 0, 127}},
                      {"F.x", new byte[] {0, 192, 0, 127}},
                      {"F.y", new byte[] {0, 200, 0, 127}},
                      {"F.z", new byte[] {0, 208, 0, 127}},

                      // ===== DOUBLE TYPE ============================================
                      // System missing value
                      {"D.", new byte[] {0, 0, 0, 0, 0, 0, 224, 127}},
                      // Extended missing values (TO BE CONFIRMED!!!!!!!!!!!)
                      {"D.a", new byte[] {0, 0, 0, 0, 0, 1, 224, 127}},
                      {"D.b", new byte[] {0, 0, 0, 0, 0, 2, 224, 127}},
                      {"D.c", new byte[] {0, 0, 0, 0, 0, 3, 224, 127}},
                      {"D.d", new byte[] {0, 0, 0, 0, 0, 4, 224, 127}},
                      {"D.e", new byte[] {0, 0, 0, 0, 0, 5, 224, 127}},
                      {"D.f", new byte[] {0, 0, 0, 0, 0, 6, 224, 127}},
                      {"D.g", new byte[] {0, 0, 0, 0, 0, 7, 224, 127}},
                      {"D.h", new byte[] {0, 0, 0, 0, 0, 8, 224, 127}},
                      {"D.i", new byte[] {0, 0, 0, 0, 0, 9, 224, 127}},
                      {"D.j", new byte[] {0, 0, 0, 0, 0, 10, 224, 127}},
                      {"D.k", new byte[] {0, 0, 0, 0, 0, 11, 224, 127}},
                      {"D.l", new byte[] {0, 0, 0, 0, 0, 12, 224, 127}},
                      {"D.m", new byte[] {0, 0, 0, 0, 0, 13, 224, 127}},
                      {"D.n", new byte[] {0, 0, 0, 0, 0, 14, 224, 127}},
                      {"D.o", new byte[] {0, 0, 0, 0, 0, 15, 224, 127}},
                      {"D.p", new byte[] {0, 0, 0, 0, 0, 16, 224, 127}},
                      {"D.q", new byte[] {0, 0, 0, 0, 0, 17, 224, 127}},
                      {"D.r", new byte[] {0, 0, 0, 0, 0, 18, 224, 127}},
                      {"D.s", new byte[] {0, 0, 0, 0, 0, 19, 224, 127}},
                      {"D.t", new byte[] {0, 0, 0, 0, 0, 20, 224, 127}},
                      {"D.u", new byte[] {0, 0, 0, 0, 0, 21, 224, 127}},
                      {"D.v", new byte[] {0, 0, 0, 0, 0, 22, 224, 127}},
                      {"D.w", new byte[] {0, 0, 0, 0, 0, 23, 224, 127}},
                      {"D.x", new byte[] {0, 0, 0, 0, 0, 24, 224, 127}},
                      {"D.y", new byte[] {0, 0, 0, 0, 0, 25, 224, 127}},
                      {"D.z", new byte[] {0, 0, 0, 0, 0, 26, 224, 127}}
                  };
        
        
        public const string MissChar = ".";

        public static readonly byte[] MissByte = new byte[] { 101 };
        public static readonly byte[] MissInt = new byte[] { 229, 127 };
        public static readonly byte[] MissLong = new byte[] { 229, 255, 255, 127 };
        public static readonly byte[] MissFloat = new byte[] { 0, 0, 0, 127 };
        public static readonly byte[] MissDouble = new byte[] { 0, 0, 0, 0, 0, 0, 224, 127 };

        internal static string StataDateTime(DateTime date)
        {
            // E.g.: "10 Jun 2013 19:21";

            var c = CultureInfo.InvariantCulture;
            var ms = new[] { "XYZ", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

            var d = date.Day.ToString(c);
            if (d.Length == 1) d = " " + d; // days are padded with spaces
            var mo = ms[date.Month];
            var y = date.Year.ToString(c);
            var h = date.Hour.ToString(c);
            if (h.Length == 1) h = "0" + h; // hours are padded with zeroes, can do "00" mask as well
            var m = date.Minute.ToString(c);
            if (m.Length == 1) m = "0" + m; // minutes are padded with zeroes , can do "00" mask as well

            var result = string.Format("{0} {1} {2} {3}:{4}", d, mo, y, h, m);
            return result;
        }

        internal static readonly DateTime ZeroDay = new DateTime(1960, 1, 1);
        internal static double GetMsecTime(string timevalue)
        {
            // compute number of msec since ZeroDay
            var cleanTimeValue = timevalue.Replace("T", " ").Replace("Z", "");
            var dateTimeValue = Convert.ToDateTime(cleanTimeValue);
            return (dateTimeValue - ZeroDay).TotalMilliseconds;
        }

        internal static Int32 GetDaysDate(string datevalue)
        {
            // compute number of days since ZeroDay
            Int32 days = (Convert.ToDateTime(datevalue) - ZeroDay).Days;
            return days;
        }

        public static bool StringRepresentsNumericMissing(string s)
        {
            if (string.IsNullOrEmpty(s))
                return true;

            if (s == MissChar)
                return true;

            return false;
        }

        public static string GetMissingByIndex(int index)
        {
            return StataMissing.StataExtendedMissings[index];
        }

        public static string GetNumericFormatWithDecimals(int d)
        {
            // The default format (%10.0g) that Stata is using when generating 
            // variables of type double is not suitable, since it doesn't
            // guarantee a fixed number of decimal points, but a fixed number 
            // of meaningful digits.
            return "%16." + d.ToString(CultureInfo.InvariantCulture) + "f";
        }
    }
}
