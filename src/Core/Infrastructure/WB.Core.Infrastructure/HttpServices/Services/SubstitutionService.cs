using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.Infrastructure.HttpServices.Services
{
    public class SubstitutionService : ISubstitutionService
    {
        private const string SubstitutionVariableDelimiter = "%";
        private const string SelfVariableReference = "self";
        private const string AllowedSubstitutionVariableNameRegexp = "(?<=" + SubstitutionVariableDelimiter + @")(@?\w+(?=" + SubstitutionVariableDelimiter + "))";
        private const string TitleSubstitution = "rostertitle";
        private const string rosterTitle = SubstitutionVariableDelimiter + TitleSubstitution + SubstitutionVariableDelimiter;
        private static readonly Regex AllowedSubstitutionVariableNameRx = new Regex(AllowedSubstitutionVariableNameRegexp, RegexOptions.Compiled);
        private readonly ConcurrentDictionary<string, string[]> cache = new ConcurrentDictionary<string, string[]>();

        public const string RowCodeVariableName = "rowcode";
        public const string RowCodeVariableNameAt = "@rowcode";
        public const string RowIndexVariableName = "rowindex";
        public const string RowIndexVariableNameAt = "@rowindex";
        public const string RowNameVariableName = "rowname";
        public const string RowNameVariableNameAt = "@rowname";

        private static readonly HashSet<string> RosterServiceVariableNames = new HashSet<string>(StringComparer.Ordinal)
        {
            RowCodeVariableName, RowCodeVariableNameAt,
            RowIndexVariableName, RowIndexVariableNameAt,
            RowNameVariableName, RowNameVariableNameAt,
        };

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

        public bool IsRosterServiceVariable(string variableName)
        {
            return RosterServiceVariableNames.Contains(variableName);
        }

        public bool ContainsAnyRosterServiceVariable(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;
            foreach (var name in RosterServiceVariableNames)
            {
                if (input.Contains(SubstitutionVariableDelimiter + name + SubstitutionVariableDelimiter))
                    return true;
            }
            return false;
        }

        public string DefaultSubstitutionText => "[...]";
    }
}
