using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport
{
    public class AudioAuditScopeResolutionResult
    {
        public Guid[] EntityIds { get; set; } = Array.Empty<Guid>();

        /// <summary>Variable names that could not be resolved to a section, group or roster.</summary>
        public string[] InvalidVariableNames { get; set; } = Array.Empty<string>();

        public bool HasErrors => this.InvalidVariableNames.Length > 0;
    }

    /// <summary>
    /// Resolves selective Audio Audit scope variable names (from the <c>_aascope</c> import column or
    /// the assignment API) into questionnaire entity ids. Only sections, groups and non-flat rosters are
    /// allowed; questions, flat rosters, unknown and ambiguous names are reported as invalid.
    /// </summary>
    public static class AudioAuditScopeResolver
    {
        public static AudioAuditScopeResolutionResult Resolve(IQuestionnaire questionnaire, IEnumerable<string> variableNames)
        {
            var entityIds = new List<Guid>();
            var invalid = new List<string>();

            if (variableNames == null)
                return new AudioAuditScopeResolutionResult();

            foreach (var rawName in variableNames)
            {
                var name = rawName?.Trim();
                if (string.IsNullOrEmpty(name))
                    continue;

                var matchingGroupIds = questionnaire.GetAllGroups()
                    .Where(groupId => string.Equals(questionnaire.GetRosterVariableName(groupId), name,
                        StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (matchingGroupIds.Count == 1)
                {
                    var entityId = matchingGroupIds[0];

                    // Flat rosters can't be selected as an Audio Audit scope.
                    if (questionnaire.IsFlatRoster(entityId))
                    {
                        invalid.Add(name);
                        continue;
                    }

                    if (!entityIds.Contains(entityId))
                        entityIds.Add(entityId);
                }
                else
                {
                    // Zero matches (unknown / question) or more than one match (ambiguous) are all invalid.
                    invalid.Add(name);
                }
            }

            return new AudioAuditScopeResolutionResult
            {
                EntityIds = entityIds.ToArray(),
                InvalidVariableNames = invalid.ToArray()
            };
        }
    }
}
