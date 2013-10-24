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
    using EnumerableVerifier = Func<QuestionnaireDocument, IEnumerable<QuestionnaireVerificationError>>;

    internal class QuestionnaireVerifier : IQuestionnaireVerifier
    {
        private readonly IEnumerable<AtomicVerifier> AtomicVerifiers;

        private readonly IEnumerable<EnumerableVerifier> EnumerableVerifiers;

        public QuestionnaireVerifier()
        {
            AtomicVerifiers = new AtomicVerifier[]
            {
            };

            EnumerableVerifiers = new EnumerableVerifier[]
            {
                ErrorsByPropagatingQuestionsThatHasNoAssociatedGroups,
                ErrorsByPropagatedGroupsThatHasMoreThanOnePropagatingQuestionPointingToIt,
                ErrorsByPropagatedGroupsThatHasNoPropagatingQuestionsPointingToIt,
                ErrorsByQuestionsReferencedByLinkedQuestionsDoNotExist,
                ErrorsByLinkedQuestionReferenceQuestionOfNotSupportedType
            };
        }

        public IEnumerable<QuestionnaireVerificationError> Verify(QuestionnaireDocument questionnaire)
        {
            if (NoQuestionsExist(questionnaire))
                return new[] { new QuestionnaireVerificationError("WB0001", VerificationMessages.WB0001_NoQuestions) };

            var errorsFromEnumerableVerifiers = EnumerableVerifiers.SelectMany(verifier => verifier.Invoke(questionnaire));
            var errorsFromAtomicVerifiers = AtomicVerifiers.Select(verifier => verifier.Invoke(questionnaire)).Where(error => error != null);

            return errorsFromEnumerableVerifiers.Union(errorsFromAtomicVerifiers);
        }

        private bool NoQuestionsExist(QuestionnaireDocument questionnaire)
        {
            return !questionnaire.Find<IQuestion>(_ => true).Any();
        }

        private IEnumerable<QuestionnaireVerificationError> ErrorsByPropagatingQuestionsThatHasNoAssociatedGroups(
            QuestionnaireDocument questionnaire)
        {
            var autoPropagateQuestionsWithEmptyTriggers = questionnaire.Find<IAutoPropagateQuestion>(question => question.Triggers.Count == 0);

            return CreateQuestionnaireVerificationErrorsForQuestions("WB0008",
                VerificationMessages.WB0008_PropagatingQuestionHasNoAssociatedGroups,
                autoPropagateQuestionsWithEmptyTriggers);
        }

        private IEnumerable<QuestionnaireVerificationError> ErrorsByPropagatedGroupsThatHasNoPropagatingQuestionsPointingToIt(
            QuestionnaireDocument questionnaire)
        {
            IEnumerable<IGroup> propagatedGroupsWithNoPropagatingQuestionsPointingToThem = questionnaire.Find<IGroup>(group
                => IsGroupPropagatable(group)
                    && !GetPropagatingQuestionsPointingToPropagatedGroup(@group.PublicKey, questionnaire).Any());

            return
                CreateQuestionnaireVerificationErrorsForGroups("WB0009",
                    VerificationMessages.WB0009_PropagatedGroupHaveNoPropagatingQuestionsPointingToThem,
                    propagatedGroupsWithNoPropagatingQuestionsPointingToThem);
        }

        private IEnumerable<QuestionnaireVerificationError> ErrorsByPropagatedGroupsThatHasMoreThanOnePropagatingQuestionPointingToIt(
            QuestionnaireDocument questionnaire)
        {
            IEnumerable<IGroup> propagatedGroups = questionnaire.Find<IGroup>(IsGroupPropagatable);
            foreach (var propagatedGroup in propagatedGroups)
            {
                var propagatingQuestionsPointingToPropagatedGroup =
                    GetPropagatingQuestionsPointingToPropagatedGroup(propagatedGroup.PublicKey, questionnaire);

                if (propagatingQuestionsPointingToPropagatedGroup.Count() < 2)
                    continue;

                var references = new List<QuestionnaireVerificationReference>();

                references.Add(this.CreateVerificationReferenceForGroup(propagatedGroup));
                references.AddRange(propagatingQuestionsPointingToPropagatedGroup.Select(this.CreateVerificationReferenceForQuestion));

                yield return new QuestionnaireVerificationError("WB0010",
                    VerificationMessages.WB0010_PropagatedGroupHasMoreThanOnePropagatingQuestionPointingToThem, references.ToArray());
            }
        }

        private IEnumerable<QuestionnaireVerificationError> ErrorsByQuestionsReferencedByLinkedQuestionsDoNotExist(
            QuestionnaireDocument questionnaire)
        {
            var linkedQuestionsWithNotExistingSources =
                questionnaire.Find<IQuestion>(
                    question =>
                        question.LinkedToQuestionId.HasValue && questionnaire.Find<IQuestion>(question.LinkedToQuestionId.Value) == null);

            return CreateQuestionnaireVerificationErrorsForQuestions("WB0011",
                VerificationMessages.WB0011_QuestionReferencedByLinkedQuestionDoesNotExist,
                linkedQuestionsWithNotExistingSources);
        }

        private IEnumerable<QuestionnaireVerificationError> ErrorsByLinkedQuestionReferenceQuestionOfNotSupportedType(
            QuestionnaireDocument questionnaire)
        {

            var linkedQuestions = questionnaire.Find<IQuestion>(
                question => question.LinkedToQuestionId.HasValue);

            foreach (var linkedQuestion in linkedQuestions)
            {
                var sourceQuestion = questionnaire.Find<IQuestion>(linkedQuestion.LinkedToQuestionId.Value);
                if (sourceQuestion == null)
                    continue;

                bool isSourceQuestionValidType = sourceQuestion.QuestionType == QuestionType.Text
                    || sourceQuestion.QuestionType == QuestionType.Numeric
                    || sourceQuestion.QuestionType == QuestionType.DateTime;
                
                if (isSourceQuestionValidType)
                    continue;

                var references = new[]
                {
                    this.CreateVerificationReferenceForQuestion(linkedQuestion), 
                    this.CreateVerificationReferenceForQuestion(sourceQuestion)
                };

                yield return new QuestionnaireVerificationError("WB0012",
                    VerificationMessages.WB0012_LinkedQuestionReferenceQuestionOfNotSupportedType, references);
            }
        }

        private QuestionnaireVerificationError CreateQuestionnaireVerificationErrorForQuestions(string code, string message,
            params IQuestion[] questions)
        {
            QuestionnaireVerificationReference[] references =
                questions
                    .Select(CreateVerificationReferenceForQuestion)
                    .ToArray();

            return references.Any()
                ? new QuestionnaireVerificationError(code, message, references)
                : null;
        }

        private IEnumerable<QuestionnaireVerificationError> CreateQuestionnaireVerificationErrorsForQuestions(string code, string message,
            IEnumerable<IQuestion> questions)
        {
            return questions.Select(q => CreateQuestionnaireVerificationErrorForQuestions(code, message, q));
        }

        private QuestionnaireVerificationError CreateQuestionnaireVerificationErrorForGroups(string code, string message,
            params IGroup[] groups)
        {
            QuestionnaireVerificationReference[] references = 
                groups
                    .Select(CreateVerificationReferenceForGroup)
                    .ToArray();

            return references.Any()
                ? new QuestionnaireVerificationError(code, message, references)
                : null;
        }

        private IEnumerable<QuestionnaireVerificationError> CreateQuestionnaireVerificationErrorsForGroups(string code, string message,
           IEnumerable<IGroup> questions)
        {
            return questions.Select(q => CreateQuestionnaireVerificationErrorForGroups(code, message, q));
        }

        private QuestionnaireVerificationReference CreateVerificationReferenceForGroup(IGroup group)
        {
            return new QuestionnaireVerificationReference(QuestionnaireVerificationReferenceType.Group, group.PublicKey);
        }

        private QuestionnaireVerificationReference CreateVerificationReferenceForQuestion(IQuestion question)
        {
            return new QuestionnaireVerificationReference(QuestionnaireVerificationReferenceType.Question, question.PublicKey);
        }

        private bool IsGroupPropagatable(IGroup group)
        {
            return group.Propagated == Propagate.AutoPropagated;
        }

        private IEnumerable<IQuestion> GetPropagatingQuestionsPointingToPropagatedGroup(Guid groupId, QuestionnaireDocument document)
        {
            return document.Find<IAutoPropagateQuestion>(question => question.Triggers.Contains(groupId));
        }
    }
}