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
        private const string AllowedSubstitutionVariableNameRegexp = "(?<=" + SubstitutionVariableDelimiter + @")(\w+(?=" + SubstitutionVariableDelimiter + "))";
        private const string TitleSubstitution = "rostertitle";
        private const string rosterTitle = SubstitutionVariableDelimiter + TitleSubstitution + SubstitutionVariableDelimiter;
        private static readonly Regex AllowedSubstitutionVariableNameRx = new Regex(AllowedSubstitutionVariableNameRegexp, RegexOptions.Compiled);
        private readonly ConcurrentDictionary<string, string[]> cache = new ConcurrentDictionary<string, string[]>();

        public string[] GetAllSubstitutionVariableNames(string source, string selfVariable)
        {
            if (String.IsNullOrWhiteSpace(source))
                return Array.Empty<string>();

            return cache.GetOrAdd(source + selfVariable, s =>
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

        public string RosterTitleSubstitutionReference => TitleSubstitution;

        public bool ContainsRosterTitle(string input)
        {
            return !string.IsNullOrEmpty(input) && input.Contains(rosterTitle);
        }

        public string DefaultSubstitutionText => "[...]";
    }
}
