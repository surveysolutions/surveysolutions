﻿using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Questionnaire.Categories;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Designer.Verifier
{
    public class QuestionVerifications : AbstractVerifier, IPartialVerifier
    {
        private readonly ISubstitutionService substitutionService;
        private readonly ICategoriesService categoriesService;

        public QuestionVerifications(ISubstitutionService substitutionService, ICategoriesService categoriesService)
        {
            this.substitutionService = substitutionService;
            this.categoriesService = categoriesService;
        }

        private IEnumerable<Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>>> ErrorsVerifiers => new[]
        {
            Error<IQuestion>("WB0030", PrefilledQuestionCantBeInsideOfRoster, VerificationMessages.WB0030_PrefilledQuestionCantBeInsideOfRoster),
            Error<IQuestion>("WB0066", QuestionTypeIsNotAllowed, VerificationMessages.WB0066_QuestionTypeIsNotAllowed),
            //Error<IQuestion>("WB0077", QuestionHasInvalidVariableName, VerificationMessages.WB0077_QuestionHasInvalidVariableName),
            Error<IQuestion>("WB0090", LinkedQuestionIsInterviewersOnly, VerificationMessages.WB0090_LinkedQuestionIsInterviewersOnly),
            Error<IQuestion>("WB0100", (q, document) => RosterSizeQuestionMaxValueCouldBeInRange1And60(q, document,GetMaxNumberOfAnswersForRosterSizeQuestionWhenMore200Options), string.Format(VerificationMessages.WB0100_MaxNumberOfAnswersForRosterSizeQuestionCannotBeGreaterThen200,MaxRosterSizeAnswer)),
            Error<IQuestion>("WB0269", QuestionTitleEmpty, VerificationMessages.WB0269_QuestionTitleIsEmpty),
            Error<ICategoricalQuestion>("WB0075", FilteredComboboxContainsMoreThanMaxOptions, string.Format(VerificationMessages.WB0075_FilteredComboboxContainsMoreThan5000Options, MaxOptionsCountInFilteredComboboxQuestion)),
            Error<SingleQuestion>("WB0086", CascadingQuestionReferencesMissingParent, VerificationMessages.WB0086_ParentCascadingQuestionShouldExist),
            Error<SingleQuestion>("WB0088", CascadingQuestionHasMoreThanAllowedOptions, string.Format(VerificationMessages.WB0088_CascadingQuestionShouldHaveAllowedAmountOfAnswers, MaxOptionsCountInFilteredComboboxQuestion)),
            Error<SingleQuestion>("WB0089", CascadingQuestionOptionsWithParentValuesShouldBeUnique, VerificationMessages.WB0089_CascadingQuestionOptionWithParentShouldBeUnique),
            Error<INumericQuestion>("WB0128", CountOfDecimalPlacesIsInRange1_15, string.Format(VerificationMessages.WB0128_CountOfDecimalPlacesIsNotInRange, MinCountOfDecimalPlaces, MaxCountOfDecimalPlaces)),
            Error<INumericQuestion>("WB0131", SpecialValuesHasNonIntegerOptionsValues, string.Format(VerificationMessages.WB0131_SpecialValuesHasNonIntegerOptionsValues, int.MinValue, int.MaxValue)),
            Error<INumericQuestion>("WB0132", SpecialValuesHasOptionsWithLongTexts, string.Format(VerificationMessages.WB0132_SpecialValuesHasOptionsWithLongTexts, 1, MaxOptionLength)),
            Error<INumericQuestion>("WB0133", SpecialValuesMustBeUniqueForNumericlQuestion, VerificationMessages.WB0133_SpecialValuesMustBeUniqueForNumericlQuestion),
            Error<INumericQuestion>("WB0134", SpecialValuesCountMoreThanMaxOptionCount, string.Format(VerificationMessages.WB0134_SpecialValuesCountMoreThanMaxOptionCount, MaxOptionsCountInCategoricalOptionQuestion)),
            Error<INumericQuestion>("WB0135", SpecialValuesForRosterSizeQuestionsCantBeMoreThanRosterLimit, VerificationMessages.WB0135_SpecialValuesForRosterSizeQuestionsCantBeMoreThanRosterLimit),
            Error<ITextListQuestion>("WB0039", TextListQuestionCannotBePrefilled, VerificationMessages.WB0039_TextListQuestionCannotBePrefilled),
            Error<ITextListQuestion>("WB0040", TextListQuestionCannotBeFilledBySupervisor, VerificationMessages.WB0040_TextListQuestionCannotBeFilledBySupervisor),
            Error<ITextListQuestion>("WB0042", TextListQuestionMaxAnswerNotInRange1And200, string.Format(VerificationMessages.WB0042_TextListQuestionMaxAnswerInRange1And200,Constants.MaxLongRosterRowCount, Constants.MinLongRosterRowCount)),
            Error<ITextListQuestion>("WB0093", RosterSizeListQuestionShouldBeLimited, VerificationMessages.WB0093_RosterSizeListOptionQuestionShouldBeLimit),
            Error<IQRBarcodeQuestion>("WB0049", QRBarcodeQuestionIsSupervisorQuestion, VerificationMessages.WB0049_QRBarcodeQuestionIsSupervisorQuestion),
            Error<IQRBarcodeQuestion>("WB0050", QRBarcodeQuestionIsPreFilledQuestion, VerificationMessages.WB0050_QRBarcodeQuestionIsPreFilledQuestion),
            Error<IMultimediaQuestion>("WB0078", MultimediaQuestionIsInterviewersOnly, VerificationMessages.WB0078_MultimediaQuestionIsInterviewersOnly),
            Error<IMultimediaQuestion>("WB0079", MultimediaShouldNotHaveValidationExpression, VerificationMessages.WB0079_MultimediaShouldNotHaveValidationExpression),
            Error<ICategoricalQuestion>("WB0029", QuestionWithOptionsFilterCannotBePrefilled, VerificationMessages.WB0029_QuestionWithOptionsFilterCannotBePrefilled),
            Error<ICategoricalQuestion>("WB0060", CategoricalQuestionHasLessThan2Options, string.Format(VerificationMessages.WB0060_CategoricalQuestionHasLessThan2Options, MinOptionsCount)),
            Error<ICategoricalQuestion>("WB0074", CategoricalQuestionIsLinked, VerificationMessages.WB0074_CategoricalQuestionHasOptionsAndIsLinked),
            Error<ICategoricalQuestion>("WB0114", CategoricalQuestionHasNonIntegerOptionsValues, string.Format(VerificationMessages.WB0114_CategoricalQuestionSupportsOnlyIntegerPositiveValues, int.MinValue, int.MaxValue)),
            Error<ICategoricalQuestion>("WB0129", CategoricalQuestionHasOptionsWithLongTexts, string.Format(VerificationMessages.WB0129_AnswerTitleIsTooLong, 1, MaxOptionLength)),
            Error<ICategoricalQuestion>("WB0073", OptionValuesMustBeUniqueForCategoricalQuestion, VerificationMessages.WB0073_OptionValuesMustBeUniqueForCategoricalQuestion),
            Error<ICategoricalQuestion>("WB0076", CategoricalOptionsCountMoreThanMaxOptionCount, string.Format(VerificationMessages.WB0076_CategoricalOptionsCountMoreThan200, MaxOptionsCountInCategoricalOptionQuestion)),
            Error<IMultyOptionsQuestion>("WB0007", MultiOptionQuestionYesNoQuestionCantBeLinked, VerificationMessages.WB0007_MultiOptionQuestionYesNoQuestionCantBeLinked),
            Error<IMultyOptionsQuestion>("WB0061", CategoricalMultiAnswersQuestionHasMaxAllowedAnswersLessThan2, string.Format(VerificationMessages.WB0061_CategoricalMultiAnswersQuestionHasMaxAllowedAnswersLessThan2, MinOptionsCount)),
            Error<IMultyOptionsQuestion>("WB0021", CategoricalMultiAnswersQuestionHasOptionsCountLessThanMaxAllowedAnswersCount, VerificationMessages.WB0021_CategoricalMultiAnswersQuestionHasOptionsCountLessThanMaxAllowedAnswersCount),
            Error<IMultyOptionsQuestion>("WB0022", CategoricalMultianswerQuestionIsPrefilled, VerificationMessages.WB0022_PrefilledQuestionsOfIllegalType),
            Error<IMultyOptionsQuestion>("WB0082", RosterSizeMultiOptionQuestionShouldBeLimited, VerificationMessages.WB0082_RosterSizeMultiOptionQuestionShouldBeLimited),
            Error<IQuestion, IComposite>("WB0084", CascadingComboboxOptionsHasNoParentOptions, VerificationMessages.WB0084_CascadingOptionsShouldHaveParent),
            Error<IQuestion, IComposite>("WB0085", ParentShouldNotHaveDeeperRosterLevelThanCascadingQuestion, VerificationMessages.WB0085_CascadingQuestionWrongParentLevel),
            Error<IQuestion>("WB0282", IdentifyingQuestionInSectionWithEnablingCondition, VerificationMessages.WB0282_IdentifyingQuestionInSectionWithCondition),
            ErrorForTranslation<IQuestion>("WB0072", OptionTitlesMustBeUniqueForCategoricalQuestion, VerificationMessages.WB0072_OptionTitlesMustBeUniqueForCategoricalQuestion),
            ErrorForTranslation<IQuestion>("WB0045", QuestionHasOptionsWithEmptyValue, VerificationMessages.WB0045_QuestionHasOptionsWithEmptyValue),
            ErrorForTranslation<IQuestion>("WB0259", QuestionTitleIsTooLong, string.Format(VerificationMessages.WB0259_QuestionTitleIsTooLong, MaxTitleLength)),
            ErrorForTranslation<INumericQuestion>("WB0136", QuestionHasSpecialValuesWithEmptyValue, VerificationMessages.WB0136_SpecialValuesHaveOptionsWithEmptyValue),
            ErrorForTranslation<INumericQuestion>("WB0137", SpecialValueTitlesMustBeUnique, VerificationMessages.WB0137_SpecialValuesTitlesMustBeUnique),
            Error<SingleQuestion, SingleQuestion>("WB0087", CascadingHasCircularReference, VerificationMessages.WB0087_CascadingQuestionHasCicularReference),
            ErrorForTranslation<IComposite, ValidationCondition>("WB0105", GetValidationConditionsOrEmpty, ValidationMessageIsTooLong, index => string.Format(VerificationMessages.WB0105_ValidationMessageIsTooLong, index, MaxValidationMessageLength)),
            ErrorForTranslation<IComposite>("WB0287", TableRosterDoesntContainsQuestionWithSubstitutions, VerificationMessages.WB0287_TableRosterDoesntContainsQuestionWithSubstitutions),

            Error_ManyGpsPrefilledQuestions_WB0006,
            ErrorsByLinkedQuestions,
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
            WarningByValueAndTitleNumbersIsNotEqualsInCategoricalQuestions,
        };

        private bool IdentifyingQuestionInSectionWithEnablingCondition(IQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (Prefilled(question))
            {
                var parentSections = question.GetParent().UnwrapReferences(x => x.GetParent()).OfType<IConditional>();
                return parentSections.Any(x => !string.IsNullOrEmpty(x.ConditionExpression));
            }

            return false;
        }

        private bool ComboboxShouldNotHaveParentValue(SingleQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (question.IsFilteredCombobox.GetValueOrDefault() && question.CascadeFromQuestionId == null)
            {
                return question.Answers.Any(x => x.ParentCode != null || x.ParentValue != null);
            }

            return false;
        }

        private bool MultiOptionQuestionYesNoQuestionCantBeLinked(IMultyOptionsQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return question.YesNoView && (question.LinkedToQuestionId.HasValue || question.LinkedToRosterId.HasValue);
        }

        private static IEnumerable<QuestionnaireEntityReference[]> SameTitle(MultiLanguageQuestionnaireDocument questionnaire)
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

        private static IEnumerable<QuestionnaireEntityReference[]> FiveOrMoreQuestionsWithSameEnabling(MultiLanguageQuestionnaireDocument questionnaire)
            => questionnaire
                .Find<IQuestion>()
                .Where(question => !string.IsNullOrWhiteSpace(question.ConditionExpression))
                .GroupBy(question => question.ConditionExpression)
                .Where(grouping => grouping.Count() >= 5)
                .Select(grouping => grouping.Select(question => CreateReference(question)).ToArray());

        private static bool MoreThan20Options(MultyOptionsQuestion question)
        {
            if (question.IsFilteredCombobox == true) return false;

            return question.Answers?.Count > 20;
        }

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

        private static IEnumerable<QuestionnaireEntityReference[]> SameCascadingParentQuestion(MultiLanguageQuestionnaireDocument questionnaire)
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

        private static IEnumerable<QuestionnaireVerificationMessage> WarningByValueAndTitleNumbersIsNotEqualsInCategoricalQuestions(MultiLanguageQuestionnaireDocument document)
        {
            var categoricalQuestions = document.Find<ICategoricalQuestion>(q => q.Answers?.Count > 0);

            foreach (var question in categoricalQuestions)
            {
                var optionsWithNotEqualsNumericValueAndTitle = question.Answers
                    .Select((option, index) => new {
                        value = option.AnswerValue.ParseIntOrNull(),
                        title = option.AnswerText.ParseIntOrNull(),
                        index = index,
                    })
                    .Where(option => option.value.HasValue && option.title.HasValue)
                    .Where(option => option.value.Value != option.title.Value)
                    .OrderBy(x => x.index)
                    .Distinct()
                    .ToArray();

                foreach (var option in optionsWithNotEqualsNumericValueAndTitle)
                {
                    var message = string.Format(VerificationMessages.WB0288_ValueAndTitleNumbersIsNotEquals, option.value.Value, option.title.Value);
                    var reference = QuestionnaireEntityReference.CreateFrom(question, QuestionnaireVerificationReferenceProperty.Option, option.index);
                    yield return QuestionnaireVerificationMessage.Warning("WB0288", message, reference);
                }
            }
        }

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

        private static bool CountOfDecimalPlacesIsInRange1_15(INumericQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (question.IsInteger)
                return false;
            if (!question.CountOfDecimalPlaces.HasValue)
                return false;

            return !(question.CountOfDecimalPlaces.Value >= MinCountOfDecimalPlaces && question.CountOfDecimalPlaces.Value <= MaxCountOfDecimalPlaces);
        }

        private static bool SpecialValuesHasNonIntegerOptionsValues(INumericQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return OptionsHaveNonIntegerValues(question.Answers);
        }

        private static bool SpecialValuesHasOptionsWithLongTexts(INumericQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return OptionsHasLongText(question.Answers);
        }

        private bool SpecialValuesMustBeUniqueForNumericlQuestion(INumericQuestion question, MultiLanguageQuestionnaireDocument questionnaire) 
            => OptionsHaveUniqueValues(question);

        private static bool SpecialValuesCountMoreThanMaxOptionCount(INumericQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return  question.Answers != null && question.Answers.Count > MaxOptionsCountInCategoricalOptionQuestion;
        }

        private static bool SpecialValuesForRosterSizeQuestionsCantBeMoreThanRosterLimit(INumericQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (!questionnaire.Questionnaire.IsRosterSizeQuestion(question))
                return false;
            if ((question.Answers?.Count ?? 0) == 0)
                return false;

            var rosterLimit = questionnaire.Questionnaire.IsTriggerForLongRoster(question) ? Constants.MaxLongRosterRowCount : Constants.MaxRosterRowCount;

            return  question.Answers.Any(x => x.GetParsedValue() > rosterLimit);
        }

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

        private static bool QuestionWithOptionsFilterCannotBePrefilled(ICategoricalQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
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

        private bool CategoricalQuestionHasOptionsWithLongTexts(ICategoricalQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return OptionsHasLongText(question.Answers);
        }

        private static bool OptionsHasLongText(List<Answer> options)
        {
            if (options == null)
                return false;

            return options.Any(option => !(option.AnswerText.Length >= 1 && option.AnswerText.Length <= MaxOptionLength));
        }

        private bool CategoricalQuestionHasNonIntegerOptionsValues(ICategoricalQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return OptionsHaveNonIntegerValues(question.Answers);
        }

        private static bool OptionsHaveNonIntegerValues(List<Answer> answers)
        {
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


        private EntityVerificationResult<IComposite> CascadingComboboxOptionsHasNoParentOptions(IQuestion question, MultiLanguageQuestionnaireDocument document)
        {
            var categoricalQuestion = question as ICategoricalQuestion;
            bool hasErrors = false;

            if (!question.CascadeFromQuestionId.HasValue)
                return new EntityVerificationResult<IComposite> {HasErrors = hasErrors};

            var parentQuestion = document.Find<SingleQuestion>(question.CascadeFromQuestionId.Value);
            if (parentQuestion == null)
                return new EntityVerificationResult<IComposite> {HasErrors = hasErrors};

            if (!categoricalQuestion.CategoriesId.HasValue && !parentQuestion.CategoriesId.HasValue)
            {
                hasErrors = !question.Answers.All(childAnswer =>
                    parentQuestion.Answers.Any(
                        parentAnswer => parentAnswer.AnswerValue == childAnswer.ParentValue));
            }
            else if(categoricalQuestion.CategoriesId.HasValue && !parentQuestion.CategoriesId.HasValue)
            {
                var categories = this.categoriesService.GetCategoriesById(categoricalQuestion.CategoriesId.Value)
                    .Select(x => new {x.Id, x.ParentId}).ToList();

                hasErrors = !categories.All(childAnswer =>
                    parentQuestion.Answers.Any(
                        parentAnswer => parentAnswer.GetParsedValue() == childAnswer.ParentId));
            }
            else if(!categoricalQuestion.CategoriesId.HasValue && parentQuestion.CategoriesId.HasValue)
            {
                var parentCategories = this.categoriesService.GetCategoriesById(parentQuestion.CategoriesId.Value)
                    .Select(x => new {x.Id, x.ParentId}).ToList();

                hasErrors = !question.Answers.All(childAnswer =>
                    parentCategories.Any(parentAnswer => parentAnswer.Id == childAnswer.GetParsedParentValue()));
            }
            else
            {
                var categories = this.categoriesService.GetCategoriesById(categoricalQuestion.CategoriesId.Value);
                var parentCategories = this.categoriesService.GetCategoriesById(parentQuestion.CategoriesId.Value);

                hasErrors = parentCategories.GroupJoin(categories, item => item.Id, item => item.ParentId,
                    (pc, c) => c).Any(x => !x.Any());
            }

            return new EntityVerificationResult<IComposite>
            {
                HasErrors = hasErrors,
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

        private bool CategoricalOptionsCountMoreThanMaxOptionCount(ICategoricalQuestion question,
            MultiLanguageQuestionnaireDocument questionnaire)
        {
            if(question.CascadeFromQuestionId.HasValue) return false;
            if (questionnaire.Questionnaire.IsFilteredComboboxQuestion(question)) return false;

            if (question.CategoriesId.HasValue)
                return this.categoriesService.GetCategoriesById(question.CategoriesId.Value).Count() > MaxOptionsCountInCategoricalOptionQuestion;

            return question.Answers?.Count > MaxOptionsCountInCategoricalOptionQuestion;
        }

        private static bool FilteredComboboxContainsMoreThanMaxOptions(ICategoricalQuestion question, MultiLanguageQuestionnaireDocument questionnaire) 
            => questionnaire.Questionnaire.IsFilteredComboboxQuestion(question) && question.Answers?.Count > MaxOptionsCountInFilteredComboboxQuestion;

        private static bool CategoricalQuestionIsLinked(ICategoricalQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return (question.Answers?.Any() ?? false) &&
                   (question.LinkedToQuestionId.HasValue || question.LinkedToRosterId.HasValue);
        }

        private bool SpecialValueTitlesMustBeUnique(INumericQuestion question, 
            MultiLanguageQuestionnaireDocument questionnaire) => HasUniqueOptionTitles(question);

        private bool OptionTitlesMustBeUniqueForCategoricalQuestion(IQuestion question,
            MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (!(question is SingleQuestion || question is IMultyOptionsQuestion))
                return false;

            if (question.CascadeFromQuestionId.HasValue) 
                return false;

            return HasUniqueOptionTitles(question);
        }

        private bool HasUniqueOptionTitles(IQuestion question)
        {
            if (question is ICategoricalQuestion categoriesQuestion && categoriesQuestion.CategoriesId.HasValue)
            {
                return this.categoriesService.GetCategoriesById(categoriesQuestion.CategoriesId.Value)
                    .Where(x => x.Text != null)
                    .GroupBy(x => x.Text.Trim())
                    .Any(x => x.Count() > 1);
            }

            return question.Answers
                       ?.Where(x => x.AnswerText != null)
                       ?.GroupBy(x => x.AnswerText.Trim())
                       ?.Any(x => x.Count() > 1) ?? false;
        }

        private bool OptionValuesMustBeUniqueForCategoricalQuestion(ICategoricalQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (!question.CascadeFromQuestionId.HasValue)
                return OptionsHaveUniqueValues(question);

            if (CascadingHasCircularReference((SingleQuestion) question, questionnaire).HasErrors) return false;

            var list = new List<int[]>();
            var parentQuestions = new List<ICategoricalQuestion>();

            var parentQuestionId = question.CascadeFromQuestionId;
            while (parentQuestionId != null)
            {
                var cascadingQuestion = questionnaire.Find<ICategoricalQuestion>(parentQuestionId.Value);
                if (cascadingQuestion == null) break;

                parentQuestions.Add(cascadingQuestion);
                parentQuestionId = cascadingQuestion.CascadeFromQuestionId;
            }

            if (parentQuestions.Count == 0) return OptionsHaveUniqueValues(question);

            var categories = question.CategoriesId.HasValue
                ? this.categoriesService.GetCategoriesById(question.CategoriesId.Value).Select(x => new {x.Id, x.ParentId}).ToList()
                : question.Answers.Select(x => new {Id = (int) x.GetParsedValue(), ParentId = x.GetParsedParentValue()}).ToList();

            foreach (var questionAnswer in categories)
            {
                var valueAndParentValues = new List<int> {questionAnswer.Id};
                var parsedParentValue = questionAnswer.ParentId;

                if (parsedParentValue.HasValue)
                    valueAndParentValues.Add(parsedParentValue.Value);

                foreach (var parentQuestion in parentQuestions)
                {
                    var lastParentId = valueAndParentValues?.LastOrDefault();

                    parsedParentValue = parentQuestion.CategoriesId.HasValue
                        ? this.categoriesService.GetCategoriesById(parentQuestion.CategoriesId.Value).FirstOrDefault(x => x.Id == lastParentId)?.ParentId
                        : parentQuestion.Answers.Find(x => (int) x.GetParsedValue() == lastParentId)?.GetParsedParentValue();

                    if (parsedParentValue.HasValue)
                        valueAndParentValues.Add(parsedParentValue.Value);
                }

                list.Add(valueAndParentValues.ToArray());
            }

            return list.Distinct(new EqualityArray()).Count() != list.Count;
        }

        class EqualityArray : IEqualityComparer<int[]>
        {
            public bool Equals(int[] x, int[] y) => x.SequenceEqual(y);
            public int GetHashCode(int[] obj) => obj.Aggregate(0, (current, i) => current ^ i.GetHashCode());
        }

        private bool OptionsHaveUniqueValues(IQuestion question)
        {
            if (question is ICategoricalQuestion categoriesQuestion && categoriesQuestion.CategoriesId.HasValue)
            {
                return this.categoriesService.GetCategoriesById(categoriesQuestion.CategoriesId.Value)
                    .GroupBy(x => x.Id)
                    .Any(x => x.Count() > 1);
            }

            return question.Answers
                ?.Where(x => !string.IsNullOrWhiteSpace(x.AnswerValue))
                ?.GroupBy(x => x.AnswerValue.Trim())
                ?.Any(x => x.Count() > 1) ?? false;
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
            if (question.LinkedToQuestionId.HasValue || question.LinkedToRosterId.HasValue || question.CategoriesId.HasValue)
                return false;

            return question.Answers == null || question.Answers.Count < 2;
        }

        private bool CategoricalMultiAnswersQuestionHasOptionsCountLessThanMaxAllowedAnswersCount(IMultyOptionsQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (!question.MaxAllowedAnswers.HasValue) return false;
            if (question.LinkedToQuestionId.HasValue || question.LinkedToRosterId.HasValue) return false;

            if (question.CategoriesId.HasValue)
                return question.MaxAllowedAnswers.Value > this.categoriesService.GetCategoriesById(question.CategoriesId.Value).Count();

            return (question.MaxAllowedAnswers.Value > question.Answers.Count);
        }

        private static bool CategoricalMultianswerQuestionIsPrefilled(IMultyOptionsQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return questionnaire.Questionnaire.IsPreFilledQuestion(question);
        }


        private static bool RosterSizeMultiOptionQuestionShouldBeLimited(IMultyOptionsQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (!questionnaire.Questionnaire.IsRosterSizeQuestion(question))
                return false;

            if ((question.Answers?.Count ?? 0) <= Constants.MaxRosterRowCount)
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

        private static bool QuestionHasSpecialValuesWithEmptyValue(INumericQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return HasEmptyOptionValues(question.Answers);
        }

        private static bool QuestionHasOptionsWithEmptyValue(IQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (!(question is SingleQuestion || question is IMultyOptionsQuestion))
                return false;

            if (question.LinkedToQuestionId.HasValue || question.LinkedToRosterId.HasValue)
                return false;

            return HasEmptyOptionValues(question.Answers);
        }

        private static bool HasEmptyOptionValues(List<Answer> options)
        {
            if (options == null)
                return false;
            return options.Any(option => string.IsNullOrWhiteSpace(option.AnswerValue));
        }

        private static bool QRBarcodeQuestionIsSupervisorQuestion(IQRBarcodeQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return questionnaire.Questionnaire.IsSupervisorQuestion(question);
        }

        private static bool QRBarcodeQuestionIsPreFilledQuestion(IQRBarcodeQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return questionnaire.Questionnaire.IsPreFilledQuestion(question);
        }

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> Error<TEntity, TReferencedEntity>(string code, Func<TEntity, MultiLanguageQuestionnaireDocument, EntityVerificationResult<TReferencedEntity>> verifyEntity, string message)
            where TEntity : class, IComposite
            where TReferencedEntity : class, IComposite
        {
            return questionnaire =>
                from entity in questionnaire.Find<TEntity>(_ => true)
                let verificationResult = verifyEntity(entity, questionnaire)
                where verificationResult.HasErrors
                select QuestionnaireVerificationMessage.Error(code, message, verificationResult.ReferencedEntities.Select(x => CreateReference(x)).ToArray());
        }

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> ErrorForTranslation<TEntity>(string code, Func<TEntity, MultiLanguageQuestionnaireDocument, bool> hasError, string message)
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

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> ErrorForTranslation<TEntity, TSubEntity>(string code, Func<TEntity, IEnumerable<TSubEntity>> getSubEnitites, Func<TEntity, TSubEntity, MultiLanguageQuestionnaireDocument, bool> hasError, Func<int, string> getMessageBySubEntityIndex)
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
            Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireEntityReference[]>> getReferences, string code, string message)
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

        private bool TableRosterDoesntContainsQuestionWithSubstitutions(IComposite entity, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (((IGroup) entity.GetParent()).DisplayMode != RosterDisplayMode.Table)
                return false;

            var title = entity.GetTitle();
            string[] substitutionReferences = this.substitutionService.GetAllSubstitutionVariableNames(title, entity.VariableName);
            return substitutionReferences.Length > 0;
        }
    }
}
