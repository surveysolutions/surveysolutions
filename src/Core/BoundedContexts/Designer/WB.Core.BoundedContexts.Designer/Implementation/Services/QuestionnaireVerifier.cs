using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects.Verification;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    internal class QuestionnaireVerifier : IQuestionnaireVerifier
    {
        public IEnumerable<QuestionnaireVerificationError> Verify(QuestionnaireDocument questionnaire)
        {
            if (NoQuestionsExist(questionnaire))
                return new[] { new QuestionnaireVerificationError("WB0001", VerificationMessages.WB0001_NoQuestions) };

            var errorList = new List<QuestionnaireVerificationError>();

            errorList.AddRange(this.GetListOfErrorsByPropagatingQuestionsThatHasNoAssociatedGroups(questionnaire));
            errorList.AddRange(this.GetListOfErrorsByPropagatedGroupsThatHasNoPropagatingQuestionsPointingToIt(questionnaire));
            errorList.AddRange(this.GetListOfErrorsByPropagatedGroupsThatHasMoreThanOnePropagatingQuestionPointingToIt(questionnaire));
            
            return errorList;
        }

        private bool NoQuestionsExist(QuestionnaireDocument questionnaire)
        {
            return !questionnaire.Find<IQuestion>(_ => true).Any();
        }

        private IEnumerable<QuestionnaireVerificationError> GetListOfErrorsByPropagatingQuestionsThatHasNoAssociatedGroups(
            QuestionnaireDocument questionnaire)
        {
            return questionnaire.Find<IAutoPropagateQuestion>(question => question.Triggers.Count == 0).Select(
                question =>
                    new QuestionnaireVerificationError("WB0008", VerificationMessages.WB0008_PropagatingQuestionHasNoAssociatedGroups,
                        new QuestionnaireVerificationReference(QuestionnaireVerificationReferenceType.Question, question.PublicKey)));
        }

        private IEnumerable<QuestionnaireVerificationError> GetListOfErrorsByPropagatedGroupsThatHasNoPropagatingQuestionsPointingToIt(
           QuestionnaireDocument questionnaire)
        {
            IEnumerable<IGroup> propagatedGroupsWithNoPropagatingQuestionsPointingToThem = questionnaire.Find<IGroup>(group
                => IsGroupPropagatable(group)
                && !GetPropagatingQuestionsPointingToPropagatedGroup(@group.PublicKey, questionnaire).Any());
            return
                propagatedGroupsWithNoPropagatingQuestionsPointingToThem.Select(
                    group =>
                        new QuestionnaireVerificationError("WB0009",
                            VerificationMessages.WB0009_PropagatedGroupHaveNoPropagatingQuestionsPointingToThem,
                            new QuestionnaireVerificationReference(QuestionnaireVerificationReferenceType.Group, group.PublicKey)));
        }

        private IEnumerable<QuestionnaireVerificationError> GetListOfErrorsByPropagatedGroupsThatHasMoreThanOnePropagatingQuestionPointingToIt(QuestionnaireDocument questionnaire)
        {
            IEnumerable<IGroup> propagatedGroupsWithMoreThanOnePropagatingQuestionPointingToThem = questionnaire.Find<IGroup>(group
               => IsGroupPropagatable(group)
               && GetPropagatingQuestionsPointingToPropagatedGroup(group.PublicKey, questionnaire).Count() > 1);
            return
               propagatedGroupsWithMoreThanOnePropagatingQuestionPointingToThem.Select(
                   group =>
                       new QuestionnaireVerificationError("WB0010",
                           VerificationMessages.WB0010_PropagatedGroupHasMoreThanOnePropagatingQuestionPointingToThem,
                           new QuestionnaireVerificationReference(QuestionnaireVerificationReferenceType.Group, group.PublicKey)));
        }

        private static bool IsGroupPropagatable(IGroup group)
        {
            return group.Propagated == Propagate.AutoPropagated;
        }

        private static IEnumerable<IQuestion> GetPropagatingQuestionsPointingToPropagatedGroup(Guid groupId, QuestionnaireDocument document)
        {
            return document.Find<IAutoPropagateQuestion>(question => question.Triggers.Contains(groupId));
        }
    }
}