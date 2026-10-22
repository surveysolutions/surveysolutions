using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport
{
    public class AudioAuditScopeResolutionResult
    {
        /// <summary>Canonical variable names that resolved to a section, group or non-flat roster.</summary>
        public string[] VariableNames { get; set; } = Array.Empty<string>();

        /// <summary>Variable names that could not be resolved to a section, group or roster.</summary>
        public string[] InvalidVariableNames { get; set; } = Array.Empty<string>();

        public bool HasErrors => this.InvalidVariableNames.Length > 0;
    }

    /// <summary>
    /// Validates selective Audio Audit scope variable names (from the <c>_aascope</c> import column or
    /// the assignment API) against the questionnaire. Only sections, groups and non-flat rosters are
    /// allowed; questions, flat rosters, unknown and ambiguous names are reported as invalid. Valid names
    /// are returned in their canonical questionnaire casing so the scope can be stored as variable names.
    /// </summary>
    public static class AudioAuditScopeResolver
    {
        public static AudioAuditScopeResolutionResult Resolve(IQuestionnaire questionnaire, IEnumerable<string> variableNames)
        {
            var resolvedNames = new List<string>();
            var invalid = new List<string>();

            if (variableNames == null)
                return new AudioAuditScopeResolutionResult();

            // Build lookup once so we don't call GetAllGroups() repeatedly inside the loop.
            var groupLookup = questionnaire.GetAllGroups()
                .GroupBy(id => questionnaire.GetRosterVariableName(id) ?? string.Empty,
                    StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

            foreach (var rawName in variableNames)
            {
                var name = rawName?.Trim();
                if (string.IsNullOrEmpty(name))
                    continue;

                groupLookup.TryGetValue(name, out var matchingGroupIds);
                matchingGroupIds ??= new List<Guid>();

                if (matchingGroupIds.Count == 1)
                {
                    var entityId = matchingGroupIds[0];

                    // Flat rosters can't be selected as an Audio Audit scope.
                    if (questionnaire.IsFlatRoster(entityId))
                    {
                        invalid.Add(name);
                        continue;
                    }

                    // Store the canonical variable name so the scope survives questionnaire upgrades.
                    var canonicalName = questionnaire.GetRosterVariableName(entityId) ?? name;
                    if (!resolvedNames.Contains(canonicalName, StringComparer.OrdinalIgnoreCase))
                        resolvedNames.Add(canonicalName);
                }
                else
                {
                    // Zero matches (unknown / question) or more than one match (ambiguous) are all invalid.
                    invalid.Add(name);
                }
            }

            return new AudioAuditScopeResolutionResult
            {
                VariableNames = resolvedNames.ToArray(),
                InvalidVariableNames = invalid.ToArray()
            };
        }
    }
}
