using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.ValueObjects;

namespace WB.Core.BoundedContexts.Headquarters.Views
{
    [Obsolete("please don't use this class or extend it, the class serves only one purpose - support linked questions barward compatibility. v5.7")]
    public static class InterviewLevelUtils
    {
        public static InterviewLevel[] GetAvailableOptionsForQuestionLinkedOnRoster(
            IQuestion question,
            RosterVector questionRosterVector,
            InterviewData interview,
            QuestionnaireDocument questionnaire,
            InterviewLinkedQuestionOptions storedLinkedOptions)
        {
            if (!question.LinkedToRosterId.HasValue && !question.LinkedToQuestionId.HasValue)
                return null;

            var questionStringKey = new Identity(question.PublicKey, questionRosterVector).ToString();

            var referencedEntity =
                questionnaire.Find<IComposite>(question.LinkedToRosterId ?? question.LinkedToQuestionId.Value);

            var referencedEntityScope = GetRosterSizeSourcesForEntity(referencedEntity);

            if (storedLinkedOptions != null && storedLinkedOptions.LinkedQuestionOptions.ContainsKey(questionStringKey))
            {
                return
                    storedLinkedOptions.LinkedQuestionOptions[questionStringKey].Select(
                        option => interview.Levels.Values.FirstOrDefault(
                            x =>
                                x.RosterVector.SequenceEqual(option.CoordinatesAsDecimals) &&
                                x.ScopeVectors.Keys.Any(v => v.SequenceEqual(referencedEntityScope)))
                        ).Where(l => l != null).ToArray();
            }

            var linkedQuestionRosterScope = GetRosterSizeSourcesForEntity(question);

            var interviewLevelsWithLinkedOptions = FindLevelsByScope(interview, referencedEntityScope)
                        .Where(l => IsLevelAllowedToBeUsedAsLinkSourceInCurrentScope(l.RosterVector, questionRosterVector, referencedEntityScope, linkedQuestionRosterScope))
                        .ToArray();

            if (question.LinkedToRosterId.HasValue)
            {
                return interviewLevelsWithLinkedOptions.ToArray();
            }
            return interviewLevelsWithLinkedOptions
                    .Where(l => IsSourceOfLinkedQuestionEnabledAndAnswered(l, question.LinkedToQuestionId.Value))
                    .ToArray();
        }

        private static bool IsSourceOfLinkedQuestionEnabledAndAnswered(InterviewLevel level, Guid sourceOfLinkQuestionId)
        {
            if (!level.QuestionsSearchCache.ContainsKey(sourceOfLinkQuestionId))
                return false;
            var question = level.QuestionsSearchCache[sourceOfLinkQuestionId];
            return !question.IsDisabled() && question.Answer != null;
        }

        public static InterviewLevel[] FindLevelsByScope(InterviewData interview, ValueVector<Guid> scopeVector)
        {
            return interview.Levels.Values
                .Where(x => x.ScopeVectors.Keys.Any(v => v.SequenceEqual(scopeVector)))
                .ToArray();
        }

        public static ValueVector<Guid> GetRosterSizeSourcesForEntity(IComposite entity)
        {
            var rosterSizes = new List<Guid>();
            while (!(entity is IQuestionnaireDocument))
            {
                var group = entity as IGroup;
                if (group != null)
                {
                    if (group.IsRoster)
                        rosterSizes.Add(group.RosterSizeQuestionId ?? group.PublicKey);

                }
                entity = entity.GetParent();
            }
            rosterSizes.Reverse();
            return rosterSizes.ToArray();
        }

        private static bool IsLevelAllowedToBeUsedAsLinkSourceInCurrentScope(
            decimal[] referencedLevelRosterVector,
            decimal[] linkedQuestionRosterVector,
            ValueVector<Guid> referencedLevelRosterScopeVector,
            ValueVector<Guid> linkedQuestionRosterScopeVector)
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
    }
}