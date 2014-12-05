using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Factories
{
    internal class ReferenceInfoForLinkedQuestionsFactory : IReferenceInfoForLinkedQuestionsFactory
    {
        public ReferenceInfoForLinkedQuestions CreateReferenceInfoForLinkedQuestions(QuestionnaireDocument questionnaire, long version)
        {
            var referenceInfoForLinkedQuestions = new ReferenceInfoForLinkedQuestions();
            referenceInfoForLinkedQuestions.QuestionnaireId = questionnaire.PublicKey;
            referenceInfoForLinkedQuestions.Version = version;

            var referenceInfo = new Dictionary<Guid, ReferenceInfoByQuestion>();

            var linkedQuestions = this.GetAllLinkedQuestions(questionnaire);

            var groupsMappedOnPropagatableQuestion = this.GetAllRosterScopesGroupedByRosterId(questionnaire);

            foreach (var linkedQuestion in linkedQuestions)
            {
                var referencedQuestion =
                    questionnaire.FirstOrDefault<IQuestion>(question => question.PublicKey == linkedQuestion.LinkedToQuestionId.Value);

                var scopeOfLinkedQuestion = GetScopeOfReferencedQuestions(linkedQuestion, groupsMappedOnPropagatableQuestion);
                var scopeOfReferenceQuestion = GetScopeOfReferencedQuestions(referencedQuestion, groupsMappedOnPropagatableQuestion);
                referenceInfo[linkedQuestion.PublicKey] = new ReferenceInfoByQuestion(
                    referencedQuestion.PublicKey, scopeOfReferenceQuestion, scopeOfLinkedQuestion);
     
            }

            referenceInfoForLinkedQuestions.ReferencesOnLinkedQuestions = referenceInfo;
            return referenceInfoForLinkedQuestions;
        }

        private ValueVector<Guid> GetScopeOfReferencedQuestions(IQuestion referencedQuestion,
            IDictionary<Guid, Guid> groupsMappedOnPropagatableQuestion)
        {
            var result = new List<Guid>();
            var questionParent = referencedQuestion.GetParent();

            while (questionParent != null)
            {
                var group = questionParent as IGroup;
                if (group != null && (group.IsRoster))
                {
                    result.Add(groupsMappedOnPropagatableQuestion[group.PublicKey]);
                }
                questionParent = questionParent.GetParent();
            }
            result.Reverse();

            return new ValueVector<Guid>(result);
        }


        private IDictionary<Guid, Guid> GetAllRosterScopesGroupedByRosterId(QuestionnaireDocument template)
        {
            var result = new Dictionary<Guid, Guid>();

            foreach (var roster in template.Find<IGroup>(group => group.IsRoster && group.RosterSizeSource == RosterSizeSourceType.Question))
            {
                result.Add(roster.PublicKey, roster.RosterSizeQuestionId.Value);
            }

            foreach (var roster in template.Find<IGroup>(group => group.IsRoster && group.RosterSizeSource == RosterSizeSourceType.FixedTitles))
            {
                result.Add(roster.PublicKey, roster.PublicKey);
            }

            return result;
        }

        private IEnumerable<IQuestion> GetAllLinkedQuestions(QuestionnaireDocument template)
        {
            return template.Find<IQuestion>(
                question =>
                    (question.QuestionType == QuestionType.SingleOption || question.QuestionType == QuestionType.MultyOption) &&
                        question.LinkedToQuestionId.HasValue);
        }
    }
}
