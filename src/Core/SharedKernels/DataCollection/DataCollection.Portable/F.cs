using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace WB.Core.SharedKernels.DataCollection
{
    public static class F
    {
        /// <summary>
        /// Checks if the value is within a closed interval defined by its boundary points
        /// </summary>
        /// <param name="x">Value being tested</param>
        /// <param name="l">Low boundary of the interval</param>
        /// <param name="h">High boundary of the interval</param>
        /// <returns>True if value being tested is within the interval, false otherwise.</returns>
        public static bool InRange(this long? x, long? l, long? h)
        {
            if (x < l) return false;
            if (x > h) return false;
            return true;
        }

        public static bool InRange(this double? x, double? l, double? h)
        {
            if (x < l) return false;
            if (x > h) return false;
            return true;
        }

        public static bool InRange(this decimal? x, decimal? l, decimal? h)
        {
            if (x < l) return false;
            if (x > h) return false;
            return true;
        }


        /// <summary>
        /// Checks if the first value is mentioned among the other values
        /// </summary>
        /// <param name="x">Candidate value</param>
        /// <param name="p">List of values</param>
        /// <returns>True if the first value is mentioned among the other values, false otherwise.</returns>
        public static bool InList(this long? x, params long?[] p)
        {
            if (p.Length == 0)
                return false;

            for (var i = 0; i < p.Length; i++)
                if (p[i] == x) return true;

            return false;
        }


        /// <summary>
        /// Checks if the multiple choice selection contains any of the specified values
        /// </summary>
        /// <param name="m">Multiple choice variable</param>
        /// <param name="p">One or more candidate values</param>
        /// <returns>True if any of the candidate values was found in the selection.</returns>
        public static bool ContainsAny(this decimal[] m, params decimal[] p)
        {
            if (m == null) return false;
            if (m.Length == 0) return false;

            if (p == null) return true;
            if (p.Length == 0) return true;

            for (var i = 0; i < p.Length; i++)
                for (var j = 0; j < m.Length; j++)
                    if (m[j] == p[i]) return true;

            return false;
        }

        /// <summary>
        /// Checks if the multiple choice selection contains only the specified value(s)
        /// </summary>
        /// <param name="m"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static bool ContainsOnly(this decimal[] m, params decimal[] x)
        {
            if (m == null) return false;
            if (m.Length != x.Length) return false;

            for (var i = 0; i < x.Length; i++)
                if (m.ContainsAny(x[i]) == false) return false;

            return true;
        }

        /// <summary>
        /// Checks that ALL of the listed items are selected in a multichoice question.
        /// </summary>
        /// <param name="multichoice">Multichoice question being inspected</param>
        /// <param name="candidates">List of items to be verified</param>
        /// <returns>True if all the candidate items are selected, false otherwise. </returns>
        /// Note that it is true if more than just specified items are selected.
        public static bool ContainsAll(this decimal[] multichoice, params decimal[] candidates)
        {
            if (multichoice == null) return false;
            if (multichoice.Length == 0) return false;

            if (candidates == null) return true;
            if (candidates.Length == 0) return false;

            foreach (var c in candidates)
                if (ContainsAny(multichoice, c) == false) return false;

            return true;
        }

        /// <summary>
        /// For a single choice item checks if the selection is any of the specified options
        /// </summary>
        /// <param name="singlechoice"></param>
        /// <param name="p">List of the options</param>
        /// <returns>True if the selection of the single choice question is among the specified options.</returns>
        /// For example educ.IsAnyOf(4,5,6)=True if educ==4 or educ==5 or educ==6 and is false otherwise
        public static bool IsAnyOf(this long? singlechoice, params long[] p)
        {
            if (singlechoice.HasValue == false) return false;

            for (var i = 0; i < p.Length; i++)
                if (p[i] == singlechoice.Value) return true;

            return false;
        }

        public static bool IsNoneOf(this decimal? singlechoice, params decimal[] candidates)
        {
            if (candidates == null) return true;
            if (candidates.Length == 0) return true;
            if (singlechoice.HasValue == false) return true;
            return !candidates.ContainsAny(singlechoice.Value);
        }

        public static long CountValues(this decimal[] multichoice, params decimal[] candidates)
        {
            if (multichoice == null) return 0;
            if (multichoice.Length == 0) return 0;

            if (candidates == null) return 0;
            if (candidates.Length == 0) return 0;

            var n = 0;
            foreach (var c in candidates)
                if (ContainsAny(multichoice, c)) n++;

            return n;
        }

        public static bool IsMilTime(this string s)
        {
            // 0600, 2315
            if (String.IsNullOrEmpty(s)) return false;
            if (s.Length != 4) return false;

            int h; if (int.TryParse(s.Substring(0, 2), out h) == false) return false;
            int m; if (int.TryParse(s.Substring(2, 2), out m) == false) return false;
            return InRange(h, 0, 23) && InRange(m, 0, 59);
        }

        public static bool IsMilTimeZ(this string s)
        {
            // 0600R, 2315Z
            const string milZones = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            if (string.IsNullOrEmpty(s)) return false;
            if (s.Length != 5) return false;

            if (IsMilTime(s.Substring(0, 4)) == false) return false;
            if (milZones.IndexOf(s.Substring(4, 1)) < 0) return false;
            return true;
        }
    }

    public class BaseFunctions
    {
        public long CmCode(long? mo, long? year)
        {
            const long baseYear = 1900; // not sure how other calendars will be handled?? e.g. Afghanistan, Thailand, etc.
            if (year == null) return -1;
            if (mo == null) return -1;

            if (mo.Value < 1 || mo.Value > 12) return -1;
            if (year.Value < baseYear) return -1;

            return (year.Value - baseYear) * 12 + mo.Value;
        }

        /// <summary>
        /// Counts the occurrancies of a certain value in a group of single choice questions
        /// </summary>
        /// <param name="x">Specific value to be searched for.</param>
        /// <param name="singleChoiceQuestions">One or more single choice questions.</param>
        /// <returns>Number of occurrancies of the specified value or zero if it is never encountered.</returns>
        public long CountValue(decimal x, params decimal?[] singleChoiceQuestions)
        {
            var c = 0;
            foreach (var variable in singleChoiceQuestions)
                if (variable.HasValue) if (variable.Value == x) c++;
            //hello
            return c;
        }
    }
}
