using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Main.Core.Utility;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects.Verification;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    using AtomicVerifier = Func<QuestionnaireDocument, QuestionnaireVerificationError>;
    using EnumerableVerifier = Func<QuestionnaireDocument, IEnumerable<QuestionnaireVerificationError>>;

    internal class QuestionnaireVerifier : IQuestionnaireVerifier
    {
        private readonly IEnumerable<QuestionType> QuestionTypesValidToBeLinkedQuestionSource = new[]
        { QuestionType.DateTime, QuestionType.Numeric, QuestionType.Text };

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

                ErrorsByLinkedQuestions,

                ErrorsByQuestionsWithSubstitutions
            };
        }

        private IEnumerable<QuestionnaireVerificationError> ErrorsByQuestionsWithSubstitutions(QuestionnaireDocument questionnaire)
        {
            IEnumerable<IQuestion> questionsWithSubstitutions =
                questionnaire.Find<IQuestion>(question => StringUtil.GetAllSubstitutionVariableNames(question.QuestionText).Length > 0);

            foreach (var questionsWithSubstitution in questionsWithSubstitutions)
            {
                if (questionsWithSubstitution.Featured)
                {
                    yield return
                        new QuestionnaireVerificationError("WB0015", VerificationMessages.WB0015_QuestionHaveIncorrectSubstitutionCantBeFeatured,
                            new QuestionnaireVerificationReference(QuestionnaireVerificationReferenceType.Question,
                                questionsWithSubstitution.PublicKey));
                    continue;
                    
                }
                var substitutionReferences = StringUtil.GetAllSubstitutionVariableNames(questionsWithSubstitution.QuestionText);
                foreach (var substitutionReference in substitutionReferences)
                {
                    if (substitutionReference == questionsWithSubstitution.StataExportCaption)
                    {
                        yield return
                            new QuestionnaireVerificationError("WB0016",
                                VerificationMessages.WB0016_QuestionWithSubstitutionsCantHaveSelfReferences,
                                new QuestionnaireVerificationReference(QuestionnaireVerificationReferenceType.Question,
                                    questionsWithSubstitution.PublicKey));
                        continue;
                    }
                    var questionSourceOfSubstitution = questionnaire.FirstOrDefault<IQuestion>(q => q.StataExportCaption == substitutionReference);
                    if (questionSourceOfSubstitution == null)
                    {
                        yield return
                            new QuestionnaireVerificationError("WB0017",
                                VerificationMessages.WB0017_QuestionReferencedByQuestionWithSubstitutionsDoesNotExist,
                                new QuestionnaireVerificationReference(QuestionnaireVerificationReferenceType.Question,
                                    questionsWithSubstitution.PublicKey));
                        continue;
                    }
                }
            }
        }

        public IEnumerable<QuestionnaireVerificationError> Verify(QuestionnaireDocument questionnaire)
        {
            if (NoQuestionsExist(questionnaire))
                return new[] { new QuestionnaireVerificationError("WB0001", VerificationMessages.WB0001_NoQuestions) };
            
            questionnaire.ConnectChildrenWithParent();

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
            var autoPropagateQuestionsWithEmptyTriggers = questionnaire.Find<IAutoPropagateQuestion>(question => question.Triggers.Count == 0).ToArray();

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

        private IEnumerable<QuestionnaireVerificationError> ErrorsByLinkedQuestions(
            QuestionnaireDocument questionnaire)
        {
            var linkedQuestions = questionnaire.Find<IQuestion>(
                question => question.LinkedToQuestionId.HasValue);

            foreach (var linkedQuestion in linkedQuestions)
            {
                var sourceQuestion = questionnaire.Find<IQuestion>(linkedQuestion.LinkedToQuestionId.Value);

                if (sourceQuestion == null)
                {
                    yield return QuestionReferencedByLinkedQuestionDoesNotExistError(linkedQuestion);
                    continue;
                }

                bool isSourceQuestionValidType = QuestionTypesValidToBeLinkedQuestionSource.Contains(sourceQuestion.QuestionType);
                if (!isSourceQuestionValidType)
                {
                    yield return LinkedQuestionReferenceQuestionOfNotSupportedTypeError(linkedQuestion, sourceQuestion);
                    continue;
                }

                var isSourceQuestionInsidePropagatedGroup = GetAllParentGroupsForQuestion(sourceQuestion, questionnaire).Any(IsGroupPropagatable);
                if (!isSourceQuestionInsidePropagatedGroup)
                {
                    yield return LinkedQuestionReferenceQuestionNotUnderPropagatedGroup(linkedQuestion, sourceQuestion);
                }
            }
        }

        private QuestionnaireVerificationError QuestionReferencedByLinkedQuestionDoesNotExistError(IQuestion linkedQuestion)
        {
            return new QuestionnaireVerificationError("WB0011",
                       VerificationMessages.WB0011_QuestionReferencedByLinkedQuestionDoesNotExist,
                       this.CreateVerificationReferenceForQuestion(linkedQuestion));
        }

        private QuestionnaireVerificationError LinkedQuestionReferenceQuestionOfNotSupportedTypeError(IQuestion linkedQuestion,
            IQuestion sourceQuestion)
        {
            var references = new[]
            {
                this.CreateVerificationReferenceForQuestion(linkedQuestion),
                this.CreateVerificationReferenceForQuestion(sourceQuestion)
            };

            return new QuestionnaireVerificationError("WB0012",
                VerificationMessages.WB0012_LinkedQuestionReferenceQuestionOfNotSupportedType, references);
        }

        private QuestionnaireVerificationError LinkedQuestionReferenceQuestionNotUnderPropagatedGroup(IQuestion linkedQuestion,
            IQuestion sourceQuestion)
        {
            var references = new[]
            {
                this.CreateVerificationReferenceForQuestion(linkedQuestion),
                this.CreateVerificationReferenceForQuestion(sourceQuestion)
            };
            return new QuestionnaireVerificationError("WB0013",
                VerificationMessages.WB0013_LinkedQuestionReferenceQuestionNotUnderPropagatedGroup, references);
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
            params  IQuestion[] questions)
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

        private  IEnumerable<IGroup> GetAllParentGroupsForQuestion(IQuestion question, QuestionnaireDocument document)
        {
            return GetSpecifiedGroupAndAllItsParentGroupsStartingFromBottom((IGroup)question.GetParent(), document);
        }

        private IEnumerable<IGroup> GetSpecifiedGroupAndAllItsParentGroupsStartingFromBottom(IGroup group, QuestionnaireDocument document)
        {
            var parentGroups = new List<IGroup>();

            while (group != document)
            {
                parentGroups.Add(group);
                group = (IGroup)group.GetParent();
            }

            return parentGroups;
        }
    }
}