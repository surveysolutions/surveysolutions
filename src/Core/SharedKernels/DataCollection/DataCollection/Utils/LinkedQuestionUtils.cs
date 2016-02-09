using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.SharedKernels.DataCollection.Utils
{
    public static class LinkedQuestionUtils
    {
        public static bool IsLevelAllowedToBeUsedAsLinkSourceInCurrentScope(decimal[] referencedLevelRosterVector, ValueVector<Guid> referencedLevelRosterScopeVector,
           decimal[] linkedQuestionRosterVector, ValueVector<Guid> linkedQuestionRosterScopeVector)
        {
            for (int i = 0; i < Math.Min(referencedLevelRosterVector.Length - 1, linkedQuestionRosterVector.Length); i++)
            {
                if (referencedLevelRosterScopeVector[i] != linkedQuestionRosterScopeVector[i])
                    continue;
                if (referencedLevelRosterVector[i] != linkedQuestionRosterVector[i])
                    return false;
            }
            return true;
        }

        public static string BuildLinkedQuestionOptionTitle(
            string referencedQuestionAnswer,
            Func<decimal[], string> getLevelName,
            decimal[] referencedQuestionRosterVector,
            ValueVector<Guid> referencedQuestionRosterScopeVector, 
            decimal[] linkedQuestionRosterVector, 
            ValueVector<Guid> linkedQuestionRosterScope)
        {
            var combinedRosterTitles = new List<string>();

            for (int i = 0; i < referencedQuestionRosterScopeVector.Length - 1; i++)
            {
                var scopeId = referencedQuestionRosterScopeVector[i];
             /*   var rosterScopeDescription = questionnaireRosters.RosterScopes[new ValueVector<Guid>(referensedQuestionRosterScopeVector.Take(i+1).ToArray())];

                var firstScreenInScopeId = rosterScopeDescription.RosterIdToRosterTitleQuestionIdMap.Keys.First();*/
                var firstScreeninScopeRosterVector =
                    referencedQuestionRosterVector.Take(i + 1).ToArray();

                if (linkedQuestionRosterScope.Length > i && linkedQuestionRosterScope[i] == scopeId &&
                    linkedQuestionRosterVector.Length >= firstScreeninScopeRosterVector.Length)
                    continue;
                var levelName = getLevelName(firstScreeninScopeRosterVector);
                if (!string.IsNullOrEmpty(levelName))
                    combinedRosterTitles.Add(levelName);
            }

            combinedRosterTitles.Add(referencedQuestionAnswer);

            return string.Join(": ", combinedRosterTitles.Where(title => !string.IsNullOrEmpty(title)));
        }
    }
}
