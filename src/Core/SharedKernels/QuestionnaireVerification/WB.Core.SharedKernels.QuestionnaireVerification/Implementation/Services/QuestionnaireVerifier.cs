 using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Complete;
using Main.Core.Entities.SubEntities.Question;
using Main.Core.Utility;
using WB.Core.SharedKernels.ExpressionProcessor.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.Properties;
using WB.Core.SharedKernels.QuestionnaireVerification.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;

namespace WB.Core.SharedKernels.QuestionnaireVerification.Implementation.Services
{
    internal class QuestionnaireVerifier : IQuestionnaireVerifier
    {
        #region Constants

        private static readonly IEnumerable<QuestionType> QuestionTypesValidToBeLinkedQuestionSource = new[]
        {
            QuestionType.DateTime,
            QuestionType.Numeric,
            QuestionType.Text,
        };

        private static readonly IEnumerable<QuestionType> QuestionTypesValidToBeSubstitutionReferences = new[]
        {
            QuestionType.DateTime,
            QuestionType.Numeric,
            QuestionType.SingleOption,
            QuestionType.Text,
            QuestionType.AutoPropagate
        };

        #endregion

        #region Types

        private struct EntityVerificationResult<TReferencedEntity>
            where TReferencedEntity : class, IComposite
        {
            public bool HasErrors { get; set; }
            public IEnumerable<TReferencedEntity> ReferencedEntities { get; set; }
        }

        #endregion

        private readonly IExpressionProcessor expressionProcessor;

        public QuestionnaireVerifier(IExpressionProcessor expressionProcessor)
        {
            this.expressionProcessor = expressionProcessor;
        }

        private IEnumerable<Func<QuestionnaireDocument, IEnumerable<QuestionnaireVerificationError>>> AtomicVerifiers
        {
            get
            {
                return new[]
                {
                    Verifier(NoQuestionsExist, "WB0001", VerificationMessages.WB0001_NoQuestions),
                    Verifier<IQuestion>(this.CustomValidationExpressionHasIncorrectSyntax, "WB0002", VerificationMessages.WB0002_CustomValidationExpressionHasIncorrectSyntax),
                    Verifier<IComposite>(this.CustomEnablementConditionHasIncorrectSyntax, "WB0003", VerificationMessages.WB0003_CustomEnablementConditionHasIncorrectSyntax),
                    Verifier<IQuestion>(this.CustomValidationExpressionReferencesNotExistingQuestion, "WB0004", VerificationMessages.WB0004_CustomValidationExpressionReferencesNotExistingQuestion),
                    Verifier<IComposite>(this.CustomEnablementConditionReferencesNotExistingQuestion, "WB0005", VerificationMessages.WB0005_CustomEnablementConditionReferencesNotExistingQuestion),
                    Verifier<IAutoPropagateQuestion>(PropagatingQuestionReferencesNotExistingGroup, "WB0006", VerificationMessages.WB0006_PropagatingQuestionReferencesNotExistingGroup),
                    Verifier<IAutoPropagateQuestion, IComposite>(PropagatingQuestionReferencesNotPropagatableGroup, "WB0007", VerificationMessages.WB0007_PropagatingQuestionReferencesNotPropagatableGroup),
                    Verifier<IAutoPropagateQuestion>(PropagatingQuestionHasNoAssociatedGroups, "WB0008", VerificationMessages.WB0008_PropagatingQuestionHasNoAssociatedGroups),
                    Verifier<IGroup>(PropagatedGroupHasNoPropagatingQuestionsPointingToIt, "WB0009", VerificationMessages.WB0009_PropagatedGroupHasNoPropagatingQuestionsPointingToIt),

                    ErrorsByPropagatedGroupsThatHasMoreThanOnePropagatingQuestionPointingToIt,
                    ErrorsByQuestionsWithCustomValidationReferencingQuestionsWithDeeperPropagationLevel,
                    ErrorsByLinkedQuestions,
                    ErrorsByQuestionsWithSubstitutions,

                    Verifier<IMultyOptionsQuestion>(CategoricalMultianswerQuestionHasIncorrectMaxAnswerCount, "WB0021", VerificationMessages.WB0021_CategoricalMultianswerQuestionHasIncorrectMaxAnswerCount),
                };
            }
        }

        public IEnumerable<QuestionnaireVerificationError> Verify(QuestionnaireDocument questionnaire)
        {
            questionnaire.ConnectChildrenWithParent();

            return
                from verifier in this.AtomicVerifiers
                let errors = verifier.Invoke(questionnaire)
                from error in errors
                select error;
        }

        private static Func<QuestionnaireDocument, IEnumerable<QuestionnaireVerificationError>> Verifier(
            Func<QuestionnaireDocument, bool> hasError, string code, string message)
        {
            return questionnaire =>
                hasError(questionnaire)
                    ? new[] { new QuestionnaireVerificationError(code, message) }
                    : Enumerable.Empty<QuestionnaireVerificationError>();
        }

        private static Func<QuestionnaireDocument, IEnumerable<QuestionnaireVerificationError>> Verifier<TEntity>(
            Func<TEntity, bool> hasError, string code, string message)
            where TEntity : class, IComposite
        {
            return questionnaire =>
                questionnaire
                    .Find<TEntity>(hasError)
                    .Select(entity => new QuestionnaireVerificationError(code, message, CreateReference(entity)));
        }

        private static Func<QuestionnaireDocument, IEnumerable<QuestionnaireVerificationError>> Verifier<TEntity>(
            Func<TEntity, QuestionnaireDocument, bool> hasError, string code, string message)
            where TEntity : class, IComposite
        {
            return questionnaire =>
                questionnaire
                    .Find<TEntity>(entity => hasError(entity, questionnaire))
                    .Select(entity => new QuestionnaireVerificationError(code, message, CreateReference(entity)));
        }

        private static Func<QuestionnaireDocument, IEnumerable<QuestionnaireVerificationError>> Verifier<TEntity, TReferencedEntity>(
            Func<TEntity, QuestionnaireDocument, EntityVerificationResult<TReferencedEntity>> verifyEntity, string code, string message)
            where TEntity : class, IComposite
            where TReferencedEntity : class, IComposite
        {
            return questionnaire =>
                from entity in questionnaire.Find<TEntity>(_ => true)
                let verificationResult = verifyEntity(entity, questionnaire)
                where verificationResult.HasErrors
                select new QuestionnaireVerificationError(code, message, verificationResult.ReferencedEntities.Select(CreateReference));
        }

        private static bool PropagatingQuestionHasNoAssociatedGroups(IAutoPropagateQuestion question, QuestionnaireDocument questionnaire)
        {
            return question.Triggers.Count == 0;
        }

        private static bool CategoricalMultianswerQuestionHasIncorrectMaxAnswerCount(IMultyOptionsQuestion question, QuestionnaireDocument questionnaire)
        {
            return question.MaxAllowedAnswers.HasValue
                &&
                (question.MaxAllowedAnswers.Value < 1 ||
                    (!question.LinkedToQuestionId.HasValue && question.MaxAllowedAnswers.Value > question.Answers.Count));
        }

        private static bool PropagatedGroupHasNoPropagatingQuestionsPointingToIt(IGroup group, QuestionnaireDocument questionnaire)
        {
            return IsGroupPropagatable(group) &&
                !GetPropagatingQuestionsPointingToPropagatedGroup(@group.PublicKey, questionnaire).Any();
        }

        private static IEnumerable<QuestionnaireVerificationError> ErrorsByPropagatedGroupsThatHasMoreThanOnePropagatingQuestionPointingToIt(
            QuestionnaireDocument questionnaire)
        {
            IEnumerable<IGroup> propagatedGroups = questionnaire.Find<IGroup>(IsGroupPropagatable);
            foreach (var propagatedGroup in propagatedGroups)
            {
                var propagatingQuestionsPointingToPropagatedGroup =
                    GetPropagatingQuestionsPointingToPropagatedGroup(propagatedGroup.PublicKey, questionnaire);

                if (propagatingQuestionsPointingToPropagatedGroup.Count() > 1)
                    yield return PropagatedGroupHasMoreThanOnePropagatingQuestionPointingToIt(propagatedGroup, propagatingQuestionsPointingToPropagatedGroup);
            }
        }

        private static IEnumerable<QuestionnaireVerificationError> ErrorsByLinkedQuestions(QuestionnaireDocument questionnaire)
        {
            var linkedQuestions = questionnaire.Find<IQuestion>(
                question => question.LinkedToQuestionId.HasValue);

            foreach (var linkedQuestion in linkedQuestions)
            {
                var sourceQuestion = questionnaire.Find<IQuestion>(linkedQuestion.LinkedToQuestionId.Value);

                if (sourceQuestion == null)
                {
                    yield return LinkedQuestionReferencesNotExistingQuestion(linkedQuestion);
                    continue;
                }

                bool isSourceQuestionValidType = QuestionTypesValidToBeLinkedQuestionSource.Contains(sourceQuestion.QuestionType);
                if (!isSourceQuestionValidType)
                {
                    yield return LinkedQuestionReferencesQuestionOfNotSupportedType(linkedQuestion, sourceQuestion);
                    continue;
                }

                var isSourceQuestionInsidePropagatedGroup = GetAllParentGroupsForQuestion(sourceQuestion, questionnaire).Any(IsGroupPropagatable);
                if (!isSourceQuestionInsidePropagatedGroup)
                {
                    yield return LinkedQuestionReferenceQuestionNotUnderPropagatedGroup(linkedQuestion, sourceQuestion);
                }
            }
        }

        private static IEnumerable<QuestionnaireVerificationError> ErrorsByQuestionsWithSubstitutions(QuestionnaireDocument questionnaire)
        {
            IEnumerable<IQuestion> questionsWithSubstitutions =
                questionnaire.Find<IQuestion>(question => StringUtil.GetAllSubstitutionVariableNames(question.QuestionText).Length > 0);
            
            var errorByAllQuestionsWithSubstitutions = new List<QuestionnaireVerificationError>();

            foreach (var questionWithSubstitution in questionsWithSubstitutions)
            {
                if (questionWithSubstitution.Featured)
                {
                    errorByAllQuestionsWithSubstitutions.Add(QuestionWithTitleSubstitutionCantBePrefilled(questionWithSubstitution));
                    continue;
                }

                var substitutionReferences = StringUtil.GetAllSubstitutionVariableNames(questionWithSubstitution.QuestionText);

                List<Guid?> vectorOfAutopropagatedQuestionsForQuestionWithSubstitution =
                    GetAllAutopropagationQuestionsAsVector(questionWithSubstitution, questionnaire);

                VerifyEnumerableAndAccumulateErrorsToList(substitutionReferences, errorByAllQuestionsWithSubstitutions,
                    identifier => GetVerificationErrorBySubstitutionReferenceOrNull(
                        questionWithSubstitution, identifier, vectorOfAutopropagatedQuestionsForQuestionWithSubstitution, questionnaire));
            }

            return errorByAllQuestionsWithSubstitutions;
        }

        private IEnumerable<QuestionnaireVerificationError> ErrorsByQuestionsWithCustomValidationReferencingQuestionsWithDeeperPropagationLevel(
            QuestionnaireDocument questionnaire)
        {
            var questionsWithValidationExpression = questionnaire.Find<IQuestion>(q => !string.IsNullOrEmpty(q.ValidationExpression));

            var errorByAllQuestionsWithCustomValidation = new List<QuestionnaireVerificationError>();

            foreach (var questionWithValidationExpression in questionsWithValidationExpression)
            {
                IEnumerable<string> identifiersUsedInExpression =
                    this.expressionProcessor.GetIdentifiersUsedInExpression(questionWithValidationExpression.ValidationExpression);

                List<Guid?> vectorOfAutopropagatedQuestionsForQuestionWithCustomValidation =
                    GetAllAutopropagationQuestionsAsVector(questionWithValidationExpression, questionnaire);

                VerifyEnumerableAndAccumulateErrorsToList(identifiersUsedInExpression, errorByAllQuestionsWithCustomValidation,
                    identifier => GetVerificationErrorByCustomValidationReferenceOrNull(
                        questionWithValidationExpression, identifier, vectorOfAutopropagatedQuestionsForQuestionWithCustomValidation, questionnaire));
            }

            return errorByAllQuestionsWithCustomValidation;
        }

        private static QuestionnaireVerificationReference CreateReference(IComposite entity)
        {
            return new QuestionnaireVerificationReference(
                entity is IGroup ? QuestionnaireVerificationReferenceType.Group : QuestionnaireVerificationReferenceType.Question,
                entity.PublicKey);
        }

        private bool CustomValidationExpressionHasIncorrectSyntax(IQuestion question)
        {
            if (string.IsNullOrWhiteSpace(question.ValidationExpression))
                return false;

            return !this.expressionProcessor.IsSyntaxValid(question.ValidationExpression);
        }

        private bool CustomEnablementConditionHasIncorrectSyntax(IComposite entity)
        {
            string customEnablementCondition = GetCustomEnablementCondition(entity);

            if (string.IsNullOrWhiteSpace(customEnablementCondition))
                return false;

            return !this.expressionProcessor.IsSyntaxValid(customEnablementCondition);
        }

        private bool CustomValidationExpressionReferencesNotExistingQuestion(IQuestion question, QuestionnaireDocument questionnaire)
        {
            string validationExpression = question.ValidationExpression;

            if (string.IsNullOrWhiteSpace(validationExpression))
                return false;

            IEnumerable<string> identifiersUsedInExpression = this.expressionProcessor.GetIdentifiersUsedInExpression(validationExpression);

            return identifiersUsedInExpression.Any(identifier
                => IsGuid(identifier)
                && !QuestionnaireContainsQuestionCorrespondingToExpressionIdentifier(questionnaire, identifier));
        }

        private bool CustomEnablementConditionReferencesNotExistingQuestion(IComposite entity, QuestionnaireDocument questionnaire)
        {
            string enablementCondition = GetCustomEnablementCondition(entity);

            if (string.IsNullOrWhiteSpace(enablementCondition))
                return false;

            IEnumerable<string> identifiersUsedInExpression = this.expressionProcessor.GetIdentifiersUsedInExpression(enablementCondition);

            return identifiersUsedInExpression.Any(
                identifier => !QuestionnaireContainsQuestionCorrespondingToExpressionIdentifier(questionnaire, identifier));
        }

        private static bool PropagatingQuestionReferencesNotExistingGroup(IAutoPropagateQuestion question, QuestionnaireDocument questionnaire)
        {
            return question.Triggers.Any(groupId
                => !QuestionnaireContainsGroup(questionnaire, groupId));
        }

        private static EntityVerificationResult<IComposite> PropagatingQuestionReferencesNotPropagatableGroup(IAutoPropagateQuestion question, QuestionnaireDocument questionnaire)
        {
            IEnumerable<IGroup> referencedNotPropagatableGroups =
                question
                    .Triggers
                    .Select(questionnaire.Find<IGroup>)
                    .Where(group => group != null && group.Propagated == Propagate.None)
                    .ToList();

            return new EntityVerificationResult<IComposite>()
            {
                HasErrors = referencedNotPropagatableGroups.Any(),

                ReferencedEntities = Enumerable.Concat(
                    new[] { question },
                    referencedNotPropagatableGroups.AsEnumerable<IComposite>()),
            };
        }

        private static bool IsGuid(string identifier)
        {
            Guid _;
            return Guid.TryParse(identifier, out _);
        }

        private static bool QuestionnaireContainsQuestionCorrespondingToExpressionIdentifier(QuestionnaireDocument questionnaire, string identifier)
        {
            Guid questionId;
            if (!Guid.TryParse(identifier, out questionId))
                return false;

            return questionnaire.Find<IQuestion>(questionId) != null;
        }

        private static bool QuestionnaireContainsGroup(QuestionnaireDocument questionnaire, Guid groupId)
        {
            return questionnaire.Find<IGroup>(groupId) != null;
        }

        private static string GetCustomEnablementCondition(IComposite entity)
        {
            if (entity is IGroup)
                return ((IGroup)entity).ConditionExpression;
            else if (entity is IQuestion)
                return ((IQuestion)entity).ConditionExpression;
            else
                return null;
        }

        private static QuestionnaireVerificationError GetVerificationErrorByCustomValidationReferenceOrNull(
            IQuestion questionWithValidationExpression, string identifier,
            List<Guid?> vectorOfAutopropagatedQuestionsForQuestionWithCustomValidation, QuestionnaireDocument questionnaire)
        {
            if (IsSpecialThisIdentifier(identifier))
            {
                return null;
            }

            Guid parsedId;
            if (!Guid.TryParse(identifier, out parsedId))
            {
                return CustomValidationExpressionUsesNotRecognizedParameter(questionWithValidationExpression);
            }

            var questionsReferencedInValidation = questionnaire.FirstOrDefault<IQuestion>(q => q.PublicKey == parsedId);
            if (questionsReferencedInValidation == null)
            {
                return null;
            }

            if (QuestionHasDeeperPropagationLevelThenVectorOfAutopropagatedQuestions(questionsReferencedInValidation,
                vectorOfAutopropagatedQuestionsForQuestionWithCustomValidation, questionnaire))
            {
                return CustomValidationExpressionReferencesQuestionWithDeeperPropagationLevel(
                    questionWithValidationExpression, questionsReferencedInValidation);
            }

            return null;
        }

        private static QuestionnaireVerificationError GetVerificationErrorBySubstitutionReferenceOrNull(IQuestion questionWithSubstitution,
            string substitutionReference, List<Guid?> vectorOfAutopropagatedQuestionsByQuestionWithSubstitutions,
            QuestionnaireDocument questionnaire)
        {
            if (substitutionReference == questionWithSubstitution.StataExportCaption)
            {
                return QuestionWithTitleSubstitutionCantReferenceSelf(questionWithSubstitution);
            }

            var questionSourceOfSubstitution =
                questionnaire.FirstOrDefault<IQuestion>(q => q.StataExportCaption == substitutionReference);

            if (questionSourceOfSubstitution == null)
            {
                return QuestionWithTitleSubstitutionReferencesNotExistingQuestion(questionWithSubstitution);
            }

            if (!QuestionTypesValidToBeSubstitutionReferences.Contains(questionSourceOfSubstitution.QuestionType))
            {
                return
                    QuestionWithTitleSubstitutionReferencesQuestionOfNotSupportedType(questionWithSubstitution, questionSourceOfSubstitution);
            }

            if (QuestionHasDeeperPropagationLevelThenVectorOfAutopropagatedQuestions(questionSourceOfSubstitution,
                vectorOfAutopropagatedQuestionsByQuestionWithSubstitutions, questionnaire))
            {
                return
                    QuestionWithTitleSubstitutionCantReferenceQuestionsWithDeeperPropagationLevel(questionWithSubstitution,
                        questionSourceOfSubstitution);
            }

            return null;
        }

        private static QuestionnaireVerificationError CustomValidationExpressionUsesNotRecognizedParameter(IQuestion questionWithValidationExpression)
        {
            return new QuestionnaireVerificationError("WB0020",
                VerificationMessages.WB0020_CustomValidationExpressionUsesNotRecognizedParameter,
                CreateReference(questionWithValidationExpression));
        }

        private static QuestionnaireVerificationError CustomValidationExpressionReferencesQuestionWithDeeperPropagationLevel(
            IQuestion questionWithValidationExpression, IQuestion questionsReferencedInValidation)
        {
            return new QuestionnaireVerificationError("WB0014",
                VerificationMessages.WB0014_CustomValidationExpressionReferencesQuestionWithDeeperPropagationLevel,
                CreateReference(questionWithValidationExpression),
                CreateReference(questionsReferencedInValidation));

        }

        private static QuestionnaireVerificationError QuestionWithTitleSubstitutionCantBePrefilled(IQuestion questionsWithSubstitution)
        {
            return new QuestionnaireVerificationError("WB0015",
                VerificationMessages.WB0015_QuestionWithTitleSubstitutionCantBePrefilled,
                CreateReference(questionsWithSubstitution));
        }

        private static QuestionnaireVerificationError PropagatedGroupHasMoreThanOnePropagatingQuestionPointingToIt(IGroup propagatedGroup, IEnumerable<IQuestion> propagatingQuestionsPointingToPropagatedGroup)
        {
            return new QuestionnaireVerificationError("WB0010",
                VerificationMessages.WB0010_PropagatedGroupHasMoreThanOnePropagatingQuestionPointingToIt,
                Enumerable.Concat(
                    new[] { CreateReference(propagatedGroup) },
                    propagatingQuestionsPointingToPropagatedGroup.Select(CreateReference)));
        }

        private static QuestionnaireVerificationError QuestionWithTitleSubstitutionCantReferenceQuestionsWithDeeperPropagationLevel(IQuestion questionsWithSubstitution,IQuestion questionSourceOfSubstitution)
        {
            return new QuestionnaireVerificationError("WB0019",
                VerificationMessages.WB0019_QuestionWithTitleSubstitutionCantReferenceQuestionsWithDeeperPropagationLevel,
                CreateReference(questionsWithSubstitution),
                CreateReference(questionSourceOfSubstitution));
        }

        private static QuestionnaireVerificationError QuestionWithTitleSubstitutionReferencesQuestionOfNotSupportedType(IQuestion questionsWithSubstitution,IQuestion questionSourceOfSubstitution)
        {
            return new QuestionnaireVerificationError("WB0018",
                VerificationMessages.WB0018_QuestionWithTitleSubstitutionReferencesQuestionOfNotSupportedType,
                CreateReference(questionsWithSubstitution),
                CreateReference(questionSourceOfSubstitution));
        }

        private static QuestionnaireVerificationError QuestionWithTitleSubstitutionReferencesNotExistingQuestion(IQuestion questionsWithSubstitution)
        {
            return new QuestionnaireVerificationError("WB0017",
                VerificationMessages.WB0017_QuestionWithTitleSubstitutionReferencesNotExistingQuestion,
                CreateReference(questionsWithSubstitution));
        }

        private static QuestionnaireVerificationError QuestionWithTitleSubstitutionCantReferenceSelf(IQuestion questionsWithSubstitution)
        {
            return new QuestionnaireVerificationError("WB0016",
                VerificationMessages.WB0016_QuestionWithTitleSubstitutionCantReferenceSelf,
                CreateReference(questionsWithSubstitution));
        }

        private static QuestionnaireVerificationError LinkedQuestionReferencesNotExistingQuestion(IQuestion linkedQuestion)
        {
            return new QuestionnaireVerificationError("WB0011",
                VerificationMessages.WB0011_LinkedQuestionReferencesNotExistingQuestion,
                CreateReference(linkedQuestion));
        }

        private static QuestionnaireVerificationError LinkedQuestionReferencesQuestionOfNotSupportedType(IQuestion linkedQuestion,IQuestion sourceQuestion)
        {
            return new QuestionnaireVerificationError("WB0012",
                VerificationMessages.WB0012_LinkedQuestionReferencesQuestionOfNotSupportedType,
                CreateReference(linkedQuestion),
                CreateReference(sourceQuestion));
        }

        private static QuestionnaireVerificationError LinkedQuestionReferenceQuestionNotUnderPropagatedGroup(IQuestion linkedQuestion,IQuestion sourceQuestion)
        {
            return new QuestionnaireVerificationError("WB0013",
                VerificationMessages.WB0013_LinkedQuestionReferencesQuestionNotUnderPropagatedGroup,
                CreateReference(linkedQuestion),
                CreateReference(sourceQuestion));
        }

        private static void VerifyEnumerableAndAccumulateErrorsToList<T>(IEnumerable<T> enumerableToVerify,
            List<QuestionnaireVerificationError> errorList, Func<T, QuestionnaireVerificationError> getErrorOrNull)
        {
            errorList.AddRange(
                enumerableToVerify
                    .Select(getErrorOrNull)
                    .Where(errorOrNull => errorOrNull != null));
        }

        private static bool NoQuestionsExist(QuestionnaireDocument questionnaire)
        {
            return !questionnaire.Find<IQuestion>(_ => true).Any();
        }

        private static bool IsGroupPropagatable(IGroup group)
        {
            return group.Propagated == Propagate.AutoPropagated;
        }

        private static IEnumerable<IQuestion> GetPropagatingQuestionsPointingToPropagatedGroup(Guid groupId, QuestionnaireDocument document)
        {
            return document.Find<IAutoPropagateQuestion>(question => question.Triggers.Contains(groupId));
        }

        private static IEnumerable<IGroup> GetAllParentGroupsForQuestion(IQuestion question, QuestionnaireDocument document)
        {
            return GetSpecifiedGroupAndAllItsParentGroupsStartingFromBottom((IGroup)question.GetParent(), document);
        }

        private static List<Guid?> GetAllAutopropagationQuestionsAsVector(IQuestion question, QuestionnaireDocument questionnaire)
        {
            List<Guid?> propagationQuestions =
                GetSpecifiedGroupAndAllItsParentGroupsStartingFromBottom((IGroup) question.GetParent(), questionnaire)
                    .Where(IsGroupPropagatable)
                    .Select<IGroup, Guid?>(g =>
                    {
                        var propagationTriggers = GetPropagatingQuestionsPointingToPropagatedGroup(g.PublicKey, questionnaire);
                        var firstTrigger = propagationTriggers.FirstOrDefault();
                        if (firstTrigger == null)
                            return null;
                        return firstTrigger.PublicKey;
                    })

                    .ToList();

            return propagationQuestions;
        }

        private static IEnumerable<IGroup> GetSpecifiedGroupAndAllItsParentGroupsStartingFromBottom(IGroup group, QuestionnaireDocument document)
        {
            var parentGroups = new List<IGroup>();

            while (group != document)
            {
                parentGroups.Add(group);
                group = (IGroup)group.GetParent();
            }

            return parentGroups;
        }

        private static bool QuestionHasDeeperPropagationLevelThenVectorOfAutopropagatedQuestions(IQuestion question,
            List<Guid?> vectorOfAutopropagatedQuestions, QuestionnaireDocument questionnaire)
        {
            List<Guid?> autopropagationQuestionsAsVectorForQuestionSourceOfSubstitution =
                GetAllAutopropagationQuestionsAsVector(question, questionnaire);
         
            return autopropagationQuestionsAsVectorForQuestionSourceOfSubstitution.Count > 0
                &&
                autopropagationQuestionsAsVectorForQuestionSourceOfSubstitution.Except(
                    vectorOfAutopropagatedQuestions, new NullableGuidsEqualityComparer()).Any();
        }

        private static bool IsSpecialThisIdentifier(string identifier)
        {
            return identifier.ToLower() == "this";
        }

        protected class NullableGuidsEqualityComparer : IEqualityComparer<Guid?>
        {
            public bool Equals(Guid? x, Guid? y)
            {
                if (!x.HasValue)
                    return false;
                if (!y.HasValue)
                    return false;
                return x.Value == y.Value;
            }

            public int GetHashCode(Guid? obj)
            {
                return obj.GetHashCode();
            }
        }

    }
}