using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace WB.Core.GenericSubdomains.Utils
{
    public static class SubstitutionUtils
    {
        private const string SubstitutionVariableDelimiter = "%";
        private static readonly string AllowedSubstitutionVariableNameRegexp = String.Format(@"(?<={0})(\w+(?={0}))", SubstitutionVariableDelimiter);

        public static string[] GetAllSubstitutionVariableNames(string source)
        {
            if (String.IsNullOrWhiteSpace(source))
                return new string[0];

            var allOccurenses = Regex.Matches(source, (string)AllowedSubstitutionVariableNameRegexp).OfType<Match>().Select(m => m.Value).Distinct();
            return allOccurenses.ToArray();
        }

        public const string DefaultSubstitutionText = "[...]";

        public static string ReplaceSubstitutionVariable(this string text, string variable, string replaceTo)
        {
            return text.Replace(String.Format("{1}{0}{1}", variable, SubstitutionVariableDelimiter), replaceTo);
        }

        public const string RosterTitleSubstitutionReference = "rostertitle";
    }
}
