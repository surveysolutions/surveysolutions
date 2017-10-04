using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Designer.Verifier
{
    public class QuestionVerifications : AbstractVerifier, IPartialVerifier
    {
        private IEnumerable<Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>>> ErrorsVerifiers => new[]
        {
            Error<IMultyOptionsQuestion>(CategoricalMultiAnswersQuestionHasOptionsCountLessThanMaxAllowedAnswersCount, "WB0021", VerificationMessages.WB0021_CategoricalMultiAnswersQuestionHasOptionsCountLessThanMaxAllowedAnswersCount),
            Error<IMultyOptionsQuestion>(CategoricalMultianswerQuestionIsPrefilled, "WB0022",VerificationMessages.WB0022_PrefilledQuestionsOfIllegalType),
            Error<IMultyOptionsQuestion>(RosterSizeMultiOptionQuestionShouldBeLimited, "WB0082",VerificationMessages.WB0082_RosterSizeMultiOptionQuestionShouldBeLimited),
            Error<IQuestion>((q, document) => RosterSizeQuestionMaxValueCouldBeInRange1And60(q, document,GetMaxNumberOfAnswersForRosterSizeQuestionWhenMore200Options), "WB0100",string.Format(VerificationMessages.WB0100_MaxNumberOfAnswersForRosterSizeQuestionCannotBeGreaterThen200,MaxRosterSizeAnswer)),
            Error<IQuestion>(PrefilledQuestionCantBeInsideOfRoster, "WB0030",VerificationMessages.WB0030_PrefilledQuestionCantBeInsideOfRoster),
            Error<ITextListQuestion>(TextListQuestionCannotBePrefilled, "WB0039",VerificationMessages.WB0039_TextListQuestionCannotBePrefilled),
            Error<ITextListQuestion>(TextListQuestionCannotBeFilledBySupervisor, "WB0040",VerificationMessages.WB0040_TextListQuestionCannotBeFilledBySupervisor),
            Error<ITextListQuestion>(TextListQuestionMaxAnswerNotInRange1And200, "WB0042",string.Format(VerificationMessages.WB0042_TextListQuestionMaxAnswerInRange1And200,Constants.MaxLongRosterRowCount, Constants.MinLongRosterRowCount)),
            Error<ITextListQuestion>(RosterSizeListQuestionShouldBeLimited, "WB0093",VerificationMessages.WB0093_RosterSizeListOptionQuestionShouldBeLimit),
            Error<IQRBarcodeQuestion>(QRBarcodeQuestionIsSupervisorQuestion, "WB0049",VerificationMessages.WB0049_QRBarcodeQuestionIsSupervisorQuestion),
            Error<IQRBarcodeQuestion>(QRBarcodeQuestionIsPreFilledQuestion, "WB0050",VerificationMessages.WB0050_QRBarcodeQuestionIsPreFilledQuestion),
            Error<IQuestion>(QuestionHasEmptyVariableName, "WB0057",VerificationMessages.WB0057_QuestionHasEmptyVariableName),
            Error<IQuestion>(QuestionHasInvalidVariableName, "WB0077",VerificationMessages.WB0077_QuestionHasInvalidVariableName),
            Error<ICategoricalQuestion>(CategoricalQuestionHasLessThan2Options, "WB0060",string.Format(VerificationMessages.WB0060_CategoricalQuestionHasLessThan2Options, MinOptionsCount)),
            Error<IMultyOptionsQuestion>(CategoricalMultiAnswersQuestionHasMaxAllowedAnswersLessThan2, "WB0061",string.Format(VerificationMessages.WB0061_CategoricalMultiAnswersQuestionHasMaxAllowedAnswersLessThan2, MinOptionsCount)),
            Error<ICategoricalQuestion>(CategoricalQuestionHasNonIntegerOptionsValues, "WB0114", string.Format(VerificationMessages.WB0114_CategoricalQuestionSupportsOnlyIntegerPositiveValues, int.MinValue, int.MaxValue)),
            Error<IQuestion>(QuestionTypeIsNotAllowed, "WB0066", VerificationMessages.WB0066_QuestionTypeIsNotAllowed),
            Error<IQuestion>(OptionValuesMustBeUniqueForCategoricalQuestion, "WB0073", VerificationMessages.WB0073_OptionValuesMustBeUniqueForCategoricalQuestion),
            Error<ICategoricalQuestion>(CategoricalQuestionIsLinked, "WB0074", VerificationMessages.WB0074_CategoricalQuestionHasOptionsAndIsLinked),
            Error<IQuestion>(FilteredComboboxContainsMoreThanMaxOptions, "WB0075", string.Format(VerificationMessages.WB0075_FilteredComboboxContainsMoreThan5000Options, MaxOptionsCountInFilteredComboboxQuestion)),
            Error<IQuestion>(CategoricalOptionsCountMoreThanMaxOptionCount, "WB0076", string.Format(VerificationMessages.WB0076_CategoricalOptionsCountMoreThan200, MaxOptionsCountInCategoricalOptionQuestion)),
            Error<IMultimediaQuestion>(MultimediaQuestionIsInterviewersOnly, "WB0078", VerificationMessages.WB0078_MultimediaQuestionIsInterviewersOnly),
            Error<IMultimediaQuestion>(MultimediaShouldNotHaveValidationExpression, "WB0079", VerificationMessages.WB0079_MultimediaShouldNotHaveValidationExpression),
            Error<SingleQuestion>(CascadingQuestionReferencesMissingParent, "WB0086", VerificationMessages.WB0086_ParentCascadingQuestionShouldExist),
            Error<SingleQuestion>(CascadingQuestionHasMoreThanAllowedOptions, "WB0088", string.Format(VerificationMessages.WB0088_CascadingQuestionShouldHaveAllowedAmountOfAnswers, MaxOptionsCountInFilteredComboboxQuestion)),
            Error<SingleQuestion>(CascadingQuestionOptionsWithParentValuesShouldBeUnique, "WB0089", VerificationMessages.WB0089_CascadingQuestionOptionWithParentShouldBeUnique),
            Error<IQuestion>(LinkedQuestionIsInterviewersOnly, "WB0090", VerificationMessages.WB0090_LinkedQuestionIsInterviewersOnly),
            Error<IQuestion>(QuestionWithOptionsFilterCannotBePrefilled, "WB0029", VerificationMessages.WB0029_QuestionWithOptionsFilterCannotBePrefilled),
            Error<IQuestion, IComposite>(CascadingComboboxOptionsHasNoParentOptions, "WB0084", VerificationMessages.WB0084_CascadingOptionsShouldHaveParent),
            Error<IQuestion, IComposite>(ParentShouldNotHaveDeeperRosterLevelThanCascadingQuestion, "WB0085", VerificationMessages.WB0085_CascadingQuestionWrongParentLevel),
            Error<SingleQuestion, SingleQuestion>(CascadingHasCircularReference, "WB0087", VerificationMessages.WB0087_CascadingQuestionHasCicularReference),
            ErrorForTranslation<IQuestion>(OptionTitlesMustBeUniqueForCategoricalQuestion, "WB0072", VerificationMessages.WB0072_OptionTitlesMustBeUniqueForCategoricalQuestion),
            ErrorForTranslation<IQuestion>(QuestionHasOptionsWithEmptyValue, "WB0045",VerificationMessages.WB0045_QuestionHasOptionsWithEmptyValue),
            ErrorForTranslation<IComposite, ValidationCondition>(GetValidationConditionsOrEmpty, ValidationMessageIsTooLong, "WB0105", index => string.Format(VerificationMessages.WB0105_ValidationMessageIsTooLong, index, MaxValidationMessageLength)),
            ErrorForTranslation<IQuestion>(QuestionTitleIsTooLong, "WB0259", string.Format(VerificationMessages.WB0259_QuestionTitleIsTooLong, MaxTitleLength)),
            Error<IQuestion>(QuestionTitleEmpty, "WB0269", VerificationMessages.WB0269_QuestionTitleIsEmpty),
            Error_ManyGpsPrefilledQuestions_WB0006,
            ErrorsByLinkedQuestions,
            //Error<IComposite>(VariableNameTooLong, "WB0121", string.Format(VerificationMessages.WB0121_VariableNameTooLong, variableLengthLimit)),
            //Error<IComposite>(VariableNameHasSpecialCharacters, "WB0122", VerificationMessages.WB0122_VariableNameHasSpecialCharacters),
            //Error<IComposite>(VariableNameStartWithDigitOrUnderscore, "WB0123", VerificationMessages.WB0123_VariableNameStartWithDigitOrUnderscore),
            //Error<IComposite>(VariableNameEndWithUnderscore, "WB0124", VerificationMessages.WB0124_VariableNameEndWithUnderscore),
            //Error<IComposite>(VariableNameHasConsecutiveUnderscores, "WB0125", VerificationMessages.WB0125_VariableNameHasConsecutiveUnderscores),
            //Error<IComposite>(VariableNameIsKeywords, "WB0125", VerificationMessages.WB0125_VariableNameHasConsecutiveUnderscores),
            //Error<IComposite>(VarialbeNameNotUnique, "WB0126", VerificationMessages.WB0126_VarialbeNameNotUnique),

            Warning(TooManyQuestions, "WB0205", string.Format(VerificationMessages.WB0205_TooManyQuestions, MaxQuestionsCountInQuestionnaire)),
            Warning(FewSectionsManyQuestions, "WB0206", string.Format(VerificationMessages.WB0206_FewSectionsManyQuestions, ManyQuestionsByFewSectionsCount)),
            Warning(MoreThan50PercentQuestionsWithoutValidationConditions, "WB0208", string.Format(VerificationMessages.WB0208_MoreThan50PercentsQuestionsWithoutValidationConditions, MinValidationsInPercents)),
            Warning<IQuestion>(CategoricalQuestionHasALotOfOptions, "WB0210", VerificationMessages.WB0210_CategoricalQuestionHasManyOptions),
            Warning(HasNoGpsQuestions, "WB0211", VerificationMessages.WB0211_QuestionnaireHasNoGpsQuestion),
            Warning<IQuestion>(VariableLableMoreThan120Characters, "WB0217", string.Format(VerificationMessages.WB0217_VariableLableMoreThan120Characters, MaxVariableLabelLength)),
            Warning(NoCurrentTimeQuestions, "WB0221", VerificationMessages.WB0221_NoCurrentTimeQuestions),
            Warning<SingleQuestion>(Prefilled, "WB0222", VerificationMessages.WB0222_SingleOptionPrefilled),
            Warning<SingleQuestion>(ComboBoxWithLessThan10Elements, "WB0225", VerificationMessages.WB0225_ComboBoxWithLessThan10Elements),
            WarningForCollection(SameCascadingParentQuestion, "WB0226", VerificationMessages.WB0226_SameCascadingParentQuestion),
            Warning<ICategoricalQuestion>(OmittedOptions, "WB0228", VerificationMessages.WB0228_OmittedOptions),
            Warning<ICategoricalQuestion>(NonconsecutiveCascadings, "WB0230", VerificationMessages.WB0230_NonconsecutiveCascadings),
            Warning<MultyOptionsQuestion>(MoreThan20Options, "WB0231", string.Format(VerificationMessages.WB0231_MultiOptionWithMoreThan20Options, MaxMultiQuestionOptionsCount)),
            WarningForCollection(FiveOrMoreQuestionsWithSameEnabling, "WB0232", VerificationMessages.WB0232_FiveOrMoreQuestionsWithSameEnabling),
            Warning<IQuestion>(UseFunctionIsValidEmailToValidateEmailAddress, "WB0254", VerificationMessages.WB0254_UseFunctionIsValidEmailToValidateEmailAddress),
            Warning<IQuestion>(QuestionIsTooShort, "WB0255", VerificationMessages.WB0255_QuestionIsTooShort),
            Warning<GpsCoordinateQuestion>(Any, "WB0264", VerificationMessages.WB0264_GpsQuestion),
            Warning<QRBarcodeQuestion>(Any, "WB0267", VerificationMessages.WB0267_QRBarcodeQuestion),
            Warning(TooFewVariableLabelsAreDefined, "WB0253", VerificationMessages.WB0253_TooFewVariableLabelsAreDefined),
            Warning(MoreThan30PercentQuestionsAreText, "WB0265", string.Format(VerificationMessages.WB0265_MoreThan30PercentQuestionsAreText, TextQuestionsLengthInPercents)),
            WarningForCollection(SameTitle, "WB0266", VerificationMessages.WB0266_SameTitle),
            Warning(NoPrefilledQuestions, "WB0216", VerificationMessages.WB0216_NoPrefilledQuestions),
        };

        private static IEnumerable<QuestionnaireNodeReference[]> SameTitle(MultiLanguageQuestionnaireDocument questionnaire)
            => questionnaire
                .Find<IQuestion>()
                .GroupBy(question => question.QuestionText)
                .Where(grouping => grouping.Count() > 1)
                .Select(grouping => grouping.Select(question => CreateReference(question)).ToArray());

        private static bool MoreThan30PercentQuestionsAreText(MultiLanguageQuestionnaireDocument questionnaire)
            => questionnaire.Find<TextQuestion>().Count(AnsweredManually) > 0.3 * questionnaire.Find<IQuestion>().Count(AnsweredManually);

        private static bool TooFewVariableLabelsAreDefined(MultiLanguageQuestionnaireDocument questionnaire)
        {
            var countOfAllQuestions = questionnaire.Find<IQuestion>(AnsweredManually).Count();
            var countOfQuestionsWithoutLabels =
                questionnaire.Find<IQuestion>(q => string.IsNullOrEmpty(q.VariableLabel) && AnsweredManually(q)).Count();

            return countOfQuestionsWithoutLabels > (countOfAllQuestions / 2);
        }

        private static bool QuestionIsTooShort(IQuestion question)
        {
            if (string.IsNullOrEmpty(question.QuestionText) || !AnsweredManually(question))
                return false;

            return question.QuestionText.Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length <= 2;
        }
        private static bool AnsweredManually(IQuestion question) =>
            !question.Featured && question.QuestionScope != QuestionScope.Headquarter && question.QuestionScope != QuestionScope.Hidden;

        private static bool Any(IComposite entity) => true;

        private static bool UseFunctionIsValidEmailToValidateEmailAddress(IQuestion question)
        {
            if (question.QuestionType != QuestionType.Text || string.IsNullOrEmpty(question.QuestionText) || question.ValidationConditions.Count > 0)
                return false;

            return question.QuestionText.IndexOf("email", StringComparison.InvariantCultureIgnoreCase) >= 0;
        }

        private static IEnumerable<QuestionnaireNodeReference[]> FiveOrMoreQuestionsWithSameEnabling(MultiLanguageQuestionnaireDocument questionnaire)
            => questionnaire
                .Find<IQuestion>()
                .Where(question => !string.IsNullOrWhiteSpace(question.ConditionExpression))
                .GroupBy(question => question.ConditionExpression)
                .Where(grouping => grouping.Count() >= 5)
                .Select(grouping => grouping.Select(question => CreateReference(question)).ToArray());

        private static bool MoreThan20Options(ICategoricalQuestion question) => question.Answers?.Count > 20;

        private static bool NonconsecutiveCascadings(ICategoricalQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
            => question.CascadeFromQuestionId.HasValue
               && question.CascadeFromQuestionId.Value != question.GetPrevious()?.PublicKey;

        private static bool OmittedOptions(ICategoricalQuestion question)
        {
            int[] existingOptions = (question.Answers ?? Enumerable.Empty<Answer>())
                .Select(option => option.AnswerValue.ParseIntOrNull())
                .Where(value => value.HasValue)
                .Select(value => value.Value)
                .OrderBy(x => x)
                .Distinct()
                .ToArray();

            if (existingOptions.Length == 0)
                return false;


            List<int> diffsByPrevNextOptionValues = new List<int>();

            existingOptions.Aggregate((prev, next) =>
            {
                diffsByPrevNextOptionValues.Add(next - prev);
                return next;
            });

            return diffsByPrevNextOptionValues.Contains(2);
        }

        private static IEnumerable<QuestionnaireNodeReference[]> SameCascadingParentQuestion(MultiLanguageQuestionnaireDocument questionnaire)
            => questionnaire
                .Find<SingleQuestion>(question => question.CascadeFromQuestionId.HasValue)
                .GroupBy(question => question.CascadeFromQuestionId)
                .Where(grouping => grouping.Count() > 1)
                .Select(grouping => grouping.Select(question => CreateReference(question)).ToArray());

        private static bool ComboBoxWithLessThan10Elements(SingleQuestion question)
            => (question.IsFilteredCombobox ?? false)
               && question.Answers.Count < 10;


        private static bool Prefilled(IQuestion question) => question.Featured;

        private static bool VariableLableMoreThan120Characters(IQuestion question)
            => (question.VariableLabel?.Length ?? 0) > 120;

        private static bool HasNoGpsQuestions(MultiLanguageQuestionnaireDocument questionnaire)
            => !questionnaire.Has<IQuestion>(q => q.QuestionType == QuestionType.GpsCoordinates);

        private static bool NoPrefilledQuestions(MultiLanguageQuestionnaireDocument questionnaire)
            => !questionnaire.Has<IQuestion>(q => q.Featured);

        private static bool NoCurrentTimeQuestions(MultiLanguageQuestionnaireDocument questionnaire)
            => !questionnaire.Has<DateTimeQuestion>(q => q.IsTimestamp);

        private static bool CategoricalQuestionHasALotOfOptions(IQuestion question)
            => question.QuestionType == QuestionType.SingleOption
               && !question.IsFilteredCombobox.GetValueOrDefault(false)
               && !question.CascadeFromQuestionId.HasValue
               && question.Answers.Count > 30;

        private static bool MoreThan50PercentQuestionsWithoutValidationConditions(MultiLanguageQuestionnaireDocument questionnaire)
            => questionnaire.Find<IQuestion>().Where(NoValidation).Count(AnsweredManually) > 0.5 * questionnaire.Find<IQuestion>().Count(AnsweredManually);

        private static bool FewSectionsManyQuestions(MultiLanguageQuestionnaireDocument questionnaire)
            => questionnaire.Find<IQuestion>().Count() > 100
               && questionnaire.Find<IGroup>(IsSection).Count() < 3;

       
        private static bool NoValidation(IQuestion question) => !question.ValidationConditions.Any();

        private static bool TooManyQuestions(MultiLanguageQuestionnaireDocument questionnaire)
            => questionnaire.Find<IQuestion>().Count() > 1000;

        private static readonly IEnumerable<QuestionType> QuestionTypesValidToBeLinkedQuestionSource = new[]
        {
            QuestionType.DateTime,
            QuestionType.Numeric,
            QuestionType.Text,
            QuestionType.TextList
        };

        private static readonly QuestionType[] RestrictedVariableLengthQuestionTypes = 
            {
                QuestionType.GpsCoordinates,
                QuestionType.MultyOption,
                QuestionType.TextList
            };

        private static readonly HashSet<QuestionType> WhiteListOfQuestionTypes = new HashSet<QuestionType>
        {
            QuestionType.SingleOption,
            QuestionType.MultyOption,
            QuestionType.Numeric,
            QuestionType.DateTime,
            QuestionType.GpsCoordinates,
            QuestionType.Text,
            QuestionType.TextList,
            QuestionType.QRBarcode,
            QuestionType.Multimedia,
            QuestionType.Area,
            QuestionType.Audio
        };

        private static IEnumerable<ValidationCondition> GetValidationConditionsOrEmpty(IComposite entity)
        {
            var entityAsIConditional = entity as IValidatable;

            return entityAsIConditional != null
                ? entityAsIConditional.ValidationConditions
                : Enumerable.Empty<ValidationCondition>();
        }

        private static bool ValidationMessageIsTooLong(IComposite question, ValidationCondition validationCondition, MultiLanguageQuestionnaireDocument questionnaire)
            => validationCondition.Message?.Length > 250;

        private static IEnumerable<QuestionnaireVerificationMessage> Error_ManyGpsPrefilledQuestions_WB0006(MultiLanguageQuestionnaireDocument document)
        {
            var gpsPrefilledQuestions = document.Find<GpsCoordinateQuestion>(q => q.Featured).ToArray();
            if (gpsPrefilledQuestions.Length < 2)
                return Enumerable.Empty<QuestionnaireVerificationMessage>();

            var gpsPrefilledQuestionsReferences = gpsPrefilledQuestions.Select(x => CreateReference(x)).ToArray();

            return new[]
            {
                QuestionnaireVerificationMessage.Error("WB0006",
                    VerificationMessages.WB0006_OnlyOneGpsQuestionCouldBeMarkedAsPrefilled,
                    gpsPrefilledQuestionsReferences)
            };
        }

        private static IEnumerable<QuestionnaireVerificationMessage> ErrorsByLinkedQuestions(
            MultiLanguageQuestionnaireDocument questionnaire)
        {
            var linkedQuestions = questionnaire.Find<IQuestion>(question => question.LinkedToQuestionId.HasValue);

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

                if (sourceQuestion.QuestionType == QuestionType.TextList)
                {
                    if (!string.IsNullOrWhiteSpace(linkedQuestion.LinkedFilterExpression) ||
                        !string.IsNullOrEmpty(linkedQuestion.Properties.OptionsFilterExpression))
                    {
                        yield return LinkedToTextListQuestionDoesNotSupportFilters(linkedQuestion);
                    }

                    var linkedRosterScope = questionnaire.Questionnaire.GetRosterScope(linkedQuestion.PublicKey);
                    var sourceRosterScope = questionnaire.Questionnaire.GetRosterScope(sourceQuestion.PublicKey);
                    if (!sourceRosterScope.IsSameOrParentScopeFor(linkedRosterScope))
                    {
                        yield return LinkedQuestionReferenceTextListQuestionFromWrongScope(linkedQuestion, sourceQuestion);
                    }
                }
                else
                {
                    var isSourceQuestionInsideRosterGroup = questionnaire.Questionnaire.GetRosterScope(sourceQuestion).Any();
                    if (!isSourceQuestionInsideRosterGroup)
                    {
                        yield return LinkedQuestionReferenceQuestionNotUnderRosterGroup(linkedQuestion, sourceQuestion);
                    }
                }
            }

            var questionsLinkedOnRoster = questionnaire.Find<IQuestion>(
                question => question.LinkedToRosterId.HasValue);

            foreach (var questionLinkedOnRoster in questionsLinkedOnRoster)
            {
                var sourceRoster = questionnaire.Find<IGroup>(questionLinkedOnRoster.LinkedToRosterId.Value);
                if (sourceRoster == null)
                {
                    yield return QuestionnaireVerificationMessage.Critical("WB0053",
                        VerificationMessages.WB0053_LinkedQuestionReferencesNotExistingRoster,
                        CreateReference(questionLinkedOnRoster));
                    continue;
                }
                if (!sourceRoster.IsRoster)
                {
                    yield return QuestionnaireVerificationMessage.Error("WB0103",
                        VerificationMessages.WB0103_LinkedQuestionReferencesGroupWhichIsNotARoster,
                        CreateReference(questionLinkedOnRoster));
                    continue;
                }
            }
        }

        private static QuestionnaireVerificationMessage LinkedQuestionReferencesNotExistingQuestion(IQuestion linkedQuestion)
            => QuestionnaireVerificationMessage.Critical("WB0011",
                VerificationMessages.WB0011_LinkedQuestionReferencesNotExistingQuestion,
                CreateReference(linkedQuestion));

        private static QuestionnaireVerificationMessage LinkedQuestionReferencesQuestionOfNotSupportedType(IQuestion linkedQuestion, IQuestion sourceQuestion)
            => QuestionnaireVerificationMessage.Error("WB0012",
                VerificationMessages.WB0012_LinkedQuestionReferencesQuestionOfNotSupportedType,
                CreateReference(linkedQuestion),
                CreateReference(sourceQuestion));

        private static QuestionnaireVerificationMessage LinkedQuestionReferenceQuestionNotUnderRosterGroup(IQuestion linkedQuestion, IQuestion sourceQuestion)
            => QuestionnaireVerificationMessage.Error("WB0013",
                VerificationMessages.WB0013_LinkedQuestionReferencesQuestionNotUnderRosterGroup,
                CreateReference(linkedQuestion),
                CreateReference(sourceQuestion));

        private static QuestionnaireVerificationMessage LinkedQuestionReferenceTextListQuestionFromWrongScope(IQuestion linkedQuestion, IQuestion sourceQuestion)
            => QuestionnaireVerificationMessage.Error("WB0116",
                VerificationMessages.WB0116_LinkedQuestionReferenceTextListQuestionFromWrongScope,
                CreateReference(linkedQuestion),
                CreateReference(sourceQuestion));

        private static QuestionnaireVerificationMessage LinkedToTextListQuestionDoesNotSupportFilters(IQuestion linkedQuestion)
            => QuestionnaireVerificationMessage.Critical("WB0117",
                VerificationMessages.WB0117_LinkedToTextListQuestionDoesNotSupportFilters,
                CreateReference(linkedQuestion));

        private static bool QuestionTitleEmpty(IQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
            => string.IsNullOrWhiteSpace(question.QuestionText);

        private static bool RosterSizeQuestionMaxValueCouldBeInRange1And60(IQuestion question,
            MultiLanguageQuestionnaireDocument questionnaire, Func<IQuestion, int?> getRosterSizeQuestionMaxValue)
        {
            if (!questionnaire.Questionnaire.IsRosterSizeQuestion(question))
                return false;
            if (!questionnaire.Questionnaire.IsQuestionAllowedToBeRosterSizeSource(question))
                return false;
            var rosterSizeQuestionMaxValue = getRosterSizeQuestionMaxValue(question);
            if (!rosterSizeQuestionMaxValue.HasValue)
                return false;
            return !Enumerable.Range(1, Constants.MaxLongRosterRowCount).Contains(rosterSizeQuestionMaxValue.Value);
        }
       

        private static int? GetMaxNumberOfAnswersForRosterSizeQuestionWhenMore200Options(IQuestion question)
        {
            var multyOptionQuestion = question as IMultyOptionsQuestion;
            if (multyOptionQuestion != null && multyOptionQuestion.Answers.Count > Constants.MaxLongRosterRowCount)
                return multyOptionQuestion.MaxAllowedAnswers;
            return null;
        }

        private static bool QuestionTitleIsTooLong(IQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
            => question.QuestionText?.Length > 500;

        private static bool QuestionWithOptionsFilterCannotBePrefilled(IQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return !string.IsNullOrWhiteSpace(question.Properties.OptionsFilterExpression) && questionnaire.Questionnaire.IsPreFilledQuestion(question);
        }

       
        private static bool LinkedQuestionIsInterviewersOnly(IQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (!(question.LinkedToQuestionId.HasValue || question.LinkedToRosterId.HasValue))
                return false;

            return question.QuestionScope != QuestionScope.Interviewer || questionnaire.Questionnaire.IsPreFilledQuestion(question);
        }

        private static bool CascadingQuestionOptionsWithParentValuesShouldBeUnique(SingleQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (question.Answers != null && question.CascadeFromQuestionId.HasValue)
            {
                var enumerable = question.Answers.Select(x => new {x.AnswerText, x.ParentValue})
                    .Distinct().ToList();
                var uniqueCount = enumerable.Count();
                var result = uniqueCount != question.Answers.Count;
                return result;
            }
            return false;
        }

        private static bool CascadingQuestionHasMoreThanAllowedOptions(SingleQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return question.CascadeFromQuestionId.HasValue && question.Answers != null &&
                   question.Answers.Count > MaxOptionsCountInCascadingQuestion;
        }

        private static EntityVerificationResult<SingleQuestion> CascadingHasCircularReference(SingleQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (!question.CascadeFromQuestionId.HasValue)
                return new EntityVerificationResult<SingleQuestion> {HasErrors = false};

            var referencedEntities = new HashSet<SingleQuestion>();

            SingleQuestion GetParentCascadingQuestion(SingleQuestion x)
            {
                return questionnaire
                    .Find<SingleQuestion>(q => q.CascadeFromQuestionId.HasValue &&
                                               q.PublicKey == x.CascadeFromQuestionId.Value)
                    .SingleOrDefault();
            }

            var cascadingAncestors = question.UnwrapReferences(GetParentCascadingQuestion);

            foreach (var ancestor in cascadingAncestors)
            {
                if (referencedEntities.Contains(ancestor))
                {
                    return new EntityVerificationResult<SingleQuestion>
                    {
                        HasErrors = true,
                        ReferencedEntities = referencedEntities
                    };
                }

                referencedEntities.Add(ancestor);
            }

            return new EntityVerificationResult<SingleQuestion> {HasErrors = false};
        }

        private static bool CascadingQuestionReferencesMissingParent(SingleQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (!question.CascadeFromQuestionId.HasValue)
                return false;

            return questionnaire.Find<SingleQuestion>(question.CascadeFromQuestionId.Value) == null;
        }

        private bool CategoricalQuestionHasNonIntegerOptionsValues(ICategoricalQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            var answers = question.Answers;
            if (answers == null)
                return false;

            foreach (var option in answers)
            {
                if (string.IsNullOrWhiteSpace(option.AnswerValue))
                    continue;

                int optionValue;
                if (!int.TryParse(option.AnswerValue, out optionValue))
                {
                    return true;
                }
            }
            return false;
        }

        private static EntityVerificationResult<IComposite> ParentShouldNotHaveDeeperRosterLevelThanCascadingQuestion(
            IQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (!question.CascadeFromQuestionId.HasValue)
                return new EntityVerificationResult<IComposite> {HasErrors = false};

            var parentQuestion = questionnaire.Find<IQuestion>(question.CascadeFromQuestionId.Value);
            if (parentQuestion == null)
                return new EntityVerificationResult<IComposite> {HasErrors = false};

            var parentRosters = questionnaire.Questionnaire.GetRosterScope(parentQuestion);
            var questionRosters = questionnaire.Questionnaire.GetRosterScope(question);

            if (parentRosters.Length > questionRosters.Length ||
                parentRosters.Where((parentGuid, i) => questionRosters[i] != parentGuid).Any())
            {
                return new EntityVerificationResult<IComposite>
                {
                    HasErrors = true,
                    ReferencedEntities = new IComposite[] {question, parentQuestion},
                };
            }

            return new EntityVerificationResult<IComposite> {HasErrors = false};
        }


        private static EntityVerificationResult<IComposite> CascadingComboboxOptionsHasNoParentOptions(IQuestion question, MultiLanguageQuestionnaireDocument document)
        {
            if (!question.CascadeFromQuestionId.HasValue)
                return new EntityVerificationResult<IComposite> {HasErrors = false};

            var parentQuestion = document.Find<SingleQuestion>(question.CascadeFromQuestionId.Value);
            if (parentQuestion == null)
                return new EntityVerificationResult<IComposite> {HasErrors = false};

            var result = !question.Answers.All(childAnswer =>
                parentQuestion.Answers.Any(
                    parentAnswer => parentAnswer.AnswerValue == childAnswer.ParentValue));

            return new EntityVerificationResult<IComposite>
            {
                HasErrors = result,
                ReferencedEntities = new List<IComposite> {question, parentQuestion}
            };
        }

        private static bool MultimediaShouldNotHaveValidationExpression(IMultimediaQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return !string.IsNullOrEmpty(question.ValidationExpression);
        }

        private static bool MultimediaQuestionIsInterviewersOnly(IMultimediaQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return question.QuestionScope != QuestionScope.Interviewer || questionnaire.Questionnaire.IsPreFilledQuestion(question);
        }

        private static bool CategoricalOptionsCountMoreThanMaxOptionCount(IQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return ((questionnaire.Questionnaire.IsCategoricalSingleAnswerQuestion(question) && !question.CascadeFromQuestionId.HasValue &&
                     !questionnaire.Questionnaire.IsFilteredComboboxQuestion(question)) || questionnaire.Questionnaire.IsCategoricalMultiAnswersQuestion(question))
                   && question.Answers != null && question.Answers.Count > MaxOptionsCountInCategoricalOptionQuestion;
        }

        private static bool FilteredComboboxContainsMoreThanMaxOptions(IQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return questionnaire.Questionnaire.IsFilteredComboboxQuestion(question) && question.Answers != null &&
                   question.Answers.Count > MaxOptionsCountInFilteredComboboxQuestion;
        }

        private static bool CategoricalQuestionIsLinked(ICategoricalQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return (question.Answers?.Any() ?? false) &&
                   (question.LinkedToQuestionId.HasValue || question.LinkedToRosterId.HasValue);
        }

        private static bool OptionTitlesMustBeUniqueForCategoricalQuestion(IQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (question.Answers != null && !question.CascadeFromQuestionId.HasValue)
            {
                return question.Answers.Where(x => x.AnswerText != null).Select(x => x.AnswerText.Trim()).Distinct()
                           .Count() != question.Answers.Count;
            }
            return false;
        }

        private static bool OptionValuesMustBeUniqueForCategoricalQuestion(IQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (question.Answers != null)
            {
                var answersValues = question.Answers.Where(x => !string.IsNullOrWhiteSpace(x.AnswerValue))
                    .Select(x => x.AnswerValue.Trim()).ToList();
                return answersValues.Distinct().Count() != answersValues.Count();
            }
            return false;
        }

        private static bool QuestionTypeIsNotAllowed(IQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return !WhiteListOfQuestionTypes.Contains(question.QuestionType);
        }

        private static bool CategoricalMultiAnswersQuestionHasMaxAllowedAnswersLessThan2(IMultyOptionsQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return question.MaxAllowedAnswers.HasValue && question.MaxAllowedAnswers < 2;
        }

        private static bool CategoricalQuestionHasLessThan2Options(ICategoricalQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (question.LinkedToQuestionId.HasValue || question.LinkedToRosterId.HasValue)
                return false;

            return question.Answers == null || question.Answers.Count < 2;
        }

        private static bool CategoricalMultiAnswersQuestionHasOptionsCountLessThanMaxAllowedAnswersCount(IMultyOptionsQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return question.MaxAllowedAnswers.HasValue &&
                   !(question.LinkedToQuestionId.HasValue || question.LinkedToRosterId.HasValue) &&
                   (question.MaxAllowedAnswers.Value > question.Answers.Count);
        }

        private static bool CategoricalMultianswerQuestionIsPrefilled(IMultyOptionsQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return questionnaire.Questionnaire.IsPreFilledQuestion(question);
        }


        private static bool RosterSizeMultiOptionQuestionShouldBeLimited(IMultyOptionsQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (!questionnaire.Questionnaire.IsRosterSizeQuestion(question))
                return false;

            if (question.Answers.Count <= Constants.MaxRosterRowCount)
                return false;

            return !question.MaxAllowedAnswers.HasValue;
        }
        private static bool PrefilledQuestionCantBeInsideOfRoster(IQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return questionnaire.Questionnaire.IsPreFilledQuestion(question) && questionnaire.Questionnaire.GetRosterScope(question).Any();
        }

        private static bool TextListQuestionCannotBePrefilled(ITextListQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return questionnaire.Questionnaire.IsPreFilledQuestion(question);
        }

        private static bool TextListQuestionCannotBeFilledBySupervisor(ITextListQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return questionnaire.Questionnaire.IsSupervisorQuestion(question);
        }

        private static bool TextListQuestionMaxAnswerNotInRange1And200(ITextListQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (!question.MaxAnswerCount.HasValue)
                return false;
            return !Enumerable.Range(1, TextListQuestion.MaxAnswerCountLimit).Contains(question.MaxAnswerCount.Value);
        }

        private static bool RosterSizeListQuestionShouldBeLimited(ITextListQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (!questionnaire.Questionnaire.IsRosterSizeQuestion(question))
                return false;

            return !question.MaxAnswerCount.HasValue;
        }

        private static bool QuestionHasOptionsWithEmptyValue(IQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (!(question is SingleQuestion || question is IMultyOptionsQuestion))
                return false;

            if (question.LinkedToQuestionId.HasValue || question.LinkedToRosterId.HasValue)
                return false;

            return question.Answers.Any(option => string.IsNullOrWhiteSpace(option.AnswerValue));
        }

        private static bool QRBarcodeQuestionIsSupervisorQuestion(IQRBarcodeQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return questionnaire.Questionnaire.IsSupervisorQuestion(question);
        }

        private static bool QRBarcodeQuestionIsPreFilledQuestion(IQRBarcodeQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return questionnaire.Questionnaire.IsPreFilledQuestion(question);
        }

        private static bool QuestionHasEmptyVariableName(IQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return string.IsNullOrEmpty(question.StataExportCaption);
        }

        private static bool QuestionHasInvalidVariableName(IQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (string.IsNullOrEmpty(question.StataExportCaption))
                return false;

            int variableLengthLimit = RestrictedVariableLengthQuestionTypes.Contains(question.QuestionType)
                ? DefaultRestrictedVariableLengthLimit
                : DefaultVariableLengthLimit;

            if (question.StataExportCaption.Length > variableLengthLimit)
                return true;

            return !VariableNameRegex.IsMatch(question.StataExportCaption);
        }

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> Error<TEntity, TReferencedEntity>(
            Func<TEntity, MultiLanguageQuestionnaireDocument, EntityVerificationResult<TReferencedEntity>> verifyEntity, string code, string message)
            where TEntity : class, IComposite
            where TReferencedEntity : class, IComposite
        {
            return questionnaire =>
                from entity in questionnaire.Find<TEntity>(_ => true)
                let verificationResult = verifyEntity(entity, questionnaire)
                where verificationResult.HasErrors
                select QuestionnaireVerificationMessage.Error(code, message, verificationResult.ReferencedEntities.Select(x => CreateReference(x)).ToArray());
        }

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> ErrorForTranslation<TEntity>(
            Func<TEntity, MultiLanguageQuestionnaireDocument, bool> hasError, string code, string message)
            where TEntity : class, IComposite
        {
            return questionnaire =>
                questionnaire
                    .FindWithTranslations<TEntity>(entity => hasError(entity, questionnaire))
                    .Select(translatedEntity =>
                    {
                        var translationMessage = translatedEntity.TranslationName == null
                            ? message
                            : translatedEntity.TranslationName + ": " + message;
                        var questionnaireVerificationReference = CreateReference(translatedEntity.Entity);
                        return QuestionnaireVerificationMessage.Error(code, translationMessage, questionnaireVerificationReference);
                    });
        }

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> ErrorForTranslation<TEntity, TSubEntity>(
            Func<TEntity, IEnumerable<TSubEntity>> getSubEnitites, Func<TEntity, TSubEntity, MultiLanguageQuestionnaireDocument, bool> hasError, string code, Func<int, string> getMessageBySubEntityIndex)
            where TEntity : class, IComposite
        {
            return questionnaire =>
                questionnaire
                    .FindWithTranslations<TEntity>(entity => true)
                    .SelectMany(translatedEntity => getSubEnitites(translatedEntity.Entity).Select((subEntity, index) => new { Entity = translatedEntity, SubEntity = subEntity, Index = index }))
                    .Where(descriptor => hasError(descriptor.Entity.Entity, descriptor.SubEntity, questionnaire))
                    .Select(descriptor =>
                        QuestionnaireVerificationMessage.Error(
                            code,
                            descriptor.Entity.TranslationName == null
                                ? getMessageBySubEntityIndex(descriptor.Index + 1)
                                : descriptor.Entity.TranslationName + ": " + getMessageBySubEntityIndex(descriptor.Index + 1),
                            CreateReference(descriptor.Entity.Entity, descriptor.Index))
                    );
        }

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> Error<TEntity>(
            Func<TEntity, MultiLanguageQuestionnaireDocument, bool> hasError, string code, string message)
            where TEntity : class, IComposite
        {
            return questionnaire =>
                questionnaire
                    .Find<TEntity>(entity => hasError(entity, questionnaire))
                    .Select(entity => QuestionnaireVerificationMessage.Error(code, message, CreateReference(entity)));
        }


        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> Warning<TEntity>(
            Func<TEntity, bool> hasError, string code, string message)
            where TEntity : class, IQuestionnaireEntity
        {
            return questionnaire =>
                questionnaire
                    .Find<TEntity>(hasError)
                    .Select(entity => QuestionnaireVerificationMessage.Warning(code, message, CreateReference(entity)));
        }

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> Warning(
            Func<MultiLanguageQuestionnaireDocument, bool> hasError, string code, string message)
        {
            return questionnaire =>
                hasError(questionnaire)
                    ? new[] { QuestionnaireVerificationMessage.Warning(code, message) }
                    : Enumerable.Empty<QuestionnaireVerificationMessage>();
        }

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> Warning<TEntity>(
            Func<TEntity, MultiLanguageQuestionnaireDocument, bool> hasError, string code, string message)
            where TEntity : class, IQuestionnaireEntity
        {
            return questionnaire =>
                questionnaire
                    .Find<TEntity>(x => hasError(x, questionnaire))
                    .Select(entity => QuestionnaireVerificationMessage.Warning(code, message, CreateReference(entity)));
        }

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> WarningForCollection(
            Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireNodeReference[]>> getReferences, string code, string message)
        {
            return questionnaire
                => getReferences(questionnaire)
                    .Select(references => QuestionnaireVerificationMessage.Warning(code, message, references));
        }

        public IEnumerable<QuestionnaireVerificationMessage> Verify(MultiLanguageQuestionnaireDocument multiLanguageQuestionnaireDocument)
        {
            var verificationMessagesByQuestionnaire = new List<QuestionnaireVerificationMessage>();
            foreach (var verifier in this.ErrorsVerifiers)
            {
                verificationMessagesByQuestionnaire.AddRange(verifier.Invoke(multiLanguageQuestionnaireDocument));
            }
            return verificationMessagesByQuestionnaire;
        }
    }
}