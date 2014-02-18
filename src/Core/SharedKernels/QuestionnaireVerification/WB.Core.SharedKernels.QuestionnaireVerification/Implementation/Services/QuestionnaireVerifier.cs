using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
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
            QuestionType.Text
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
                    Verifier<IGroup>(GroupWhereRosterSizeSourceIsQuestionHasNoRosterSizeQuestion, "WB0009", VerificationMessages.WB0009_GroupWhereRosterSizeSourceIsQuestionHasNoRosterSizeQuestion),
                    Verifier<IMultyOptionsQuestion>(CategoricalMultianswerQuestionHasIncorrectMaxAnswerCount, "WB0021", VerificationMessages.WB0021_CategoricalMultianswerQuestionHasIncorrectMaxAnswerCount),
                    Verifier<IMultyOptionsQuestion>(this.CategoricalMultianswerQuestionIsFeatured, "WB0022",VerificationMessages.WB0022_PrefilledQuestionsOfIllegalType),
                    Verifier<IGroup>(RosterSizeSourceQuestionTypeIsIncorrect, "WB0023", VerificationMessages.WB0023_RosterSizeSourceQuestionTypeIsIncorrect),
                    Verifier<IQuestion>(RosterSizeQuestionCannotBeInsideAnyRosterGroup, "WB0024", VerificationMessages.WB0024_RosterSizeQuestionCannotBeInsideAnyRosterGroup),
                    Verifier<IQuestion>(RosterSizeQuestionMaxValueCouldNotBeEmpty, "WB0025", VerificationMessages.WB0025_RosterSizeQuestionMaxValueCouldNotBeEmpty),
                    Verifier<IQuestion>(RosterSizeQuestionMaxValueCouldBeInRange1And40, "WB0026", VerificationMessages.WB0026_RosterSizeQuestionMaxValueCouldBeInRange1And40),
                    Verifier<IGroup>(QuestionnaireHaveAutopropagatedGroups, "WB0027", VerificationMessages.WB0027_QuestionnaireHaveAutopropagatedGroups),
                    Verifier<IQuestion>(QuestionnaireHaveAutopropagatedQuestions, "WB0028", VerificationMessages.WB0028_QuestionnaireHaveAutopropagatedQuestions),
                    Verifier<IGroup>(RosterHasRosterInsideItself, "WB0029", VerificationMessages.WB0029_RosterHasRoster),
                    Verifier<IQuestion>(PrefilledQuestionCantBeInsideOfRoster, "WB0030", VerificationMessages.WB0030_PrefilledQuestionCantBeInsideOfRoster),
                    Verifier<IGroup>(GroupWhereRosterSizeSourceIsQuestionHaveFixedTitles, "WB0032", VerificationMessages.WB0032_GroupWhereRosterSizeSourceIsQuestionHaveFixedTitles),
                    Verifier<IGroup>(GroupWhereRosterSizeSourceIsFixedTitlesHaveRosterSizeQuestion, "WB0033", VerificationMessages.WB0033_GroupWhereRosterSizeSourceIsFixedTitlesHaveRosterSizeQuestion),
                    Verifier<IGroup>(GroupWhereRosterSizeSourceIsFixedTitlesHaveRosterTitleQuestion, "WB0034", VerificationMessages.WB0034_GroupWhereRosterSizeSourceIsFixedTitlesHaveRosterTitleQuestion),
                    Verifier<IGroup>(GroupWhereRosterSizeSourceIsQuestionHasInvalidRosterTitleQuestion, "WB0035", VerificationMessages.WB0035_GroupWhereRosterSizeSourceIsQuestionHasInvalidRosterTitleQuestion),
                    Verifier<IGroup>(GroupWhereRosterSizeIsCategoricalMultyAnswerQuestionHaveRosterTitleQuestion, "WB0036", VerificationMessages.WB0036_GroupWhereRosterSizeIsCategoricalMultyAnswerQuestionHaveRosterTitleQuestion),
                    Verifier<IGroup>(GroupWhereRosterSizeSourceIsFixedTitlesHaveEmptyTitles, "WB0037", VerificationMessages.WB0037_GroupWhereRosterSizeSourceIsFixedTitlesHaveEmptyTitles),
                    Verifier<IGroup>(RosterFixedTitlesHaveMoreThan250Items, "WB0038", VerificationMessages.WB0038_RosterFixedTitlesHaveMoreThan250Items),
                    Verifier<ITextListQuestion>(TextListQuestionCannotBePrefilled, "WB0039",VerificationMessages.WB0039_TextListQuestionCannotBePrefilled),
                    Verifier<ITextListQuestion>(TextListQuestionCannotBeFilledBySupervisor, "WB0040",VerificationMessages.WB0040_TextListQuestionCannotBeFilledBySupervisor),
                    Verifier<ITextListQuestion>(TextListQuestionCannotHaveCustomValidation, "WB0041",VerificationMessages.WB0041_TextListQuestionCannotCustomValidation),
                    Verifier<ITextListQuestion>(TextListQuestionMaxAnswerNotInRange1And40, "WB0042",VerificationMessages.WB0042_TextListQuestionMaxAnswerInRange1And40),
                    Verifier<IQuestion>(QuestionHasOptionsWithEmptyValue, "WB0045",VerificationMessages.WB0045_QuestionHasOptionsWithEmptyValue),

                    this.ErrorsByQuestionsWithCustomValidationReferencingQuestionsWithDeeperRosterLevel,
                    this.ErrorsByQuestionsWithCustomConditionReferencingQuestionsWithDeeperRosterLevel,
                    ErrorsByEpressionsThatUsesTextListQuestions,
                    ErrorsByLinkedQuestions,
                    ErrorsByQuestionsWithSubstitutions,
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

        private static bool CategoricalMultianswerQuestionHasIncorrectMaxAnswerCount(IMultyOptionsQuestion question, QuestionnaireDocument questionnaire)
        {
            return question.MaxAllowedAnswers.HasValue
                &&
                (question.MaxAllowedAnswers.Value < 1 ||
                    (!question.LinkedToQuestionId.HasValue && question.MaxAllowedAnswers.Value > question.Answers.Count));
        }

        private static bool GroupWhereRosterSizeSourceIsQuestionHasNoRosterSizeQuestion(IGroup group, QuestionnaireDocument questionnaire)
        {
            return IsRosterByQuestion(group) && GetRosterSizeQuestionByRosterGroup(group, questionnaire) == null;
        }

        private static bool RosterSizeSourceQuestionTypeIsIncorrect(IGroup group, QuestionnaireDocument questionnaire)
        {
            var rosterSizeQuestion = GetRosterSizeQuestionByRosterGroup(group, questionnaire);
            if (rosterSizeQuestion == null)
                return false;

            return !IsQuestionAllowedToBeRosterSizeSource(rosterSizeQuestion);
        }

        private static bool GroupWhereRosterSizeSourceIsQuestionHaveFixedTitles(IGroup group)
        {
            return IsRosterByQuestion(group) && group.RosterFixedTitles != null && group.RosterFixedTitles.Any();
        }

        private static bool GroupWhereRosterSizeSourceIsFixedTitlesHaveRosterSizeQuestion(IGroup group)
        {
            return IsRosterByFixedTitles(group) && group.RosterSizeQuestionId.HasValue;
        }

        private static bool GroupWhereRosterSizeSourceIsFixedTitlesHaveRosterTitleQuestion(IGroup group)
        {
            return IsRosterByFixedTitles(group) && group.RosterTitleQuestionId.HasValue;
        }

        private static bool GroupWhereRosterSizeSourceIsQuestionHasInvalidRosterTitleQuestion(IGroup group, QuestionnaireDocument questionnaire)
        {
            if (!IsRosterByQuestion(group))
                return false;
            if (!IsNumericRosterSizeQuestion(GetRosterSizeQuestionByRosterGroup(group, questionnaire)))
                return false;
            if (!group.RosterTitleQuestionId.HasValue)
                return false;

            var rosterTitleQuestion = questionnaire.FirstOrDefault<IQuestion>(x => x.PublicKey == group.RosterTitleQuestionId.Value);
            if (rosterTitleQuestion == null)
                return true;

            if (!GetAllParentGroupsForQuestion(rosterTitleQuestion, questionnaire).Any(IsRosterGroup))
                return true;

            var rostersByRosterTitleQuestion =
                questionnaire.Find<IGroup>(
                    g => g.RosterTitleQuestionId.HasValue && (g.RosterTitleQuestionId.Value == group.RosterTitleQuestionId.Value));

            return rostersByRosterTitleQuestion.Any(g =>g.RosterSizeQuestionId.HasValue && (g.RosterSizeQuestionId.Value != group.RosterSizeQuestionId.Value));
        }

        private static bool GroupWhereRosterSizeIsCategoricalMultyAnswerQuestionHaveRosterTitleQuestion(IGroup group, QuestionnaireDocument questionnaire)
        {
            if (!IsRosterByQuestion(group))
                return false;
            if (!IsCategoricalRosterSizeQuestion(GetRosterSizeQuestionByRosterGroup(group, questionnaire)))
                return false;
            return group.RosterTitleQuestionId.HasValue;
        }

        private static bool GroupWhereRosterSizeSourceIsFixedTitlesHaveEmptyTitles(IGroup group)
        {
            if (!IsRosterByFixedTitles(group))
                return false;
            if (group.RosterFixedTitles == null)
                return false;
            if (group.RosterFixedTitles.Length == 0)
                return false;

            return group.RosterFixedTitles.Any(string.IsNullOrWhiteSpace);
        }

        private static bool RosterFixedTitlesHaveMoreThan250Items(IGroup group)
        {
            if (!IsRosterByFixedTitles(group))
                return false;

            return group.RosterFixedTitles.Length > 250;
        }

        private static bool RosterSizeQuestionCannotBeInsideAnyRosterGroup(IQuestion question, QuestionnaireDocument questionnaire)
        {
            return IsRosterSizeQuestion(question, questionnaire) && GetAllParentGroupsForQuestion(question, questionnaire).Any(IsRosterGroup);
        }

        private static bool RosterSizeQuestionMaxValueCouldNotBeEmpty(IQuestion question, QuestionnaireDocument questionnaire)
        {
            return IsRosterSizeQuestion(question, questionnaire)
                && IsQuestionAllowedToBeRosterSizeSource(question)
                && IsMaxValueMissing(question);
        }

        private static bool RosterSizeQuestionMaxValueCouldBeInRange1And40(IQuestion question, QuestionnaireDocument questionnaire)
        {
            if (!IsRosterSizeQuestion(question, questionnaire))
                return false;
            if (!IsQuestionAllowedToBeRosterSizeSource(question))
                return false;
            var rosterSizeQuestionMaxValue = GetRosterSizeQuestionMaxValue(question);
            if (!rosterSizeQuestionMaxValue.HasValue)
                return false;
            return !Enumerable.Range(1, 40).Contains(rosterSizeQuestionMaxValue.Value);
        }

        private static bool IsQuestionAllowedToBeRosterSizeSource(IQuestion question)
        {
            return IsNumericRosterSizeQuestion(question)
                || IsCategoricalRosterSizeQuestion(question)
                || IsTextListQuestion(question);
        }

        private static bool IsTextListQuestion(IQuestion question)
        {
            return question.QuestionType == QuestionType.TextList;
        }

        private static bool IsNumericRosterSizeQuestion(IQuestion question)
        {
            var numericQuestion = question as NumericQuestion;
            return numericQuestion != null && numericQuestion.IsInteger;
        }

        private static bool IsCategoricalRosterSizeQuestion(IQuestion question)
        {
            var multiOptionQuestion = question as MultyOptionsQuestion;
            return multiOptionQuestion != null && !multiOptionQuestion.LinkedToQuestionId.HasValue;
        }

        private static bool PrefilledQuestionCantBeInsideOfRoster(IQuestion question, QuestionnaireDocument questionnaire)
        {
            return question.Featured && GetAllParentGroupsForQuestion(question, questionnaire).Any(IsRosterGroup);
        }

        private static bool RosterHasRosterInsideItself(IGroup group, QuestionnaireDocument questionnaire)
        {
            if (!IsRosterGroup(group))
                return false;
            
            foreach (var nestedGroup in group.Children.OfType<IGroup>())
            {
                if (HasRosterInsideGroup(nestedGroup))
                    return true;
            }

            return false;
        }

        private static bool HasRosterInsideGroup(IGroup group)
        {
            if (IsRosterGroup(group))
                return true;

            foreach (var nestedGroup in group.Children.OfType<IGroup>())
            {
                if (HasRosterInsideGroup(nestedGroup))
                    return true;
            }
            return false;
        }

        private static bool QuestionnaireHaveAutopropagatedQuestions(IQuestion question, QuestionnaireDocument questionnaire)
        {
            return questionnaire.Find<IAutoPropagateQuestion>(_ => true).Any();
        }

        private static bool QuestionnaireHaveAutopropagatedGroups(IGroup group, QuestionnaireDocument questionnaire)
        {
            return questionnaire.Find<IGroup>(IsGroupPropagatable).Any();
        }

        private static bool TextListQuestionCannotBePrefilled(ITextListQuestion question)
        {
            return question.Featured;
        }

        private static bool TextListQuestionMaxAnswerNotInRange1And40(ITextListQuestion question)
        {
            if (!question.MaxAnswerCount.HasValue)
                return false;
            return !Enumerable.Range(1, TextListQuestion.MaxAnswerCountLimit).Contains(question.MaxAnswerCount.Value);
        }

        private static bool QuestionHasOptionsWithEmptyValue(IQuestion question)
        {
            if (!(question is SingleQuestion || question is IMultyOptionsQuestion))
                return false;

            if (question.LinkedToQuestionId.HasValue)
                return false;

            return question.Answers.Any(option => string.IsNullOrEmpty(option.AnswerValue));
        }

        private static bool TextListQuestionCannotHaveCustomValidation(ITextListQuestion question)
        {
            return !string.IsNullOrWhiteSpace(question.ValidationExpression);
        }

        private static bool TextListQuestionCannotBeFilledBySupervisor(ITextListQuestion question)
        {
            return question.QuestionScope == QuestionScope.Supervisor;
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

                var isSourceQuestionInsideRosterGroup = GetAllParentGroupsForQuestion(sourceQuestion, questionnaire).Any(IsRosterGroup);
                if (!isSourceQuestionInsideRosterGroup)
                {
                    yield return LinkedQuestionReferenceQuestionNotUnderRosterGroup(linkedQuestion, sourceQuestion);
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

                Guid[] vectorOfRosterSizeQuestionsForQuestionWithSubstitution =
                    GetAllRosterSizeQuestionsAsVectorOrNullIfSomeAreMissing(questionWithSubstitution, questionnaire);

                if (vectorOfRosterSizeQuestionsForQuestionWithSubstitution != null)
                {
                    VerifyEnumerableAndAccumulateErrorsToList(substitutionReferences, errorByAllQuestionsWithSubstitutions,
                        identifier => GetVerificationErrorBySubstitutionReferenceOrNull(
                            questionWithSubstitution, identifier, vectorOfRosterSizeQuestionsForQuestionWithSubstitution, questionnaire));
                }
            }

            return errorByAllQuestionsWithSubstitutions;
        }

        private IEnumerable<QuestionnaireVerificationError> ErrorsByQuestionsWithCustomValidationReferencingQuestionsWithDeeperRosterLevel(
            QuestionnaireDocument questionnaire)
        {
            var questionsWithValidationExpression = questionnaire.Find<IQuestion>(q => !string.IsNullOrEmpty(q.ValidationExpression));

            var errorByAllQuestionsWithCustomValidation = new List<QuestionnaireVerificationError>();

            foreach (var questionWithValidationExpression in questionsWithValidationExpression)
            {
                IEnumerable<string> identifiersUsedInExpression =
                    this.expressionProcessor.GetIdentifiersUsedInExpression(questionWithValidationExpression.ValidationExpression);

                Guid[] vectorOfRosterSizeQuestionsForQuestionWithCustomValidation =
                    GetAllRosterSizeQuestionsAsVectorOrNullIfSomeAreMissing(questionWithValidationExpression, questionnaire);

                if (vectorOfRosterSizeQuestionsForQuestionWithCustomValidation != null)
                {
                    VerifyEnumerableAndAccumulateErrorsToList(identifiersUsedInExpression, errorByAllQuestionsWithCustomValidation,
                        identifier => GetVerificationErrorByCustomExpressionReferenceOrNull(
                            questionWithValidationExpression, identifier, vectorOfRosterSizeQuestionsForQuestionWithCustomValidation,
                            questionnaire, 
                            CustomValidationExpressionUsesNotRecognizedParameter,
                            CustomValidationExpressionReferencesQuestionWithDeeperPropagationLevel));
                }
            }

            return errorByAllQuestionsWithCustomValidation;
        }

        private IEnumerable<QuestionnaireVerificationError> ErrorsByQuestionsWithCustomConditionReferencingQuestionsWithDeeperRosterLevel(
    QuestionnaireDocument questionnaire)
        {
            var itemsWithConditionExpression = questionnaire.Find<IComposite>(q => !string.IsNullOrEmpty(GetCustomEnablementCondition(q)));

            var errorByAllItemsWithCustomCondition = new List<QuestionnaireVerificationError>();

            foreach (var itemWithConditionExpression in itemsWithConditionExpression)
            {
                IEnumerable<string> identifiersUsedInExpression =
                    this.expressionProcessor.GetIdentifiersUsedInExpression(GetCustomEnablementCondition(itemWithConditionExpression));

                Guid[] vectorOfRosterSizeQuestionsForQuestionWithCustomCondition =
                    GetAllRosterSizeQuestionsAsVectorOrNullIfSomeAreMissing(itemWithConditionExpression, questionnaire);

                if (vectorOfRosterSizeQuestionsForQuestionWithCustomCondition != null)
                {
                    VerifyEnumerableAndAccumulateErrorsToList(identifiersUsedInExpression, errorByAllItemsWithCustomCondition,
                        identifier => GetVerificationErrorByCustomExpressionReferenceOrNull(
                            itemWithConditionExpression, identifier, vectorOfRosterSizeQuestionsForQuestionWithCustomCondition,
                            questionnaire,
                            CustomConditionExpressionUsesNotRecognizedParameter,
                            CustomConditionExpressionReferencesQuestionWithDeeperPropagationLevel));
                }
            }

            return errorByAllItemsWithCustomCondition;
        }

        private IEnumerable<QuestionnaireVerificationError> ErrorsByEpressionsThatUsesTextListQuestions(QuestionnaireDocument questionnaire)
        {
            var errors = new List<QuestionnaireVerificationError>();
            errors.AddRange(this.ErrorsByGroupsOrQuestionsWithCustomExpression<IQuestion>(questionnaire, q => q.ValidationExpression, CustomValidationExpressionUsesTextListQuestion));
            errors.AddRange(this.ErrorsByGroupsOrQuestionsWithCustomExpression<IQuestion>(questionnaire, q => q.ConditionExpression, CustomConditionExpressionUsesTextListQuestion));
            errors.AddRange(this.ErrorsByGroupsOrQuestionsWithCustomExpression<IGroup>(questionnaire, g => g.ConditionExpression, CustomConditionExpressionUsesTextListQuestion));
            return errors;
        }

        private IEnumerable<QuestionnaireVerificationError> ErrorsByGroupsOrQuestionsWithCustomExpression<T>(QuestionnaireDocument questionnaire, Func<T, string> getExpression, Func<T, QuestionnaireVerificationError> getCustomError) where T : class
        {
            var itemsWithValidationExpression = questionnaire.Find<T>(q => !string.IsNullOrEmpty(getExpression(q)));

            var errorByAllQuestionsOrGroupsWithCustomValidation = new List<QuestionnaireVerificationError>();

            foreach (var itemWithValidationExpression in itemsWithValidationExpression)
            {
                IEnumerable<string> identifiersUsedInExpression =
                    this.expressionProcessor.GetIdentifiersUsedInExpression(getExpression(itemWithValidationExpression));

                VerifyEnumerableAndAccumulateErrorsToList(identifiersUsedInExpression, errorByAllQuestionsOrGroupsWithCustomValidation,
                    identifier => GetVerificationErrorByEpressionUsesTextListQuestion<T>(itemWithValidationExpression, identifier, questionnaire, getCustomError));
            }
            return errorByAllQuestionsOrGroupsWithCustomValidation;
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

        private bool CategoricalMultianswerQuestionIsFeatured(IMultyOptionsQuestion question, QuestionnaireDocument questionnaire)
        {
            return question.Featured;
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

        private static QuestionnaireVerificationError GetVerificationErrorByEpressionUsesTextListQuestion<T>(
            T questionnaireItemWithExpression,
            string identifier,
            QuestionnaireDocument questionnaire,
            Func<T, QuestionnaireVerificationError> getCustomError)
        {
            if (IsSpecialThisIdentifier(identifier))
            {
                return null;
            }

            IQuestion questionsReferencedInValidation = GetQuestionByIdentifier(identifier, questionnaire);

            if (questionsReferencedInValidation is ITextListQuestion)
            {
                return getCustomError(questionnaireItemWithExpression);
            }

            return null;
        }

        private static QuestionnaireVerificationError GetVerificationErrorByCustomExpressionReferenceOrNull(
            IComposite itemWithExpression, string identifier,
            Guid[] vectorOfRosterQuestionsForQuestionWithExpression, QuestionnaireDocument questionnaire,
            Func<IComposite, QuestionnaireVerificationError> notRecognizedParameterError,
            Func<IComposite, IQuestion, QuestionnaireVerificationError> referencesQuestionWithDeeperPropagationLevelError)
        {
            if (IsSpecialThisIdentifier(identifier))
            {
                return null;
            }

            IQuestion questionsReferencedInExpression = GetQuestionByIdentifier(identifier, questionnaire);

            if (questionsReferencedInExpression == null)
            {
                return notRecognizedParameterError(itemWithExpression);
            }

            if (QuestionHasDeeperRosterLevelThenVectorOfRosterQuestions(questionsReferencedInExpression, vectorOfRosterQuestionsForQuestionWithExpression, questionnaire))
            {
                return referencesQuestionWithDeeperPropagationLevelError(
                    itemWithExpression, questionsReferencedInExpression);
            }

            return null;
        }

        private static IQuestion GetQuestionByIdentifier(string identifier, QuestionnaireDocument questionnaire)
        {
            Guid parsedId;

            return Guid.TryParse(identifier, out parsedId)
                ? questionnaire.FirstOrDefault<IQuestion>(q => q.PublicKey == parsedId)
                : questionnaire.FirstOrDefault<IQuestion>(q => q.StataExportCaption == identifier);
        }

        private static QuestionnaireVerificationError GetVerificationErrorBySubstitutionReferenceOrNull(IQuestion questionWithSubstitution,
            string substitutionReference, Guid[] vectorOfAutopropagatedQuestionsByQuestionWithSubstitutions,
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

            if (QuestionHasDeeperRosterLevelThenVectorOfRosterQuestions(questionSourceOfSubstitution,
                vectorOfAutopropagatedQuestionsByQuestionWithSubstitutions, questionnaire))
            {
                return
                    QuestionWithTitleSubstitutionCantReferenceQuestionsWithDeeperPropagationLevel(questionWithSubstitution,
                        questionSourceOfSubstitution);
            }

            return null;
        }

        private static QuestionnaireVerificationError CustomValidationExpressionUsesTextListQuestion(IQuestion questionWithValidationExpression)
        {
            return new QuestionnaireVerificationError("WB0043",
                VerificationMessages.WB0043_TextListQuestionCannotBeUsedInValidationExpressions,
                CreateReference(questionWithValidationExpression));
        }

        private static QuestionnaireVerificationError CustomConditionExpressionUsesTextListQuestion(IQuestion questionWithConditionExpression)
        {
            return new QuestionnaireVerificationError("WB0044",
                VerificationMessages.WB0044_TextListQuestionCannotBeUsedInConditionsExpressions,
                CreateReference(questionWithConditionExpression));
        }

        private static QuestionnaireVerificationError CustomConditionExpressionUsesTextListQuestion(IGroup groupWithConditionExpression)
        {
            return new QuestionnaireVerificationError("WB0044",
                VerificationMessages.WB0044_TextListQuestionCannotBeUsedInConditionsExpressions,
                CreateReference(groupWithConditionExpression));
        }

        private static QuestionnaireVerificationError CustomConditionExpressionUsesNotRecognizedParameter(IComposite questionWithConditionExpression)
        {
            return new QuestionnaireVerificationError("WB0005",
                VerificationMessages.WB0005_CustomEnablementConditionReferencesNotExistingQuestion,
                CreateReference(questionWithConditionExpression));
        }

        private static QuestionnaireVerificationError CustomValidationExpressionUsesNotRecognizedParameter(IComposite questionWithValidationExpression)
        {
            return new QuestionnaireVerificationError("WB0004",
                VerificationMessages.WB0004_CustomValidationExpressionReferencesNotExistingQuestion,
                CreateReference(questionWithValidationExpression));
        }

        private static QuestionnaireVerificationError CustomConditionExpressionReferencesQuestionWithDeeperPropagationLevel(
           IComposite questionWithValidationExpression, IQuestion questionsReferencedInValidation)
        {
            return new QuestionnaireVerificationError("WB0046",
                VerificationMessages.WB0046_CustomConditionExpressionReferencesQuestionWithDeeperRosterLevel,
                CreateReference(questionWithValidationExpression),
                CreateReference(questionsReferencedInValidation));

        }

        private static QuestionnaireVerificationError CustomValidationExpressionReferencesQuestionWithDeeperPropagationLevel(
            IComposite questionWithValidationExpression, IQuestion questionsReferencedInValidation)
        {
            return new QuestionnaireVerificationError("WB0014",
                VerificationMessages.WB0014_CustomValidationExpressionReferencesQuestionWithDeeperRosterLevel,
                CreateReference(questionWithValidationExpression),
                CreateReference(questionsReferencedInValidation));

        }

        private static QuestionnaireVerificationError QuestionWithTitleSubstitutionCantBePrefilled(IQuestion questionsWithSubstitution)
        {
            return new QuestionnaireVerificationError("WB0015",
                VerificationMessages.WB0015_QuestionWithTitleSubstitutionCantBePrefilled,
                CreateReference(questionsWithSubstitution));
        }

        private static QuestionnaireVerificationError QuestionWithTitleSubstitutionCantReferenceQuestionsWithDeeperPropagationLevel(IQuestion questionsWithSubstitution, IQuestion questionSourceOfSubstitution)
        {
            return new QuestionnaireVerificationError("WB0019",
                VerificationMessages.WB0019_QuestionWithTitleSubstitutionCantReferenceQuestionsWithDeeperPropagationLevel,
                CreateReference(questionsWithSubstitution),
                CreateReference(questionSourceOfSubstitution));
        }

        private static QuestionnaireVerificationError QuestionWithTitleSubstitutionReferencesQuestionOfNotSupportedType(IQuestion questionsWithSubstitution, IQuestion questionSourceOfSubstitution)
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

        private static QuestionnaireVerificationError LinkedQuestionReferencesQuestionOfNotSupportedType(IQuestion linkedQuestion, IQuestion sourceQuestion)
        {
            return new QuestionnaireVerificationError("WB0012",
                VerificationMessages.WB0012_LinkedQuestionReferencesQuestionOfNotSupportedType,
                CreateReference(linkedQuestion),
                CreateReference(sourceQuestion));
        }

        private static QuestionnaireVerificationError LinkedQuestionReferenceQuestionNotUnderRosterGroup(IQuestion linkedQuestion, IQuestion sourceQuestion)
        {
            return new QuestionnaireVerificationError("WB0013",
                VerificationMessages.WB0013_LinkedQuestionReferencesQuestionNotUnderRosterGroup,
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

        private static bool IsRosterGroup(IGroup group)
        {
            return group.IsRoster;
        }

        private static bool IsRosterByQuestion(IGroup group)
        {
            return group.IsRoster && (group.RosterSizeSource == RosterSizeSourceType.Question);
        }

        private static bool IsRosterByFixedTitles(IGroup group)
        {
            return group.IsRoster && (group.RosterSizeSource == RosterSizeSourceType.FixedTitles);
        }

        private static bool IsRosterSizeQuestion(IQuestion question, QuestionnaireDocument questionnaire)
        {
            var rosterSizeQuestionIds =
                questionnaire.Find<IGroup>(IsRosterByQuestion).Select(group => group.RosterSizeQuestionId);
            return rosterSizeQuestionIds.Contains(question.PublicKey);
        }

        private static bool IsMaxValueMissing(IQuestion question)
        {
            var integerQuestion = question as INumericQuestion;

            if (integerQuestion != null && integerQuestion.IsInteger)
                return !integerQuestion.MaxValue.HasValue;
            else
                return false;
        }

        private static int? GetRosterSizeQuestionMaxValue(IQuestion question)
        {
            var integerQuestion = question as INumericQuestion;

            return integerQuestion != null && integerQuestion.IsInteger
                ? integerQuestion.MaxValue
                : null;
        }

        private static IQuestion GetRosterSizeQuestionByRosterGroup(IGroup group, QuestionnaireDocument questionnaire)
        {
            return
                questionnaire.FirstOrDefault<IQuestion>(
                    question => group.RosterSizeQuestionId.HasValue && question.PublicKey == group.RosterSizeQuestionId.Value);
        }

        private static IEnumerable<IGroup> GetAllParentGroupsForQuestion(IQuestion question, QuestionnaireDocument document)
        {
            return GetSpecifiedGroupAndAllItsParentGroupsStartingFromBottom((IGroup)question.GetParent(), document);
        }

        private static Guid[] GetAllRosterSizeQuestionsAsVectorOrNullIfSomeAreMissing(IComposite item, QuestionnaireDocument questionnaire)
        {
            IGroup parent = item is IQuestion
                ? (IGroup) item.GetParent()
                : (IGroup) item;

            Guid?[] rosterSizeQuestions =
                GetSpecifiedGroupAndAllItsParentGroupsStartingFromBottom(parent, questionnaire)
                    .Where(IsRosterGroup)
                    .Select(g => g.RosterSizeQuestionId)
                    .ToArray();

            return rosterSizeQuestions.All(id => id.HasValue)
                ? rosterSizeQuestions.Select(id => id.Value).ToArray()
                : null;
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

        private static bool QuestionHasDeeperRosterLevelThenVectorOfRosterQuestions(IQuestion question,
            Guid[] vectorOfRosterSizeQuestions, QuestionnaireDocument questionnaire)
        {
            Guid[] rosterQuestionsAsVectorForQuestionSourceOfSubstitution =
                GetAllRosterSizeQuestionsAsVectorOrNullIfSomeAreMissing(question, questionnaire);

            return
                rosterQuestionsAsVectorForQuestionSourceOfSubstitution != null &&
                rosterQuestionsAsVectorForQuestionSourceOfSubstitution.Length > 0 &&
                rosterQuestionsAsVectorForQuestionSourceOfSubstitution.Except(
                    vectorOfRosterSizeQuestions).Any();
        }

        private static bool IsSpecialThisIdentifier(string identifier)
        {
            return identifier.ToLower() == "this";
        }
    }
}