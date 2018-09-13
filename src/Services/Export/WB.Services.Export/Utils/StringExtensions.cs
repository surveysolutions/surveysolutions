using System;
using System.IO;
using System.Text.RegularExpressions;
using Ganss.XSS;

namespace WB.Services.Export.Utils
{
    public static class StringExtensions
    {
        private static readonly Regex RemoveNewLineRegEx = new Regex(@"\t|\n|\r", RegexOptions.Compiled);
        public static string RemoveNewLine(this string value)
        {
            return string.IsNullOrEmpty(value) ? string.Empty : RemoveNewLineRegEx.Replace(value, " ");
        }

        public static string MakeStataCompatibleFileName(this string name)
        {
            var result = RemoveNonAscii(MakeValidFileName(name)).Trim();
            var clippedResult = result.Length < 128 ? result : result.Substring(0, 128);
            return string.IsNullOrWhiteSpace(clippedResult) ? "_" : clippedResult;
        }

        public static string RemoveNonAscii(this string s) => Regex.Replace(s, @"[^\u0000-\u007F]", string.Empty);

        public static string MakeValidFileName(this string name)
        {
            if (string.IsNullOrEmpty(name))
                return string.Empty;

            var invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            var invalidReStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);
            var fileNameWithReplaceInvalidChars = Regex.Replace(name, invalidReStr, "_");
            return fileNameWithReplaceInvalidChars.Substring(0, Math.Min(fileNameWithReplaceInvalidChars.Length, 128));
        }

        private static readonly HtmlSanitizer Sanitizer;

        static StringExtensions()
        {
            Sanitizer = new HtmlSanitizer { KeepChildNodes = true };
            Sanitizer.AllowedAttributes.Clear();
            Sanitizer.AllowedTags.Clear();
        }

        public static string RemoveHtmlTags(this string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return value;
            return Sanitizer.Sanitize(value);
        }
    }
}
