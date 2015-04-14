using System;
using System.Text;

namespace WB.Core.SharedKernels.DataCollection.V2.CustomFunctions
{
    public class BaseFunctions
    {
        /// @name  Date and time management functions
        /// @{

        #region Date and time management functions

        /// <summary>
        /// Computes the century month code (CMC) given a month and a year
        /// </summary>
        /// <param name="month">Month number (1..12)</param>
        /// <param name="year">Year</param>
        /// <returns>Century month code value</returns>
        public long CmCode(long? month, long? year)
        {
            const long baseYear = 1900;
            // not sure how other calendars will be handled?? e.g. Afghanistan, Thailand, etc.
            if (!year.HasValue) return -1;
            if (!month.HasValue) return -1;
            if (month.Value < 1 || month.Value > 12) return -1;
            if (year.Value < baseYear) return -1;
            return (year.Value - baseYear)*12 + month.Value;
        }

        public long CmCode(double? month, double? year)
        {
            if (!year.HasValue) return -1;
            if (!month.HasValue) return -1;
            long? m = (long) Math.Floor((double) month);
            long? y = (long) Math.Floor((double) year);
            if (m != month) return -1;
            if (y != year) return -1;
            return CmCode(m, y);
        }

        public long CmCode(decimal? month, decimal? year)
        {
            if (!year.HasValue) return -1;
            if (!month.HasValue) return -1;
            long? m = (long) Math.Floor((double) month);
            long? y = (long) Math.Floor((double) year);
            return CmCode(m, y);
        }

        /// <summary>
        /// Verifies that a certain combination of year, month and day is a valid date.
        /// </summary>
        /// <param name="year">Year</param>
        /// <param name="month">Month</param>
        /// <param name="day">Day</param>
        /// <returns>True if the three parameters define a valid date. False otherwise.</returns>
        public bool IsDate(decimal? year, decimal? month, decimal? day)
        {
            return IsDate(year.HasValue ? (double) year : (double?) null,
                month.HasValue ? (double) month : (double?) null, day.HasValue ? (double) day : (double?) null);
        }

        /// <summary>
        /// Verifies that a certain combination of year, month and day is a valid date.
        /// </summary>
        /// <param name="year">Year</param>
        /// <param name="month">Month</param>
        /// <param name="day">Day</param>
        /// <returns>True if the three parameters define a valid date. False otherwise.</returns>
        public bool IsDate(double? year, double? month, double? day)
        {
            if (!year.HasValue) return false;
            if (!month.HasValue) return false;
            if (!day.HasValue) return false;

            if (year != Math.Floor(year.Value)) return false;
            if (month != Math.Floor(month.Value)) return false;
            if (day != Math.Floor(day.Value)) return false;

            try
            {
                new DateTime((int) year.Value, (int) month.Value, (int) day.Value);
            }
            catch
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Verifies that specified string contains time in 'military' format without time zone.
        /// </summary>
        /// <param name="stringVar">String variable</param>
        /// <returns>True if the specified variable contains valid time in 'military format' without time zone, false otherwise.</returns>
        public bool IsMilitaryTime(string stringVar)
        {
            // 0600, 2315
            if (String.IsNullOrEmpty(stringVar)) return false;
            if (stringVar.Length != 4) return false;

            int h;
            if (Int32.TryParse(stringVar.Substring(0, 2), out h) == false) return false;
            int m;
            if (Int32.TryParse(stringVar.Substring(2, 2), out m) == false) return false;

            return (0 <= h) && (h <= 23) && (0 <= m) && (m <= 59);
        }

        /// <summary>
        /// Verifies that specified string contains time in 'military' format with time zone.
        /// </summary>
        /// <param name="stringVar">String variable.</param>
        /// <returns>True if the specified variable contains valid time in 'military format' without time zone, false otherwise.</returns>
        public bool IsMilitaryTimeZ(string stringVar)
        {
            // 0600R, 2315Z
            const string milZones = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            if (String.IsNullOrEmpty(stringVar)) return false;
            if (stringVar.Length != 5) return false;

            if (IsMilitaryTime(stringVar.Substring(0, 4)) == false) return false;
            if (milZones.IndexOf(stringVar.Substring(4, 1)) < 0) return false;
            return true;
        }

        /// <summary>
        /// Number of full years between dates
        /// </summary>
        /// <param name="date1">Earlier date</param>
        /// <param name="date2">Later date</param>
        /// <returns>Number of complete years between the two dates.</returns>
        public int FullYearsBetween(DateTime? date1, DateTime? date2)
        {
            if (date1.HasValue == false) return -9999;
            if (date2.HasValue == false) return -9999;

            if (date1 > date2) return -9998;

            var d1Bis = new DateTime(date2.Value.Year, date1.Value.Month, date1.Value.Day);
            var yearsDif = date2.Value.Year - date1.Value.Year;
            var yearsAdj = (d1Bis - date2.Value).TotalMilliseconds > 0 ? 1 : 0;
            return yearsDif - yearsAdj;
        }

        #endregion

        /// @}



        /// @name  Sampling functions
        /// @{

        #region Sampling functions

        /// <summary>
        /// Returns roster item selection based on the Kish grid
        /// </summary>
        /// <param name="tableNumber">Number of the table</param>
        /// <param name="size">Size of the roster</param>
        /// <returns>Index in the roster</returns>
        /// 
        /// The following table numbers are assumed here: \n
        /// 1 for table A,   (p=1/6)  \n
        /// 2 for table B1,  (p=1/12) \n
        /// 3 for table B2,  (p=1/12) \n
        /// 4 for table C,   (p=1/6)  \n
        /// 5 for table D,   (p=1/6)  \n
        /// 6 for table E1,  (p=1/12) \n
        /// 7 for table E2,  (p=1/12) \n
        /// 8 for table F,   (p=1/6)  \n
        /// 
        ///
        /// The function returns special value -9999 if the size is null, 
        /// special value -9998 if questionnaire number is less than 1, and
        /// special value -9997 if size is less than 1.
        /// 
        public long SelectKish1949(long tableNumber, long? size)
        {
            // Based on Table 3 in "Respondent selection within the household - A modification of the Kish grid", R.Nemeth
            // https://www.stat.aau.at/Tagungen/Ossiach/Nemeth.pdf

            if (size.HasValue == false) return -9999;
            if (tableNumber < 1 || tableNumber > 8) return -9998;
            if (size.Value < 1) return -9997;

            var h = size.Value;
            if (h > 6) h = 6;

            var kishTable = new long[,]
            {
                {1, 1, 1, 1, 1, 1},
                {1, 1, 1, 1, 2, 2},
                {1, 1, 1, 2, 2, 2},
                {1, 1, 2, 2, 3, 3},
                {1, 2, 2, 3, 4, 4},
                {1, 2, 3, 3, 3, 5},
                {1, 2, 3, 4, 5, 5},
                {1, 2, 3, 4, 5, 6}
            };

            return kishTable[(int) tableNumber - 1, h - 1]; // matrix indexed from 0
        }

        /// <summary>
        /// Returns roster item selection based on Kish table modified by ILO.
        /// </summary>
        /// <param name="questNumber">Questionnaire number</param>
        /// <param name="size">Size of the roster</param>
        /// <returns>Index in the roster</returns>
        /// 
        /// The function returns special value -9999 if the size is null, 
        /// special value -9998 if questionnaire number is less than 1, and
        /// special value -9997 if size is less than 1.
        /// 
        /// Based on Table 3.1 in "Module 3: Sampling Methodology", S.Elder
        /// http://www.ilo.org/wcmsp5/groups/public/@ed_emp/documents/instructionalmaterial/wcms_140859.pdf
        public long SelectKishIlo(long questNumber, long? size)
        {
            if (size.HasValue == false) return -9999;
            if (questNumber < 1) return -9998;
            if (size.Value < 1) return -9997;

            var h = size.Value;
            if (h > 8) h = 8;

            var rs = questNumber.ToString(); // Android does not support InvariantCulture
            var i = rs.Length - 1;
            while (rs.Substring(i, 1) == "0")
                i = i - 1;

            var r = Int32.Parse(rs.Substring(i, 1));

            var kishTable = new long[,]
            {
                {1, 1, 1, 1, 1, 1, 1, 1},
                {1, 2, 1, 2, 1, 2, 1, 2},
                {1, 2, 3, 1, 2, 3, 1, 2},
                {1, 2, 3, 4, 1, 2, 3, 4},
                {1, 2, 3, 4, 5, 3, 4, 5},
                {1, 2, 3, 4, 5, 6, 3, 6},
                {1, 2, 3, 4, 5, 6, 7, 4},
                {1, 2, 3, 4, 5, 6, 7, 8},
                {1, 2, 3, 4, 5, 6, 7, 8}
            };

            return kishTable[r - 1, h - 1]; // matrix indexed from 0
        }

        #endregion

        /// @}




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

            return c;
        }

        /// <summary>
        /// Concatenates multiple strings into one.
        /// </summary>
        /// <param name="strings">one or more strings</param>
        /// <returns>Concatenated string</returns>
        public string Concat(params string[] strings)
        {
            if (strings == null) return String.Empty;
            var sb = new StringBuilder();
            foreach (var s in strings)
            {
                sb.Append(s);
            }
            return sb.ToString();
        }
    }
}
