using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.SharedKernels.DataCollection.Utils
{
    public static class LinkedQuestionUtils
    {
        public static bool IsLevelAllowedToBeUsedAsLinkSourceInCurrentScope(decimal[] referensedLevelRosterVector, Guid[] referensedLevelRosterScopeVector,
           decimal[] linkedQuestionRosterVector, Guid[] linkedQuestionRosterScopeVector)
        {
            for (int i = 0; i < Math.Min(referensedLevelRosterVector.Length - 1, linkedQuestionRosterVector.Length); i++)
            {
                if (referensedLevelRosterScopeVector[i] != linkedQuestionRosterScopeVector[i])
                    continue;
                if (referensedLevelRosterVector[i] != linkedQuestionRosterVector[i])
                    return false;
            }
            return true;
        }

        public static string BuildLinkedQuestionOptionTitle(string referencedQuestionAnswer,
            Func<Guid, decimal[], string> getLevelName,
            decimal[] referensedQuestionRosterVector,
            Guid[] referensedQuestionRosterScopeVector, decimal[] linkedQuestionRosterVector, Guid[] linkedQuestionRosterScope,
            QuestionnaireRosterStructure questionnaireRosters)
        {
            var combinedRosterTitles = new List<string>();

            for (int i = 0; i < referensedQuestionRosterScopeVector.Length - 1; i++)
            {
                var scopeId = referensedQuestionRosterScopeVector[i];
                var rosterScopeDescription = questionnaireRosters.RosterScopes[scopeId];

                var firstScreenInScopeId = rosterScopeDescription.RosterIdToRosterTitleQuestionIdMap.Keys.First();
                var firstScreeninScopeRosterVector =
                    referensedQuestionRosterVector.Take(i + 1).ToArray();

                if (linkedQuestionRosterScope.Length > i && linkedQuestionRosterScope[i] == scopeId &&
                    linkedQuestionRosterVector.Length >= firstScreeninScopeRosterVector.Length)
                    continue;
                var levelName = getLevelName(firstScreenInScopeId, firstScreeninScopeRosterVector);
                if (!string.IsNullOrEmpty(levelName))
                    combinedRosterTitles.Add(levelName);
            }

            combinedRosterTitles.Add(referencedQuestionAnswer);

            return string.Join(": ", combinedRosterTitles.Where(title => !string.IsNullOrEmpty(title)));
        }
    }
}
