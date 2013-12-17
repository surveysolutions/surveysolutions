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
                    Verifier<IComposite>(this.CustomEnablementConditionReferencesNotExistingQuestion, "WB0005", VerificationMessages.WB0005_CustomEnablementConditionReferencesNotExistingQuestion),
                    Verifier<IGroup>(QuestionnaireHaveAutopropagatedGroups, "WB0027", VerificationMessages.WB0027_QuestionnaireHaveAutopropagatedGroups),
                    Verifier<IQuestion>(QuestionnaireHaveAutopropagatedQuestions, "WB0028", VerificationMessages.WB0028_QuestionnaireHaveAutopropagatedQuestions),
                    Verifier<IGroup>(RosterGroupHasGroupInsideItself, "WB0029", VerificationMessages.WB0029_RosterGroupHasGroup),
                    Verifier<IGroup>(GroupRosteredByQuestionHasNoRosterSizeQuestion, "WB0009", VerificationMessages.WB0009_GroupRosteredByQuestionHasNoRosterSizeQuestion),
                    Verifier<IGroup>(GroupRosteredByQuestionHasInvalidRosterSizeQuestion, "WB0023", VerificationMessages.WB0023_GroupRosteredByQuestionHasInvalidRosterSizeQuestion),
                    Verifier<IQuestion>(RosterSizeQuestionCannotBeInsideAnyRosterGroup, "WB0024", VerificationMessages.WB0024_RosterSizeQuestionCannotBeInsideAnyRosterGroup),
                    Verifier<IQuestion>(RosterSizeQuestionMaxValueCouldNotBeEmpty, "WB0025", VerificationMessages.WB0025_RosterSizeQuestionMaxValueCouldNotBeEmpty),
                    Verifier<IQuestion>(RosterSizeQuestionMaxValueCouldBeInRange1And20, "WB0026", VerificationMessages.WB0026_RosterSizeQuestionMaxValueCouldBeInRange1And20),
                    Verifier<IQuestion>(PrefilledQuestionCantBeInsideOfRoster, "WB0030", VerificationMessages.WB0030_PrefilledQuestionCantBeInsideOfRoster),
                    Verifier<IQuestion>(HeadQuestionCantBeInsideOfNonRoster, "WB0031", VerificationMessages.WB0031_HeadQuestionCantBeInsideOfNonRoster),
                    Verifier<IGroup>(GroupRosteredByQuestionHaveFixedTitles, "WB0032", VerificationMessages.WB0032_GroupRosteredByQuestionHaveFixedTitles),
                    Verifier<IGroup>(GroupRosteredByFixedTitlesHaveRosterSizeQuestion, "WB0033", VerificationMessages.WB0033_GroupRosteredByFixedTitlesHaveRosterSizeQuestion),
                    Verifier<IGroup>(GroupRosteredByFixedTitlesHaveRosterTitleQuestion, "WB0034", VerificationMessages.WB0034_GroupRosteredByFixedTitlesHaveRosterTitleQuestion),
                    Verifier<IGroup>(GroupRosteredByQuestionHasInvalidRosterTitleQuestion, "WB0035", VerificationMessages.WB0035_GroupRosteredByQuestionHasInvalidRosterTitleQuestion),
                    Verifier<IGroup>(GroupRosteredByCategoricalQuestionHaveRosterTitleQuestion, "WB0036", VerificationMessages.WB0036_GroupRosteredByCategoricalQuestionHaveRosterTitleQuestion),
                    Verifier<IGroup>(GroupRosteredByFixedTitlesHaveEmptyTitles, "WB0037", VerificationMessages.WB0037_GroupRosteredByFixedTitlesHaveEmptyTitles),

                    this.ErrorsByQuestionsWithCustomValidationReferencingQuestionsWithDeeperRosterLevel,
                    ErrorsByLinkedQuestions,
                    ErrorsByQuestionsWithSubstitutions,

                    Verifier<IMultyOptionsQuestion>(this.CategoricalMultianswerQuestionIsFeatured, "WB0022",VerificationMessages.WB0022_PrefilledQuestionsOfIllegalType),
                    Verifier<IMultyOptionsQuestion>(CategoricalMultianswerQuestionHasIncorrectMaxAnswerCount, "WB0021", VerificationMessages.WB0021_CategoricalMultianswerQuestionHasIncorrectMaxAnswerCount)
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

        private static bool GroupRosteredByQuestionHasNoRosterSizeQuestion(IGroup group, QuestionnaireDocument questionnaire)
        {
            return IsRosterByQuestion(group) && GetRosterSizeQuestionByRosterGroup(group, questionnaire) == null;
        }

        private static bool GroupRosteredByQuestionHasInvalidRosterSizeQuestion(IGroup group, QuestionnaireDocument questionnaire)
        {
            var rosterSizeQuestion = GetRosterSizeQuestionByRosterGroup(group, questionnaire);
            if (rosterSizeQuestion == null)
                return false;

            return !IsQuestionAllowedToBeRosterSize(rosterSizeQuestion);
        }

        private static bool GroupRosteredByQuestionHaveFixedTitles(IGroup group)
        {
            return IsRosterByQuestion(group) && group.RosterFixedTitles != null && group.RosterFixedTitles.Any();
        }

        private static bool GroupRosteredByFixedTitlesHaveRosterSizeQuestion(IGroup group)
        {
            return IsRosterByFixedTitles(group) && group.RosterSizeQuestionId.HasValue;
        }

        private static bool GroupRosteredByFixedTitlesHaveRosterTitleQuestion(IGroup group)
        {
            return IsRosterByFixedTitles(group) && group.RosterTitleQuestionId.HasValue;
        }

        private static bool GroupRosteredByQuestionHasInvalidRosterTitleQuestion(IGroup group, QuestionnaireDocument questionnaire)
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

            var groupsRosteredByRosterSizeQuestion =
                questionnaire.Find<IGroup>(
                    x => x.RosterSizeQuestionId.HasValue && x.RosterSizeQuestionId.Value == group.RosterSizeQuestionId.Value)
                    .Select(x => x.PublicKey);

            var parentForRosterTitleQuestion = questionnaire.GetParentOfQuestion(rosterTitleQuestion.PublicKey);
            if (parentForRosterTitleQuestion == null)
                return true;

            return !groupsRosteredByRosterSizeQuestion.Contains(parentForRosterTitleQuestion.PublicKey);
        }

        private static bool GroupRosteredByCategoricalQuestionHaveRosterTitleQuestion(IGroup group, QuestionnaireDocument questionnaire)
        {
            if (!IsRosterByQuestion(group))
                return false;
            if (!IsCategoricalRosterSizeQuestion(GetRosterSizeQuestionByRosterGroup(group, questionnaire)))
                return false;
            return group.RosterTitleQuestionId.HasValue;
        }

        private static bool GroupRosteredByFixedTitlesHaveEmptyTitles(IGroup group)
        {
            if (!IsRosterByFixedTitles(group))
                return false;
            if (group.RosterFixedTitles == null)
                return false;
            if (group.RosterFixedTitles.Length == 0)
                return false;

            return group.RosterFixedTitles.Any(string.IsNullOrWhiteSpace);
        }

        private static bool RosterSizeQuestionCannotBeInsideAnyRosterGroup(IQuestion question, QuestionnaireDocument questionnaire)
        {
            return IsRosterSizeQuestion(question, questionnaire) && GetAllParentGroupsForQuestion(question, questionnaire).Any(IsRosterGroup);
        }

        private static bool RosterSizeQuestionMaxValueCouldNotBeEmpty(IQuestion question, QuestionnaireDocument questionnaire)
        {
            if (!IsRosterSizeQuestion(question, questionnaire))
                return false;
            if (!IsQuestionAllowedToBeRosterSize(question))
                return false;
            return !GetRosterSizeQuestionMaxValue(question).HasValue;
        }

        private static bool RosterSizeQuestionMaxValueCouldBeInRange1And20(IQuestion question, QuestionnaireDocument questionnaire)
        {
            if (!IsRosterSizeQuestion(question, questionnaire))
                return false;
            if (!IsQuestionAllowedToBeRosterSize(question))
                return false;
            var rosterSizeQuestionMaxValue = GetRosterSizeQuestionMaxValue(question);
            if (!rosterSizeQuestionMaxValue.HasValue)
                return false;
            return !Enumerable.Range(1, 20).Contains(rosterSizeQuestionMaxValue.Value);
        }

        private static bool IsQuestionAllowedToBeRosterSize(IQuestion question)
        {
            return IsNumericRosterSizeQuestion(question) || IsCategoricalRosterSizeQuestion(question);
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

        private static bool HeadQuestionCantBeInsideOfNonRoster(IQuestion question, QuestionnaireDocument questionnaire)
        {
            return question.Capital && !GetAllParentGroupsForQuestion(question, questionnaire).Any(IsRosterGroup);
        }

        private static bool PrefilledQuestionCantBeInsideOfRoster(IQuestion question, QuestionnaireDocument questionnaire)
        {
            return question.Featured && GetAllParentGroupsForQuestion(question, questionnaire).Any(IsRosterGroup);
        }

        private static bool RosterGroupHasGroupInsideItself(IGroup group, QuestionnaireDocument questionnaire)
        {
            return IsRosterGroup(group) && questionnaire.Find<IGroup>(x => IsParentForGroup(x, group)).Any();
        }

        private static bool IsParentForGroup(IGroup group, IGroup parentGroup)
        {
            var parent = group.GetParent();
            return parent != null && parent.PublicKey == parentGroup.PublicKey;
        }

        private static bool QuestionnaireHaveAutopropagatedQuestions(IQuestion question, QuestionnaireDocument questionnaire)
        {
            return questionnaire.Find<IAutoPropagateQuestion>(_ => true).Any();
        }

        private static bool QuestionnaireHaveAutopropagatedGroups(IGroup group, QuestionnaireDocument questionnaire)
        {
            return questionnaire.Find<IGroup>(IsGroupPropagatable).Any();
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
                        identifier => GetVerificationErrorByCustomValidationReferenceOrNull(
                            questionWithValidationExpression, identifier, vectorOfRosterSizeQuestionsForQuestionWithCustomValidation,
                            questionnaire));
                }
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

        private bool CustomEnablementConditionReferencesNotExistingQuestion(IComposite entity, QuestionnaireDocument questionnaire)
        {
            string enablementCondition = GetCustomEnablementCondition(entity);

            if (string.IsNullOrWhiteSpace(enablementCondition))
                return false;

            IEnumerable<string> identifiersUsedInExpression = this.expressionProcessor.GetIdentifiersUsedInExpression(enablementCondition);

            return identifiersUsedInExpression.Any(
                identifier => !QuestionnaireContainsQuestionCorrespondingToExpressionIdentifier(questionnaire, identifier));
        }

        private bool CategoricalMultianswerQuestionIsFeatured(IMultyOptionsQuestion question, QuestionnaireDocument questionnaire)
        {
            return question.Featured;
        }

        private static bool QuestionnaireContainsQuestionCorrespondingToExpressionIdentifier(QuestionnaireDocument questionnaire, string identifier)
        {
            Guid questionId;
            if (!Guid.TryParse(identifier, out questionId))
                return questionnaire.FirstOrDefault<IQuestion>(question=>question.StataExportCaption==identifier) != null;

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
            Guid[] vectorOfRosterQuestionsForQuestionWithCustomValidation, QuestionnaireDocument questionnaire)
        {
            if (IsSpecialThisIdentifier(identifier))
            {
                return null;
            }
            IQuestion questionsReferencedInValidation = null;
            Guid parsedId;

            if (Guid.TryParse(identifier, out parsedId))
            {
                questionsReferencedInValidation = questionnaire.FirstOrDefault<IQuestion>(q => q.PublicKey == parsedId);
            }
            else
            {
                questionsReferencedInValidation = questionnaire.FirstOrDefault<IQuestion>(q => q.StataExportCaption == identifier);
            }

            if (questionsReferencedInValidation == null)
            {
                return CustomValidationExpressionUsesNotRecognizedParameter(questionWithValidationExpression);
            }

            if (QuestionHasDeeperRosterLevelThenVectorOfRosterQuestions(questionsReferencedInValidation,
                vectorOfRosterQuestionsForQuestionWithCustomValidation, questionnaire))
            {
                return CustomValidationExpressionReferencesQuestionWithDeeperPropagationLevel(
                    questionWithValidationExpression, questionsReferencedInValidation);
            }

            return null;
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

        private static QuestionnaireVerificationError CustomValidationExpressionUsesNotRecognizedParameter(IQuestion questionWithValidationExpression)
        {
            return new QuestionnaireVerificationError("WB0004",
                VerificationMessages.WB0004_CustomValidationExpressionReferencesNotExistingQuestion,
                CreateReference(questionWithValidationExpression));
        }

        private static QuestionnaireVerificationError CustomValidationExpressionReferencesQuestionWithDeeperPropagationLevel(
            IQuestion questionWithValidationExpression, IQuestion questionsReferencedInValidation)
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

        private static int? GetRosterSizeQuestionMaxValue(IQuestion question)
        {
            var integerQuestion = question as INumericQuestion;
            if (integerQuestion != null)
                return integerQuestion.IsInteger ? integerQuestion.MaxValue : null;
            
            var multiOptionsQuestion = question as IMultyOptionsQuestion;
            if (multiOptionsQuestion != null)
                return question.LinkedToQuestionId.HasValue ? (int?)null : question.Answers.Count;
            return null;
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

        private static Guid[] GetAllRosterSizeQuestionsAsVectorOrNullIfSomeAreMissing(IQuestion question, QuestionnaireDocument questionnaire)
        {
            Guid?[] rosterSizeQuestions =
                GetSpecifiedGroupAndAllItsParentGroupsStartingFromBottom((IGroup) question.GetParent(), questionnaire)
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