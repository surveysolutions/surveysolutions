using System;
using System.Linq;
using System.Text.RegularExpressions;
using WB.Core.SharedKernels.SurveySolutions.Services;

namespace WB.Core.SharedKernels.SurveySolutions.Implementation.Services
{
    public class SubstitutionService : ISubstitutionService
    {
        private const string SubstitutionVariableDelimiter = "%";
        private readonly string AllowedSubstitutionVariableNameRegexp = String.Format(@"(?<={0})(\w+(?={0}))", SubstitutionVariableDelimiter);

        public string[] GetAllSubstitutionVariableNames(string source)
        {
            if (String.IsNullOrWhiteSpace(source))
                return new string[0];

            var allOccurenses = Regex.Matches(source, (string)this.AllowedSubstitutionVariableNameRegexp).OfType<Match>().Select(m => m.Value).Distinct();
            return allOccurenses.ToArray();
        }

        public string ReplaceSubstitutionVariable(string text, string variable, string replaceTo)
        {
            return text.Replace(String.Format("{1}{0}{1}", variable, SubstitutionVariableDelimiter), replaceTo);
        }

        public string RosterTitleSubstitutionReference { get { return "rostertitle"; } }

        public bool ContainsRosterTitle(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;

            return input.Contains(string.Format("{0}{1}{0}", SubstitutionVariableDelimiter, RosterTitleSubstitutionReference));
        }

        public string GenerateRosterName(string groupTitle, string rosterTitle)
        {
            return string.Format("{0} - {1}", groupTitle, string.IsNullOrEmpty(rosterTitle) ? DefaultSubstitutionText : rosterTitle);
        }

        public string DefaultSubstitutionText { get { return "[...]"; } }
    }
}
