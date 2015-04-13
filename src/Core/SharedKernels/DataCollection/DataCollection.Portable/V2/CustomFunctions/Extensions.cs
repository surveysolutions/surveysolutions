using System;

namespace WB.Core.SharedKernels.DataCollection.CustomFunctions
{
    public static class Extensions
    {
        /// @name  Numeric functions
        /// @{ 

        #region Numeric functions

        /// <summary>
        /// Checks if the value is within a closed interval defined by its boundary points
        /// </summary>
        /// <param name="value">Value being tested</param>
        /// <param name="low">Low boundary of the interval</param>
        /// <param name="high">High boundary of the interval</param>
        /// <returns>True if value being tested is within the interval, false otherwise.</returns>
        public static bool InRange(this long? value, long? low, long? high)
        {
            if (value < low) return false;
            if (value > high) return false;
            return true;
        }

        /// <summary>
        /// Checks if the value is within a closed interval defined by its boundary points
        /// </summary>
        /// <param name="value">Value being tested</param>
        /// <param name="low">Low boundary of the interval</param>
        /// <param name="high">High boundary of the interval</param>
        /// <returns>True if value being tested is within the interval, false otherwise.</returns>
        public static bool InRange(this double? value, double? low, double? high)
        {
            if (value < low) return false;
            if (value > high) return false;
            return true;
        }

        /// <summary>
        /// Checks if the value is within a closed interval defined by its boundary points
        /// </summary>
        /// <param name="value">Value being tested</param>
        /// <param name="low">Low boundary of the interval</param>
        /// <param name="high">High boundary of the interval</param>
        /// <returns>True if value being tested is within the interval, false otherwise.</returns>
        public static bool InRange(this decimal? value, decimal? low, decimal? high)
        {
            if (value < low) return false;
            if (value > high) return false;
            return true;
        }

        /// <summary>
        /// Checks if the value of the variable is mentioned among the specified values
        /// </summary>
        /// <param name="value">Value to be searched for</param>
        /// <param name="valuesList">List of values for search</param>
        /// <returns>True if the value is mentioned among the values in the list, false otherwise.</returns>
        public static bool InList(this long? value, params long?[] valuesList)
        {
            if (valuesList.Length == 0)
                return false;

            for (var i = 0; i < valuesList.Length; i++)
                if (valuesList[i] == value) return true;

            return false;
        }

        /// <summary>
        /// Checks if the value of the variable is mentioned among the specified values
        /// </summary>
        /// <param name="value">Value to be searched for</param>
        /// <param name="valuesList">List of values for search</param>
        /// <returns>True if the value is mentioned among the values in the list, false otherwise.</returns>
        public static bool InList(this double? value, params double?[] valuesList)
        {
            if (valuesList.Length == 0)
                return false;

            for (var i = 0; i < valuesList.Length; i++)
                if (valuesList[i] == value) return true;

            return false;
        }

        /// <summary>
        /// Checks if the value of the variable is mentioned among the specified values
        /// </summary>
        /// <param name="value">Value to be searched for</param>
        /// <param name="valuesList">List of values for search</param>
        /// <returns>True if the value is mentioned among the values in the list, false otherwise.</returns>
        public static bool InList(this decimal? value, params decimal?[] valuesList)
        {
            if (valuesList.Length == 0)
                return false;

            for (var i = 0; i < valuesList.Length; i++)
                if (valuesList[i] == value) return true;

            return false;
        }

        #endregion

        /// @}

        /// @name  List functions
        /// @{

        #region List functions

        /// <summary>
        /// Checks that ALL of the listed items are selected in a multichoice question.
        /// </summary>
        /// <param name="multichoice">Multichoice question being inspected</param>
        /// <param name="valuesList">List of items to be verified</param>
        /// <returns>True if all the candidate items are selected, false otherwise. </returns>
        /// Note that it is true if more than just specified items are selected.
        public static bool ContainsAll(this decimal[] multichoice, params decimal[] valuesList)
        {
            if (multichoice == null) return false;
            if (multichoice.Length == 0) return false;

            if (valuesList == null) return true;
            if (valuesList.Length == 0) return true;

            foreach (var c in valuesList)
                if (ContainsAny(multichoice, c) == false) return false;

            return true;
        }

        /// <summary>
        /// Checks if the multiple choice selection contains any of the specified values
        /// </summary>
        /// <param name="multichoice">Multiple choice variable</param>
        /// <param name="valuesList">One or more candidate values</param>
        /// <returns>True if any of the candidate values was found in the selection.</returns>
        public static bool ContainsAny(this decimal[] multichoice, params decimal[] valuesList)
        {
            if (multichoice == null) return false;
            if (multichoice.Length == 0) return false;

            if (valuesList == null) return true;
            if (valuesList.Length == 0) return true;

            for (var i = 0; i < valuesList.Length; i++)
                for (var j = 0; j < multichoice.Length; j++)
                    if (multichoice[j] == valuesList[i]) return true;

            return false;
        }

        /// <summary>
        /// Checks if the multiple choice selection contains only the specified value(s)
        /// </summary>
        /// <param name="multichoice">Multiple choice variable</param>
        /// <param name="valuesList">One or more values</param>
        /// <returns></returns>
        public static bool ContainsOnly(this decimal[] multichoice, params decimal[] valuesList)
        {
            if (multichoice == null) return false;
            if (multichoice.Length != valuesList.Length) return false;

            for (var i = 0; i < valuesList.Length; i++)
                if (multichoice.ContainsAny(valuesList[i]) == false) return false;

            return true;
        }

        /// <summary>
        /// For a single choice question checks that the selection is mentioned in the specified list of values.
        /// </summary>
        /// <param name="singlechoice">Single choice variable</param>
        /// <param name="valuesList">List of the options</param>
        /// <returns>True if the selection of the single choice question is among the specified options.</returns>
        /// 
        /// For example educ.IsAnyOf(4,5,6)=True if educ==4 or educ==5 or educ==6 and is false otherwise
        public static bool IsAnyOf(this decimal? singlechoice, params decimal[] valuesList)
        {
            if (singlechoice.HasValue == false) return false;

            for (var i = 0; i < valuesList.Length; i++)
                if (valuesList[i] == singlechoice.Value) return true;

            return false;
        }

        /// <summary>
        /// For a single choice question checks that the selection is not mentioned in the specified list of values.
        /// </summary>
        /// <param name="singlechoice">single choice variable.</param>
        /// <param name="valuesList">list of values (blacklist).</param>
        /// <returns>True if the selection is not mentioned in the specified list of values.</returns>
        public static bool IsNoneOf(this decimal? singlechoice, params decimal[] valuesList)
        {
            if (valuesList == null) return true;
            if (valuesList.Length == 0) return true;
            if (singlechoice.HasValue == false) return true;
            return !valuesList.ContainsAny(singlechoice.Value);
        }

        /// <summary>
        /// Counts how many of the specified values are mentioned in the multichoice selection.
        /// </summary>
        /// <param name="multichoice">Multichoice variable</param>
        /// <param name="valuesList">List of one or more numeric values</param>
        /// <returns>Count of how many of the specified values are present in the selection.</returns>
        /// 
        /// Note that zero is returned if the selection is empty or the list of specified values is empty.
        public static long CountValues(this decimal[] multichoice, params decimal[] valuesList)
        {
            if (multichoice == null) return 0;
            if (multichoice.Length == 0) return 0;

            if (valuesList == null) return 0;
            if (valuesList.Length == 0) return 0;

            var n = 0;
            foreach (var c in valuesList)
                if (ContainsAny(multichoice, c)) n++;

            return n;
        }

        #endregion

        /// @}


        /// @name  String functions
        /// @{

        #region String functions

        /// <summary>
        /// Extracts the specified number of characters from the beginning of the string.
        /// </summary>
        /// <param name="s">The string to return characters from.</param>
        /// <param name="size">Specifies how many characters to return. 
        /// If 0 empty string is returned. 
        /// If larger than the length of the string, the whole string is returned.</param>
        /// <returns>specified number of characters from the beginning of the string.</returns>
        public static string Left(this string s, long? size)
        {
            if (size.HasValue == false) return null;
            if (size.Value < 0) return String.Empty;
            if (size.Value >= s.Length) return s;
            return s.Substring(0, (int)size.Value);
        }

        /// <summary>
        /// Extracts the specified number of characters from the beginning of the string.
        /// </summary>
        /// <param name="s">The string to return characters from.</param>
        /// <param name="size">Specifies how many characters to return. 
        /// If 0 empty string is returned. 
        /// If larger than the length of the string, the whole string is returned.</param>
        /// <returns>specified number of characters from the beginning of the string.</returns>
        public static string Left(this string s, decimal? size)
        {
            if (!size.HasValue) return null;
            return s.Left((long)Math.Floor((double)size.Value));
        }

        /// <summary>
        /// Extracts the specified number of characters from the beginning of the string.
        /// </summary>
        /// <param name="s">The string to return characters from.</param>
        /// <param name="size">Specifies how many characters to return. 
        /// If 0 empty string is returned. 
        /// If larger than the length of the string, the whole string is returned.</param>
        /// <returns>specified number of characters from the beginning of the string.</returns>
        public static string Left(this string s, double? size)
        {
            if (!size.HasValue) return null;
            return s.Left((long)Math.Floor(size.Value));
        }

        /// <summary>
        /// Extracts the specified number of characters from the end of the string.
        /// </summary>
        /// <param name="s">The string to return characters from.</param>
        /// <param name="size">Specifies how many characters to return. 
        /// If 0 empty string is returned.
        /// If larger than the length of the string, the whole string is returned.</param>
        /// <returns>specified number of characters from the end of the string.</returns>
        public static string Right(this string s, long? size)
        {
            if (size.HasValue == false) return null;
            if (size.Value < 0) return String.Empty;
            if (size.Value > s.Length) return s;
            return s.Substring(s.Length - (int)size.Value);
        }

        /// <summary>
        /// Extracts the specified number of characters from the end of the string.
        /// </summary>
        /// <param name="s">The string to return characters from.</param>
        /// <param name="size">Specifies how many characters to return. 
        /// If 0 empty string is returned.
        /// If larger than the length of the string, the whole string is returned.</param>
        /// <returns>specified number of characters from the end of the string.</returns>
        public static string Right(this string s, decimal? size)
        {
            if (!size.HasValue) return null;
            return s.Right((long)Math.Floor((double)size.Value));
        }

        /// <summary>
        /// Extracts the specified number of characters from the end of the string.
        /// </summary>
        /// <param name="s">The string to return characters from.</param>
        /// <param name="size">Specifies how many characters to return. 
        /// If 0 empty string is returned.
        /// If larger than the length of the string, the whole string is returned.</param>
        /// <returns>specified number of characters from the end of the string.</returns>
        public static string Right(this string s, double? size)
        {
            if (!size.HasValue) return null;
            return s.Right((long)Math.Floor(size.Value));
        }

        /// <summary>
        /// Confirms the string consists entirely of latin letters A..Z and a..z
        /// </summary>
        /// <param name="s">string to be checked</param>
        /// <returns>True if the string consists entirely of latin letters, false otherwise.</returns>
        public static bool IsAlphaLatin(this string s)
        {
            const string latinChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            return s.ConsistsOf(latinChars);
        }

        /// <summary>
        /// Confirms the string consists entirely of latin letters A..Z, a..z 
        /// or characters '.' (dot), ',' (comma), ';' (semicolon), " " (whitespace).
        /// </summary>
        /// <param name="s">string to be checked</param>
        /// <returns>True if the string consists entirely of permitted characters, false otherwise.</returns>
        public static bool IsAlphaLatinOrDelimiter(this string s)
        {
            const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz ,;.";
            return s.ConsistsOf(validChars);
        }

        /// <summary>
        /// Verifies the string consists of only listed characters.
        /// </summary>
        /// <param name="s">String to be checked</param>
        /// <param name="listChars">List of permitted characters.</param>
        /// <returns>True if the string is empty or consists of only permitted characters. False otherwise.</returns>
        public static bool ConsistsOf(this string s, string listChars)
        {
            if (s == null) return true;
            if (s == "") return true;

            foreach (char c in s)
                if (listChars.IndexOf(c) < 0) return false;

            return true;
        }

        /// <summary>
        /// Checks if the string satisfies a mask
        /// </summary>
        /// <param name="s">String to be checked</param>
        /// <param name="mask">Mask to be applied</param>
        /// <returns>True if the string satisfies the mask or both strings are empty. False otherwise.</returns>
        /// 
        /// The mask is expected to be formulated using the ? and * characters.
        /// Use the ? (question mark) character to denote any one character, which must be present.
        /// Use the * (star) character to denote any number of characters, which may or may not be present.
        public static bool IsLike(this string s, string mask)
        {
            if (String.IsNullOrEmpty(s) && String.IsNullOrEmpty(mask))
                return true;
            if (String.IsNullOrEmpty(s) && !String.IsNullOrEmpty(mask))
                return false;

            string msk;
            var q = mask.IndexOf('*');
            if (mask.IndexOf('*', q + 1) >= 0)
                throw new ArgumentException("No more than one * is allowed in the mask.");

            if (q < 0)
            {
                if (mask.Length != s.Length) return false;
                msk = mask;
            }
            else
            {
                var n = s.Length - (mask.Length - 1);
                if (n < 0) return false;

                msk = mask.Substring(0, q);
                for (var i = 0; i < n; i++)
                    msk = msk + "?";

                msk = msk + mask.Substring(q + 1);
            }

            for (var i = 0; i < s.Length; i++)
                if (msk[i] != '?')
                    if (msk[i] != s[i]) return false;

            return true;
        }

        #endregion

        /// @}

        /// @name  Location functions
        /// @{
        #region Location functions

        /// <summary>
        /// Converts an angle to a radian.
        /// </summary>
        /// <param name="angle">The angle that is to be converted.</param>
        /// <returns>The angle in radians.</returns>
        private static double ToRad(double angle)
        {
            return angle * (Math.PI / 180);
        }

        /// <summary>
        /// Computes the distance between two points using the Haversine formula.
        /// </summary>
        /// <param name="point1">First point</param>
        /// <param name="point2">Second point</param>
        /// <returns>Distance in km between the two points approximated using the haversine formula.</returns>
        public static double GpsDistance(this GeoLocation point1, GeoLocation point2)
        {
            // Source: http://rosettacode.org/wiki/Haversine_formula#C.23
            const double r = 6372800; // In meters
            var dLat = ToRad(point2.Latitude - point1.Latitude);
            var dLon = ToRad(point2.Longitude - point1.Longitude);
            var lat1 = ToRad(point1.Latitude);
            var lat2 = ToRad(point2.Latitude);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);

            return r * 2 * Math.Asin(Math.Sqrt(a));
        }

        #endregion
    }
}
