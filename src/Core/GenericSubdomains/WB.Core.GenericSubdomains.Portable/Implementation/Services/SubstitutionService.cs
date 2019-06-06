using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text.RegularExpressions;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.GenericSubdomains.Portable.Implementation.Services
{
    public class SubstitutionService : ISubstitutionService
    {
        private const string SubstitutionVariableDelimiter = "%";
        private const string SelfVariableReference = "self";
        private static readonly string AllowedSubstitutionVariableNameRegexp = String.Format(@"(?<={0})(\w+(?={0}))", SubstitutionVariableDelimiter);
        private static readonly Regex AllowedSubstitutionVariableNameRx = new Regex(AllowedSubstitutionVariableNameRegexp, RegexOptions.Compiled);
        private readonly ConcurrentDictionary<string, string[]> cache = new ConcurrentDictionary<string, string[]>();
        private readonly string rosterTitle;

        public SubstitutionService()
        {
            rosterTitle = string.Format("{0}{1}{0}", SubstitutionVariableDelimiter, RosterTitleSubstitutionReference);
        }

        public string[] GetAllSubstitutionVariableNames(string source, string selfVariable)
        {
            if (String.IsNullOrWhiteSpace(source))
                return Array.Empty<string>();

            return cache.GetOrAdd(source, s =>
            {
                var allOccurenses = AllowedSubstitutionVariableNameRx.Matches(s)
                    .OfType<Match>()
                    .Select(m => m.Value.Equals(SelfVariableReference, StringComparison.Ordinal) ? selfVariable : m.Value)
                    .Distinct();
                
                return allOccurenses.ToArray();
            });
        }

        public string ReplaceSubstitutionVariable(string text, string selfVariableName, string variable, string replaceTo)
        {
            var result = text;
            if (!string.IsNullOrEmpty(selfVariableName) && selfVariableName.Equals(variable, StringComparison.Ordinal))
            {
                result = result.Replace($"{SubstitutionVariableDelimiter}{SelfVariableReference}{SubstitutionVariableDelimiter}",
                    replaceTo);
            }

            result = result.Replace($"{SubstitutionVariableDelimiter}{variable}{SubstitutionVariableDelimiter}", replaceTo);
            return result;
        }

        public string RosterTitleSubstitutionReference => "rostertitle";

        public bool ContainsRosterTitle(string input)
        {
            return !string.IsNullOrEmpty(input) && input.Contains(this.rosterTitle);
        }

        public string GenerateRosterName(string groupTitle, string rosterInstanceTitle)
        {
            return string.Format("{0} - {1}", groupTitle, string.IsNullOrEmpty(rosterInstanceTitle) ? DefaultSubstitutionText : rosterInstanceTitle);
        }

        public string DefaultSubstitutionText => "[...]";
    }
}
