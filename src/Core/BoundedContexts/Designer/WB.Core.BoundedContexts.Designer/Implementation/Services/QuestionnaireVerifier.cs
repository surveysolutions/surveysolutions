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

            this.AddGroupOfErrorsByQuestionnaire(errorList, questionnaire, this.ErrorsByPropagatingQuestionsThatHasNoAssociatedGroups);
            this.AddGroupOfErrorsByQuestionnaire(errorList, questionnaire,
                this.ErrorsByPropagatedGroupsThatHasNoPropagatingQuestionsPointingToIt);
            this.AddGroupOfErrorsByQuestionnaire(errorList, questionnaire,
                this.ErrorsByPropagatedGroupsThatHasMoreThanOnePropagatingQuestionPointingToIt);

            this.AddGroupOfErrorsByQuestionnaire(errorList, questionnaire, this.ErrorsByQuestionsReferencedByLinkedQuestionsDoNotExist);

            return errorList;
        }

        private bool NoQuestionsExist(QuestionnaireDocument questionnaire)
        {
            return !questionnaire.Find<IQuestion>(_ => true).Any();
        }

        private void AddGroupOfErrorsByQuestionnaire(List<QuestionnaireVerificationError> errorList, QuestionnaireDocument questionnaire,
            Func<QuestionnaireDocument, QuestionnaireVerificationError> errorChecker)
        {
            var error = errorChecker(questionnaire);
            if (error.References.Any())
                errorList.Add(error);
        }

        private QuestionnaireVerificationError ErrorsByPropagatingQuestionsThatHasNoAssociatedGroups(
            QuestionnaireDocument questionnaire)
        {
            var autoPropagateQuestionsWithEmptyTriggers = questionnaire.Find<IAutoPropagateQuestion>(question => question.Triggers.Count == 0);

            return CreateQuestionnaireVerificationErrorForQuestions("WB0008",
                VerificationMessages.WB0008_PropagatingQuestionHasNoAssociatedGroups,
                autoPropagateQuestionsWithEmptyTriggers);
        }

        private QuestionnaireVerificationError ErrorsByPropagatedGroupsThatHasNoPropagatingQuestionsPointingToIt(
            QuestionnaireDocument questionnaire)
        {
            IEnumerable<IGroup> propagatedGroupsWithNoPropagatingQuestionsPointingToThem = questionnaire.Find<IGroup>(group
                => IsGroupPropagatable(group)
                    && !GetPropagatingQuestionsPointingToPropagatedGroup(@group.PublicKey, questionnaire).Any());

            return
                CreateQuestionnaireVerificationErrorForGroups("WB0009",
                    VerificationMessages.WB0009_PropagatedGroupHaveNoPropagatingQuestionsPointingToThem,
                    propagatedGroupsWithNoPropagatingQuestionsPointingToThem);
        }

        private QuestionnaireVerificationError ErrorsByPropagatedGroupsThatHasMoreThanOnePropagatingQuestionPointingToIt(
            QuestionnaireDocument questionnaire)
        {
            IEnumerable<IGroup> propagatedGroupsWithMoreThanOnePropagatingQuestionPointingToThem = questionnaire.Find<IGroup>(group
                => IsGroupPropagatable(group)
                    && GetPropagatingQuestionsPointingToPropagatedGroup(group.PublicKey, questionnaire).Count() > 1);

            return this.CreateQuestionnaireVerificationErrorForGroups("WB0010",
                VerificationMessages.WB0010_PropagatedGroupHasMoreThanOnePropagatingQuestionPointingToThem,
                propagatedGroupsWithMoreThanOnePropagatingQuestionPointingToThem);
        }

        private QuestionnaireVerificationError ErrorsByQuestionsReferencedByLinkedQuestionsDoNotExist(QuestionnaireDocument document)
        {
            var linkedQuestionsWithNotExistingSources =
                document.Find<IQuestion>(
                    question => question.LinkedToQuestionId.HasValue && document.Find<IQuestion>(question.LinkedToQuestionId.Value) == null);

            return CreateQuestionnaireVerificationErrorForQuestions("WB0011",
                VerificationMessages.WB0011_QuestionReferencedByLinkedQuestionDoesNotExist, linkedQuestionsWithNotExistingSources);
        }

        private QuestionnaireVerificationError CreateQuestionnaireVerificationErrorForQuestions(string code, string message,
            IEnumerable<IQuestion> questions)
        {
            return new QuestionnaireVerificationError(code, message,
                questions.Select(q => new QuestionnaireVerificationReference(QuestionnaireVerificationReferenceType.Question, q.PublicKey))
                    .ToArray());
        }

        private QuestionnaireVerificationError CreateQuestionnaireVerificationErrorForGroups(string code, string message,
            IEnumerable<IGroup> groups)
        {
            return new QuestionnaireVerificationError(code, message,
                groups.Select(g => new QuestionnaireVerificationReference(QuestionnaireVerificationReferenceType.Group, g.PublicKey))
                    .ToArray());
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