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
    using AtomicVerifier = Func<QuestionnaireDocument, QuestionnaireVerificationError>;

    internal class QuestionnaireVerifier : IQuestionnaireVerifier
    {
        private static readonly IEnumerable<AtomicVerifier> AtomicVerifiers = new AtomicVerifier[]
        {
            ErrorsByPropagatingQuestionsThatHasNoAssociatedGroups,
            ErrorsByPropagatedGroupsThatHasNoPropagatingQuestionsPointingToIt,
            ErrorsByPropagatedGroupsThatHasMoreThanOnePropagatingQuestionPointingToIt,

            ErrorsByQuestionsReferencedByLinkedQuestionsDoNotExist,
            ErrorsByLinkedQuestionReferenceQuestionOfNotSupportedType,
        };

        public IEnumerable<QuestionnaireVerificationError> Verify(QuestionnaireDocument questionnaire)
        {
            if (NoQuestionsExist(questionnaire))
                return new[] { new QuestionnaireVerificationError("WB0001", VerificationMessages.WB0001_NoQuestions) };

            return
                from verifier in AtomicVerifiers
                let error = verifier.Invoke(questionnaire)
                where error != null
                select error;
        }

        private static bool NoQuestionsExist(QuestionnaireDocument questionnaire)
        {
            return !questionnaire.Find<IQuestion>(_ => true).Any();
        }

        private static QuestionnaireVerificationError ErrorsByPropagatingQuestionsThatHasNoAssociatedGroups(
            QuestionnaireDocument questionnaire)
        {
            var autoPropagateQuestionsWithEmptyTriggers = questionnaire.Find<IAutoPropagateQuestion>(question => question.Triggers.Count == 0);

            return CreateQuestionnaireVerificationErrorForQuestions("WB0008",
                VerificationMessages.WB0008_PropagatingQuestionHasNoAssociatedGroups,
                autoPropagateQuestionsWithEmptyTriggers);
        }

        private static QuestionnaireVerificationError ErrorsByPropagatedGroupsThatHasNoPropagatingQuestionsPointingToIt(
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

        private static QuestionnaireVerificationError ErrorsByPropagatedGroupsThatHasMoreThanOnePropagatingQuestionPointingToIt(
            QuestionnaireDocument questionnaire)
        {
            IEnumerable<IGroup> propagatedGroupsWithMoreThanOnePropagatingQuestionPointingToThem = questionnaire.Find<IGroup>(group
                => IsGroupPropagatable(group)
                    && GetPropagatingQuestionsPointingToPropagatedGroup(group.PublicKey, questionnaire).Count() > 1);

            return CreateQuestionnaireVerificationErrorForGroups("WB0010",
                VerificationMessages.WB0010_PropagatedGroupHasMoreThanOnePropagatingQuestionPointingToThem,
                propagatedGroupsWithMoreThanOnePropagatingQuestionPointingToThem);
        }

        private static QuestionnaireVerificationError ErrorsByQuestionsReferencedByLinkedQuestionsDoNotExist(QuestionnaireDocument questionnaire)
        {
            var linkedQuestionsWithNotExistingSources =
                questionnaire.Find<IQuestion>(
                    question => question.LinkedToQuestionId.HasValue && questionnaire.Find<IQuestion>(question.LinkedToQuestionId.Value) == null);

            return CreateQuestionnaireVerificationErrorForQuestions("WB0011",
                VerificationMessages.WB0011_QuestionReferencedByLinkedQuestionDoesNotExist, linkedQuestionsWithNotExistingSources);
        }

        private static QuestionnaireVerificationError ErrorsByLinkedQuestionReferenceQuestionOfNotSupportedType(QuestionnaireDocument questionnaire)
        {
            Func<Guid, bool> isReferencedQuestionTypeSupported = questionId =>
            {
                var question = questionnaire.Find<IQuestion>(questionId);
                if (question == null)
                    return true;
                return question.QuestionType == QuestionType.Text
                    || question.QuestionType == QuestionType.Numeric
                    || question.QuestionType == QuestionType.DateTime;
            };

            var linkedQuestionsReferenceQuestionOfNotSupportedType =
                questionnaire.Find<IQuestion>(
                    question => question.LinkedToQuestionId.HasValue && !isReferencedQuestionTypeSupported(question.LinkedToQuestionId.Value));

            return CreateQuestionnaireVerificationErrorForQuestions("WB0012",
                VerificationMessages.WB0012_LinkedQuestionReferenceQuestionOfNotSupportedType, linkedQuestionsReferenceQuestionOfNotSupportedType);
        }

        private static QuestionnaireVerificationError CreateQuestionnaireVerificationErrorForQuestions(string code, string message,
            IEnumerable<IQuestion> questions)
        {
            QuestionnaireVerificationReference[] references =
                questions
                    .Select(q => new QuestionnaireVerificationReference(QuestionnaireVerificationReferenceType.Question, q.PublicKey))
                    .ToArray();

            return references.Any()
                ? new QuestionnaireVerificationError(code, message, references)
                : null;
        }

        private static QuestionnaireVerificationError CreateQuestionnaireVerificationErrorForGroups(string code, string message,
            IEnumerable<IGroup> groups)
        {
            QuestionnaireVerificationReference[] references = 
                groups
                    .Select(g => new QuestionnaireVerificationReference(QuestionnaireVerificationReferenceType.Group, g.PublicKey))
                    .ToArray();

            return references.Any()
                ? new QuestionnaireVerificationError(code, message, references)
                : null;
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