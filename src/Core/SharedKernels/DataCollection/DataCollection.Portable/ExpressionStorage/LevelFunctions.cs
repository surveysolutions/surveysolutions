﻿using System;
using System.Text;
using WB.Core.SharedKernels.DataCollection.Portable;

namespace WB.Core.SharedKernels.DataCollection.ExpressionStorage
{
    public class LevelFunctions
    {
        public bool IsAnswered(string answer) => !string.IsNullOrWhiteSpace(answer);

        public bool IsAnswered<TY>(TY? answer) where TY : struct => answer.HasValue; // doublle?, int?, DateTime?

        public bool IsAnswered(GeoLocation answer) => !(answer == null || (answer.Altitude == 0 && answer.Longitude == 0 && answer.Accuracy == 0 && answer.Latitude == 0));

        public bool IsAnswered(int[] answer) => !(answer == null || answer.Length == 0);

        public bool IsAnswered(RosterVector[] answer) => !(answer == null || answer.Length == 0);

        public bool IsAnswered(TextListAnswerRow[] answer) => !(answer == null || answer.Length == 0);

        public bool IsAnswered(RosterVector answer) => answer != null;

        public bool IsAnswered(YesNoAndAnswersMissings answer) => !(answer.Yes.Length == 0 && answer.No.Length == 0);

        public bool IsAnswered(AudioAnswerForConditions answer) => !string.IsNullOrEmpty(answer?.FileName) && answer.Length.Ticks > 0;

        /// @name  Date and time management functions
        /// @{

        #region Date and time management functions

        /// <summary>
        /// Computes the century month code (CMC) given a month and a year
        /// </summary>
        /// <param name="month">Month number (1..12)</param>
        /// <param name="year">Year</param>
        /// <returns>Century month code value</returns>
        /// 
        /// See details and an example here: http://demographicestimation.iussp.org/content/dhs-century-month-codes
        /// 
        /// An error code -1 is returned in case of invalid values 
        /// of inputs, such as undefined or negative values of year 
        /// or month.
        public long CenturyMonthCode(long? month, long? year)
        {
            const long baseYear = 1900;
            // not sure how other calendars will be handled?? e.g. Afghanistan, Thailand, etc.
            if (!year.HasValue) return -1;
            if (!month.HasValue) return -1;
            if (month.Value < 1 || month.Value > 12) return -1;
            if (year.Value < baseYear) return -1;
            return (year.Value - baseYear) * 12 + month.Value;
        }

        public long CenturyMonthCode(double? month, double? year)
        {
            if (!year.HasValue) return -1;
            if (!month.HasValue) return -1;
            long? m = (long)Math.Floor((double)month);
            long? y = (long)Math.Floor((double)year);
            if (m != month) return -1;
            if (y != year) return -1;
            return CenturyMonthCode(m, y);
        }

        public long CenturyMonthCode(decimal? month, decimal? year)
        {
            if (!year.HasValue) return -1;
            if (!month.HasValue) return -1;
            long? m = (long)Math.Floor((double)month);
            long? y = (long)Math.Floor((double)year);
            return CenturyMonthCode(m, y);
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
            return IsDate(year.HasValue ? (double)year : (double?)null,
                month.HasValue ? (double)month : (double?)null, day.HasValue ? (double)day : (double?)null);
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
                new DateTime((int)year.Value, (int)month.Value, (int)day.Value);
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
        /// 
        /// See also: IsMilitaryTime()
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
        /// Number of full years between a date and a later date
        /// </summary>
        /// <param name="date1">Earlier date</param>
        /// <param name="date2">Later date</param>
        /// <returns>Number of complete years.</returns>
        /// 
        /// The second date is required to be later than or same as the first one.
        /// The function returns special value -9998 is returned if the second date is 
        /// prior to the first one; and special value -9999 is returned if any of the 
        /// two dates are missing.
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

            return kishTable[(int)tableNumber - 1, h - 1]; // matrix indexed from 0
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

        /// <summary>
        /// Computes the body mass index (BMI) from person's weight and height.
        /// </summary>
        /// <param name="weight">Person's weight in kg.</param>
        /// <param name="height">Person's height in m.</param>
        /// <returns>computed value of BMI, or null if any argument is null.</returns>
        /// 
        /// For details see: 
        /// http://www.cdc.gov/healthyweight/assessing/bmi/adult_bmi/
        public double? Bmi(double? weight, double? height)
        {
            return weight / (height * height);
        }

        #region March 2016 functions

        /// <summary>
        /// Verifies that a certain combination of year, month and day is a valid date.
        /// </summary>
        /// <param name="year">Year</param>
        /// <param name="month">Month</param>
        /// <param name="day">Day</param>
        /// <returns>True if the three parameters define a valid date. False otherwise.</returns>
        public bool IsDate(long? year, long? month, long? day)
        {
            if (!year.HasValue) return false;
            if (!month.HasValue) return false;
            if (!day.HasValue) return false;

            if (year < 0) return false;
            if (month < 0) return false;
            if (day < 0) return false;

            try
            {
                var dateTime = new DateTime((int)year.Value, (int)month.Value, (int)day.Value);
            }
            catch
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Verifies that a certain combination of year, month and day is a valid date.
        /// </summary>
        /// <param name="year">Year</param>
        /// <param name="month">Month</param>
        /// <param name="day">Day</param>
        /// <returns>True if the three parameters define a valid date. False otherwise.</returns>
        public bool IsDate(int? year, int? month, int? day)
        {
            if (!year.HasValue) return false;
            if (!month.HasValue) return false;
            if (!day.HasValue) return false;

            if (year < 0) return false;
            if (month < 0 || month > 12) return false;
            if (day < 0 || day > 31) return false;

            try
            {
                var dateTime = new DateTime(year.Value, month.Value, day.Value);
            }
            catch
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Number of calendar days between a date and a later date
        /// </summary>
        /// <param name="date1">Earlier date</param>
        /// <param name="date2">Later date</param>
        /// <returns>Number of complete years.</returns>
        /// 
        /// The second date is required to be later than or same as the first one.
        /// The function returns special value -9998 is returned if the second date is 
        /// prior to the first one; and special value -9999 is returned if any of the 
        /// two dates are missing.
        public int DaysBetweenDates(DateTime? date1, DateTime? date2)
        {
            if (date1.HasValue == false) return -9999;
            if (date2.HasValue == false) return -9999;

            if (date1 > date2) return -9998;
            return (int)Math.Floor((date2.Value - date1.Value).TotalDays);
        }

        /// <summary>
        /// Number of days in a particular month
        /// </summary>
        /// <param name="date1">A date</param>
        /// <returns>Number of days in the month, which contains 
        /// the given date or a special value -9999 if the date 
        /// argument is missing.</returns>
        public int DaysInMonth(DateTime? date1)
        {
            if (date1.HasValue == false) return -9999;

            return DateTime.DaysInMonth(date1.Value.Year, date1.Value.Month);
        }

        /// <summary>
        /// Number of days in a particular month
        /// </summary>
        /// <param name="cmc">Century month code for the month</param>
        /// <returns>Number of days in the specified month; a special 
        /// value -9999 is returned for an invalid negative cmc 
        /// argument.</returns>
        public int DaysInMonth(int cmc)
        {
            if (cmc < 0) return -9999;
            var year = YearOfCmc(cmc);
            var month = MonthOfCmc(cmc);
            return DateTime.DaysInMonth(year, month);
        }

        /// <summary>
        /// Number of days in a particular month
        /// </summary>
        /// <param name="cmc">Century month code for the month</param>
        /// <returns>Number of days in the specified month; a special 
        /// value -9999 is returned for an invalid negative cmc 
        /// argument.</returns>
        public int DaysInMonth(long cmc)
        {
            return DaysInMonth((int)cmc);
        }

        /// <summary>
        /// Determines the calendar year for a given century month code
        /// </summary>
        /// <param name="cmc">Century month code</param>
        /// <returns>Calendar year or a special value -9999 for invalid 
        /// negative century month codes.</returns>
        public int YearOfCmc(int cmc)
        {
            if (cmc < 0) return -9999;
            const int baseYear = 1900;
            var year = (int)Math.Floor((cmc - 1) / 12.0) + baseYear;
            return year;
        }

        /// <summary>
        /// Determines the calendar month corresponding to a given century month code
        /// </summary>
        /// <param name="cmc">Century month code</param>
        /// <returns>Calendar month number 1..12 or a special value -9999 for 
        /// invalid negative century month codes.</returns>
        public int MonthOfCmc(int cmc)
        {
            if (cmc < 0) return -9999;
            const int baseYear = 1900;
            var month = cmc - (YearOfCmc(cmc) - baseYear) * 12;
            return month;
        }

        /// <summary>
        /// Determines the calendar year for a given century month code
        /// </summary>
        /// <param name="cmc">Century month code</param>
        /// <returns>Calendar year or a special value -9999 for invalid 
        /// negative century month codes.</returns>
        public int YearOfCmc(long cmc)
        {
            return YearOfCmc((int)cmc);
        }

        /// <summary>
        /// Determines the calendar month corresponding to a given century month code
        /// </summary>
        /// <param name="cmc">Century month code</param>
        /// <returns>Calendar month number 1..12 or a special value -9999 for 
        /// invalid negative century month codes.</returns>
        public int MonthOfCmc(long cmc)
        {
            return MonthOfCmc((int)cmc);
        }

        /// <summary>
        /// Determines bracket index where a value belongs.
        /// </summary>
        /// <param name="x">value</param>
        /// <param name="cuts">one or more cut-off points</param>
        /// <returns>A zero-based index of an interval </returns>
        /// 
        /// Breaking the ties: exact values fall to the left intervals.
        public int BracketIndexLeft(decimal x, params decimal?[] cuts)
        {
            var c = 0;
            while (c < cuts.Length && x > cuts[c]) c++;
            return c;
        }

        /// <summary>
        /// Determines bracket index where a value belongs.
        /// </summary>
        /// <param name="x">value</param>
        /// <param name="cuts">one or more cut-off points</param>
        /// <returns>A zero-based index of an interval </returns>
        /// 
        /// Breaking the ties: exact values fall to the left intervals.
        public int BracketIndexLeft(double x, params double?[] cuts)
        {
            var c = 0;
            while (c < cuts.Length && x > cuts[c]) c++;
            return c;
        }

        /// <summary>
        /// Determines bracket index where a value belongs.
        /// </summary>
        /// <param name="x">value</param>
        /// <param name="cuts">one or more cut-off points</param>
        /// <returns>A zero-based index of an interval </returns>
        /// 
        /// Breaking the ties: exact values fall to the left intervals.
        public int BracketIndexLeft(long x, params long?[] cuts)
        {
            var c = 0;
            while (c < cuts.Length && x > cuts[c]) c++;
            return c;
        }

        /// <summary>
        /// Determines bracket index where a value belongs.
        /// </summary>
        /// <param name="x">value</param>
        /// <param name="cuts">one or more cut-off points</param>
        /// <returns>A zero-based index of an interval </returns>
        /// 
        /// Breaking the ties: exact values fall to the left intervals.
        public int BracketIndexLeft(int x, params int?[] cuts)
        {
            var c = 0;
            while (c < cuts.Length && x > cuts[c]) c++;
            return c;
        }


        /// <summary>
        /// Determines bracket index where a value belongs.
        /// </summary>
        /// <param name="x">value</param>
        /// <param name="cuts">one or more cut-off points</param>
        /// <returns>A zero-based index of an interval </returns>
        /// 
        /// Breaking the ties: exact values fall to the right intervals.
        public int BracketIndexRight(decimal x, params decimal?[] cuts)
        {
            var c = 0;
            while (c < cuts.Length && x >= cuts[c]) c++;
            return c;
        }


        /// <summary>
        /// Determines bracket index where a value belongs.
        /// </summary>
        /// <param name="x">value</param>
        /// <param name="cuts">one or more cut-off points</param>
        /// <returns>A zero-based index of an interval </returns>
        /// 
        /// Breaking the ties: exact values fall to the right intervals.
        public int BracketIndexRight(double x, params double?[] cuts)
        {
            var c = 0;
            while (c < cuts.Length && x >= cuts[c]) c++;
            return c;
        }

        /// <summary>
        /// Determines bracket index where a value belongs.
        /// </summary>
        /// <param name="x">value</param>
        /// <param name="cuts">one or more cut-off points</param>
        /// <returns>A zero-based index of an interval </returns>
        /// 
        /// Breaking the ties: exact values fall to the right intervals.
        public int BracketIndexRight(long x, params long?[] cuts)
        {
            var c = 0;
            while (c < cuts.Length && x >= cuts[c]) c++;
            return c;
        }

        /// <summary>
        /// Determines bracket index where a value belongs.
        /// </summary>
        /// <param name="x">value</param>
        /// <param name="cuts">one or more cut-off points</param>
        /// <returns>A zero-based index of an interval </returns>
        /// 
        /// Breaking the ties: exact values fall to the right intervals.
        public int BracketIndexRight(int x, params int?[] cuts)
        {
            var c = 0;
            while (c < cuts.Length && x >= cuts[c]) c++;
            return c;
        }

        #endregion

    }
}
