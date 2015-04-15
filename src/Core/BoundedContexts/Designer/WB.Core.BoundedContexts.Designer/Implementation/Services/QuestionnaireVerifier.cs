using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveySolutions.Services;
using Newtonsoft.Json;
using System.Text;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    internal class QuestionnaireVerifier : IQuestionnaireVerifier
    {
        private const int MaxExpressionLength = 10000;

        private static readonly IEnumerable<QuestionType> QuestionTypesValidToBeLinkedQuestionSource = new[]
        {
            QuestionType.DateTime,
            QuestionType.Numeric,
            QuestionType.Text,
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
            QuestionType.Multimedia
        };

        private static readonly HashSet<QuestionType> QuestionTypesValidToBeRosterTitles = new HashSet<QuestionType>
        {
            QuestionType.SingleOption,
            QuestionType.MultyOption,
            QuestionType.Numeric,
            QuestionType.DateTime,
            QuestionType.GpsCoordinates,
            QuestionType.Text,
            QuestionType.TextList,
            QuestionType.QRBarcode
        };

        private static readonly IEnumerable<QuestionType> QuestionTypesValidToBeSubstitutionReferences = new[]
        {
            QuestionType.DateTime,
            QuestionType.Numeric,
            QuestionType.SingleOption,
            QuestionType.Text
        };

        private static readonly IEnumerable<QuestionType> QuestionTypesValidToHaveValidationExpressions = new[]
        {
            QuestionType.DateTime,
            QuestionType.Numeric,
            QuestionType.SingleOption,
            QuestionType.Text,
            QuestionType.GpsCoordinates, 
            QuestionType.MultyOption
        };

        private class VerificationState
        {
            public bool HasExceededLimitByValidationExpresssionCharactersLength { get; set; }
            public bool HasExceededLimitByConditionExpresssionCharactersLength { get; set; }
        }

        private struct EntityVerificationResult<TReferencedEntity>
            where TReferencedEntity : class, IComposite
        {
            public bool HasErrors { get; set; }
            public IEnumerable<TReferencedEntity> ReferencedEntities { get; set; }
        }

        private readonly IExpressionProcessor expressionProcessor;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ISubstitutionService substitutionService;
        private readonly IKeywordsProvider keywordsProvider;
        private readonly IExpressionProcessorGenerator expressionProcessorGenerator;

        private static readonly Regex VariableNameRegex = new Regex("^[_A-Za-z][_A-Za-z0-9]*$");
        private static readonly Regex QuestionnaireNameRegex = new Regex(@"^[\w \-\(\)\\/]*$");

        public QuestionnaireVerifier(IExpressionProcessor expressionProcessor, IFileSystemAccessor fileSystemAccessor,
            ISubstitutionService substitutionService, IKeywordsProvider keywordsProvider,
            IExpressionProcessorGenerator expressionProcessorGenerator)
        {
            this.expressionProcessor = expressionProcessor;
            this.fileSystemAccessor = fileSystemAccessor;
            this.substitutionService = substitutionService;
            this.keywordsProvider = keywordsProvider;
            this.expressionProcessorGenerator = expressionProcessorGenerator;
        }

        private IEnumerable<Func<QuestionnaireDocument, VerificationState, IEnumerable<QuestionnaireVerificationError>>> AtomicVerifiers
        {
            get
            {
                return new[]
                {
                    Verifier(NoQuestionsExist, "WB0001", VerificationMessages.WB0001_NoQuestions),
                    Verifier<IGroup>(GroupWhereRosterSizeSourceIsQuestionHasNoRosterSizeQuestion, "WB0009", VerificationMessages.WB0009_GroupWhereRosterSizeSourceIsQuestionHasNoRosterSizeQuestion),
                    Verifier<IMultyOptionsQuestion>(CategoricalMultiAnswersQuestionHasOptionsCountLessThanMaxAllowedAnswersCount, "WB0021", VerificationMessages.WB0021_CategoricalMultiAnswersQuestionHasOptionsCountLessThanMaxAllowedAnswersCount),
                    Verifier<IMultyOptionsQuestion>(CategoricalMultianswerQuestionIsFeatured, "WB0022",VerificationMessages.WB0022_PrefilledQuestionsOfIllegalType),
                    Verifier<IGroup>(RosterSizeSourceQuestionTypeIsIncorrect, "WB0023", VerificationMessages.WB0023_RosterSizeSourceQuestionTypeIsIncorrect),
                    Verifier<IQuestion>(RosterSizeQuestionMaxValueCouldNotBeEmpty, "WB0025", VerificationMessages.WB0025_RosterSizeQuestionMaxValueCouldNotBeEmpty),
                    Verifier<IQuestion>(RosterSizeQuestionMaxValueCouldBeInRange1And40, "WB0026", VerificationMessages.WB0026_RosterSizeQuestionMaxValueCouldBeInRange1And40),
                    Verifier<IQuestion>(PrefilledQuestionCantBeInsideOfRoster, "WB0030", VerificationMessages.WB0030_PrefilledQuestionCantBeInsideOfRoster),
                    Verifier<IGroup>(GroupWhereRosterSizeSourceIsQuestionHaveFixedTitles, "WB0032", VerificationMessages.WB0032_GroupWhereRosterSizeSourceIsQuestionHaveFixedTitles),
                    Verifier<IGroup>(GroupWhereRosterSizeSourceIsFixedTitlesHaveRosterSizeQuestion, "WB0033", VerificationMessages.WB0033_GroupWhereRosterSizeSourceIsFixedTitlesHaveRosterSizeQuestion),
                    Verifier<IGroup>(GroupWhereRosterSizeSourceIsFixedTitlesHaveRosterTitleQuestion, "WB0034", VerificationMessages.WB0034_GroupWhereRosterSizeSourceIsFixedTitlesHaveRosterTitleQuestion),
                    Verifier<IGroup>(GroupWhereRosterSizeSourceIsQuestionHasInvalidRosterTitleQuestion, "WB0035", VerificationMessages.WB0035_GroupWhereRosterSizeSourceIsQuestionHasInvalidRosterTitleQuestion),
                    Verifier<IGroup>(GroupWhereRosterSizeIsCategoricalMultyAnswerQuestionHaveRosterTitleQuestion, "WB0036", VerificationMessages.WB0036_GroupWhereRosterSizeIsCategoricalMultyAnswerQuestionHaveRosterTitleQuestion),
                    Verifier<IGroup>(GroupWhereRosterSizeSourceIsFixedTitlesHaveEmptyTitles, "WB0037", VerificationMessages.WB0037_GroupWhereRosterSizeSourceIsFixedTitlesHaveEmptyTitles),
                    Verifier<IGroup>(GroupWhereRosterSizeSourceIsFixedTitlesHaveDuplicateValues, "WB0041", VerificationMessages.WB0041_GroupWhereRosterSizeSourceIsFixedTitlesHaveDuplicateValues),
                    Verifier<IGroup>(RosterFixedTitlesHaveMoreThanAllowedItems, "WB0038", VerificationMessages.WB0038_RosterFixedTitlesHaveMoreThan40Items),
                    Verifier<ITextListQuestion>(TextListQuestionCannotBePrefilled, "WB0039", VerificationMessages.WB0039_TextListQuestionCannotBePrefilled),
                    Verifier<ITextListQuestion>(TextListQuestionCannotBeFilledBySupervisor, "WB0040", VerificationMessages.WB0040_TextListQuestionCannotBeFilledBySupervisor),
                    Verifier<ITextListQuestion>(TextListQuestionMaxAnswerNotInRange1And40, "WB0042", VerificationMessages.WB0042_TextListQuestionMaxAnswerInRange1And40),
                    Verifier<IQuestion>(QuestionHasOptionsWithEmptyValue, "WB0045", VerificationMessages.WB0045_QuestionHasOptionsWithEmptyValue),
                    Verifier<IQRBarcodeQuestion>(QRBarcodeQuestionIsSupervisorQuestion, "WB0049", VerificationMessages.WB0049_QRBarcodeQuestionIsSupervisorQuestion),
                    Verifier<IQRBarcodeQuestion>(QRBarcodeQuestionIsPreFilledQuestion, "WB0050", VerificationMessages.WB0050_QRBarcodeQuestionIsPreFilledQuestion),
                    Verifier<IGroup, IComposite>(RosterSizeQuestionHasDeeperRosterLevelThanDependentRoster, "WB0054", VerificationMessages.WB0054_RosterSizeQuestionHasDeeperRosterLevelThanDependentRoster),
                    Verifier<IGroup>(RosterHasRosterLevelMoreThan4, "WB0055", VerificationMessages.WB0055_RosterHasRosterLevelMoreThan4),
                    Verifier<IQuestion, IComposite>(this.QuestionShouldNotHaveCircularReferences, "WB0056", VerificationMessages.WB0056_QuestionShouldNotHaveCircularReferences),
                    Verifier<IQuestion>(QuestionHasEmptyVariableName, "WB0057", VerificationMessages.WB0057_QuestionHasEmptyVariableName),
                    Verifier<IQuestion>(QuestionHasInvalidVariableName, "WB0077", VerificationMessages.WB0077_QuestionHasInvalidVariableName),
                    Verifier<IQuestion>(QuestionHasVariableNameReservedForServiceNeeds, "WB0058", VerificationMessages.WB0058_QuestionHasVariableNameReservedForServiceNeeds),
                    Verifier<IQuestion>(CategoricalQuestionHasLessThan2Options, "WB0060", VerificationMessages.WB0060_CategoricalQuestionHasLessThan2Options),
                    Verifier<IMultyOptionsQuestion>(CategoricalMultiAnswersQuestionHasMaxAllowedAnswersLessThan2, "WB0061", VerificationMessages.WB0061_CategoricalMultiAnswersQuestionHasMaxAllowedAnswersLessThan2),
                    Verifier<IQuestion, IComposite>(this.CategoricalLinkedQuestionUsedInValidationExpression, "WB0063", VerificationMessages.WB0063_CategoricalLinkedQuestionUsedInValidationExpression),
                    Verifier<IQuestion, IComposite>(this.CategoricalLinkedQuestionUsedInQuestionEnablementCondition, "WB0064", VerificationMessages.WB0064_CategoricalLinkedQuestionUsedInEnablementCondition),
                    Verifier<IGroup, IComposite>(this.CategoricalLinkedQuestionUsedInGroupEnablementCondition, "WB0064", VerificationMessages.WB0064_CategoricalLinkedQuestionUsedInEnablementCondition),
                    Verifier<IQuestion>(QuestionHasValidationExpressionWithoutValidationMessage, "WB0065", VerificationMessages.WB0065_QuestionHasValidationExpressionWithoutValidationMessage),
                    Verifier<IQuestion>(QuestionTypeIsNotAllowed, "WB0066", VerificationMessages.WB0066_QuestionTypeIsNotAllowed),
                    Verifier<IGroup>(RosterHasEmptyVariableName, "WB0067", VerificationMessages.WB0067_RosterHasEmptyVariableName),
                    Verifier<IGroup>(RosterHasInvalidVariableName, "WB0069", VerificationMessages.WB0069_RosterHasInvalidVariableName),
                    Verifier<IGroup>(RosterHasVariableNameEqualToQuestionnaireTitle, "WB0070", VerificationMessages.WB0070_RosterHasVariableNameEqualToQuestionnaireTitle),
                    Verifier<IGroup>(RosterHasVariableNameReservedForServiceNeeds, "WB0058", VerificationMessages.WB0058_QuestionHasVariableNameReservedForServiceNeeds),
                    Verifier<IStaticText>(StaticTextIsEmpty, "WB0071", VerificationMessages.WB0071_StaticTextIsEmpty),
                    Verifier<IQuestion>(OptionTitlesMustBeUniqueForCategoricalQuestion, "WB0072", VerificationMessages.WB0072_OptionTitlesMustBeUniqueForCategoricalQuestion),
                    Verifier<IQuestion>(OptionValuesMustBeUniqueForCategoricalQuestion, "WB0073", VerificationMessages.WB0073_OptionValuesMustBeUniqueForCategoricalQuestion),
                    Verifier<IQuestion>(FilteredComboboxIsLinked, "WB0074", VerificationMessages.WB0074_FilteredComboboxIsLinked),
                    Verifier<IQuestion>(FilteredComboboxContainsMoreThan5000Options, "WB0075", VerificationMessages.WB0075_FilteredComboboxContainsMoreThan5000Options),
                    Verifier<IQuestion>(CategoricalOneAnswerOptionsCountMoreThanMaxOptionCount, "WB0076", VerificationMessages.WB0076_CategoricalOneAnswerOptionsCountMoreThan200),
                    Verifier<IMultimediaQuestion>(MultimediaQuestionIsInterviewersOnly, "WB0078", VerificationMessages.WB0078_MultimediaQuestionIsInterviewersOnly),
                    Verifier<IMultimediaQuestion>(MultimediaShouldNotHaveValidationExpression, "WB0079", VerificationMessages.WB0079_MultimediaShouldNotHaveValidationExpression),
                    Verifier<IQuestion, IComposite>(MultimediaQuestionsCannotBeUsedInValidationExpression, "WB0080", VerificationMessages.WB0080_MultimediaQuestionsCannotBeUsedInValidationExpression),
                    Verifier<IGroup, IComposite>(MultimediaQuestionsCannotBeUsedInGroupEnablementCondition, "WB0081", VerificationMessages.WB0081_MultimediaQuestionsCannotBeUsedInGroupEnablementCondition),
                    Verifier<IQuestion, IComposite>(MultimediaQuestionsCannotBeUsedInQuestionEnablementCondition, "WB0082", VerificationMessages.WB0082_MultimediaQuestionsCannotBeUsedInQuestionEnablementCondition),
                    Verifier<IGroup, IComposite>(QuestionsCannotBeUsedAsRosterTitle, "WB0083", VerificationMessages.WB0083_QuestionCannotBeUsedAsRosterTitle),
                    Verifier<IQuestion, IComposite>(CascadingComboboxHasNoParentOptions, "WB0084", VerificationMessages.WB0084_CascadingOptionsShouldHaveParent),
                    Verifier<IQuestion, IComposite>(ParentShouldNotHaveDeeperRosterLevelThanCascadingQuestion, "WB0085", VerificationMessages.WB0085_CascadingQuestionWrongParentLevel),
                    Verifier<IQuestion>(CascadingQuestionReferencesMissingParent, "WB0086", VerificationMessages.WB0086_ParentCascadingQuestionShouldExist),
                    Verifier<SingleQuestion, SingleQuestion>(CascadingHasCircularReference, "WB0087", VerificationMessages.WB0087_CascadingQuestionHasCicularReference),
                    Verifier<SingleQuestion>(CascadingQuestionHasMoreThanAllowedOptions, "WB0088", VerificationMessages.WB0088_CascadingQuestionShouldHaveAllowedAmountOfAnswers),
                    Verifier<SingleQuestion>(CascadingQuestionOptionsWithParentValuesShouldBeUnique, "WB0089", VerificationMessages.WB0089_CascadingQuestionOptionWithParentShouldBeUnique),
                    Verifier<IQuestion>(LinkedQuestionIsInterviewersOnly, "WB0090", VerificationMessages.WB0090_LinkedQuestionIsInterviewersOnly),
                    Verifier<SingleQuestion>(CascadingQuestionHasEnablementCondition, "WB0091", VerificationMessages.WB0091_CascadingChildQuestionShouldNotContainCondition),
                    Verifier<SingleQuestion>(CascadingQuestionHasValidationExpresssion, "WB0092", VerificationMessages.WB0092_CascadingChildQuesionShouldNotContainValidation),
                    Verifier<IComposite>(ConditionExpresssionHasLengthMoreThan10000Characters, "WB0094", VerificationMessages.WB0094_ConditionExpresssionHasLengthMoreThan10000Characters),
                    Verifier<IQuestion>(ValidationExpresssionHasLengthMoreThan10000Characters, "WB0095", VerificationMessages.WB0095_ValidationExpresssionHasLengthMoreThan10000Characters),
                    Verifier(QuestionnaireTitleHasInvalidCharacters, "WB0097", VerificationMessages.WB0097_QuestionnaireTitleHasInvalidCharacters),
                    Verifier(QuestionnaireHasSizeMoreThan5MB, "WB0098", size => VerificationMessages.WB0098_QuestionnaireHasSizeMoreThan5MB.FormatString(size)),

                    this.ErrorsByQuestionsWithCustomConditionReferencingQuestionsWithDeeperRosterLevel,
                    ErrorsByLinkedQuestions,
                    ErrorsByQuestionsWithSubstitutions,
                    ErrorsByQuestionsWithDuplicateVariableName,
                    ErrorsByRostersWithDuplicateVariableName,
                    ErrorsByConditionAndValidationExpressions,
                };
            }
        }

        private static bool ValidationExpresssionHasLengthMoreThan10000Characters(IQuestion question, VerificationState state)
        {
            if (string.IsNullOrEmpty(question.ValidationExpression))
                return false;

            var exceeded = question.ValidationExpression.Length > MaxExpressionLength;

            state.HasExceededLimitByValidationExpresssionCharactersLength |= exceeded;

            return exceeded;
        }

        private static bool ConditionExpresssionHasLengthMoreThan10000Characters(IComposite groupOrQuestion, VerificationState state)
        {
            var customEnablementCondition = GetCustomEnablementCondition(groupOrQuestion);
            
            if (string.IsNullOrEmpty(customEnablementCondition))
                return false;

            var exceeded = customEnablementCondition.Length > MaxExpressionLength;

            state.HasExceededLimitByConditionExpresssionCharactersLength |= exceeded;

            return exceeded;
        }

        private static bool CascadingQuestionHasValidationExpresssion(SingleQuestion question)
        {
            return question.CascadeFromQuestionId.HasValue && !string.IsNullOrWhiteSpace(question.ValidationExpression);
        }

        private static bool CascadingQuestionHasEnablementCondition(SingleQuestion question)
        {
            return question.CascadeFromQuestionId.HasValue && !string.IsNullOrWhiteSpace(question.ConditionExpression);
        }
       
        private IEnumerable<QuestionnaireVerificationError> ErrorsByConditionAndValidationExpressions(
            QuestionnaireDocument questionnaire, VerificationState state)
        {
            if (state.HasExceededLimitByConditionExpresssionCharactersLength || state.HasExceededLimitByValidationExpresssionCharactersLength)
                yield break;

            var compilationResult = GetCompilationResult(questionnaire);

            if (compilationResult.Success)
                yield break;

            foreach (var locationOfExpressionError in compilationResult.Diagnostics.Select(x => x.Location).Distinct())
            {
                yield return CreateExpressionSyntaxError(new ExpressionLocation(locationOfExpressionError));
            }
        } 

        private static Tuple<bool, decimal> QuestionnaireHasSizeMoreThan5MB(QuestionnaireDocument questionnaire)
        {
            var jsonQuestionnaire = JsonConvert.SerializeObject(questionnaire, Formatting.None);
            var questionnaireByteCount = Encoding.UTF8.GetByteCount(jsonQuestionnaire);
            var isOversized = questionnaireByteCount > 5 * 1024 * 1024; // 5MB
            var questionnaireMegaByteCount = (decimal)questionnaireByteCount / 1024 / 1024;
            return new Tuple<bool, decimal>(isOversized, questionnaireMegaByteCount) ;
        }
        
        private GenerationResult GetCompilationResult(QuestionnaireDocument questionnaire)
        {
            string resultAssembly;

            return this.expressionProcessorGenerator.GenerateProcessorStateAssembly(questionnaire, out resultAssembly);
        }

        private static bool CascadingQuestionOptionsWithParentValuesShouldBeUnique(SingleQuestion question)
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

        private static bool CascadingQuestionHasMoreThanAllowedOptions(SingleQuestion question)
        {
            return question.CascadeFromQuestionId.HasValue && question.Answers != null && question.Answers.Count > 10000;
        }

        private static EntityVerificationResult<SingleQuestion> CascadingHasCircularReference(SingleQuestion question, QuestionnaireDocument questionnaire)
        {
            if (!question.CascadeFromQuestionId.HasValue)
                return new EntityVerificationResult<SingleQuestion> { HasErrors = false };

            var referencedEntities = new HashSet<SingleQuestion>();

            Func<SingleQuestion, SingleQuestion> getParentCascadingQuestion = x => questionnaire.Find<SingleQuestion>(q =>
                q.CascadeFromQuestionId.HasValue && q.PublicKey == x.CascadeFromQuestionId.Value)
                .SingleOrDefault();
            var cascadingAncestors = question.UnwrapReferences(getParentCascadingQuestion);

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

            return new EntityVerificationResult<SingleQuestion> { HasErrors = false };
        }

        private static bool CascadingQuestionReferencesMissingParent(IQuestion question, QuestionnaireDocument questionnaire)
        {
            if (!question.CascadeFromQuestionId.HasValue)
                return false;

            return questionnaire.Find<SingleQuestion>(question.CascadeFromQuestionId.Value) == null;
        }

        private static EntityVerificationResult<IComposite> ParentShouldNotHaveDeeperRosterLevelThanCascadingQuestion(IQuestion question, QuestionnaireDocument questionnaire)
        {
            if (!question.CascadeFromQuestionId.HasValue)
                return new EntityVerificationResult<IComposite> { HasErrors = false };

            var parentQuestion = questionnaire.Find<IQuestion>(question.CascadeFromQuestionId.Value);
            if (parentQuestion == null)
                return new EntityVerificationResult<IComposite> { HasErrors = false };

            var parentRosters = GetAllRosterSizeQuestionsAsVectorOrNullIfSomeAreMissing(parentQuestion, questionnaire);

            if (parentRosters == null)
                return new EntityVerificationResult<IComposite> { HasErrors = false };

            parentRosters = parentRosters.Reverse().ToArray();

            var questionRosters = GetAllRosterSizeQuestionsAsVectorOrNullIfSomeAreMissing(question, questionnaire).Reverse().ToArray();

            if (parentRosters.Length > questionRosters.Length || parentRosters.Where((parentGuid, i) => questionRosters[i] != parentGuid).Any())
            {
                return new EntityVerificationResult<IComposite>
                {
                    HasErrors = true,
                    ReferencedEntities = new IComposite[] { question, parentQuestion },
                };
            }

            return new EntityVerificationResult<IComposite> { HasErrors = false };
        }

        private static EntityVerificationResult<IComposite> CascadingComboboxHasNoParentOptions(IQuestion question, QuestionnaireDocument document)
        {
            if (!question.CascadeFromQuestionId.HasValue)
                return new EntityVerificationResult<IComposite> { HasErrors = false };

            var parentQuestion = document.Find<SingleQuestion>(question.CascadeFromQuestionId.Value);
            if (parentQuestion == null)
                return new EntityVerificationResult<IComposite> { HasErrors = false };

            var result = !question.Answers.All(childAnswer =>
                parentQuestion.Answers.Any(
                    parentAnswer => parentAnswer.AnswerValue == childAnswer.ParentValue));

            return new EntityVerificationResult<IComposite>()
            {
                HasErrors = result,
                ReferencedEntities = new List<IComposite> { question, parentQuestion }
            };
        }

        private static bool CategoricalOneAnswerOptionsCountMoreThanMaxOptionCount(IQuestion question)
        {
            return !question.CascadeFromQuestionId.HasValue && IsCategoricalSingleAnswerQuestion(question) && !IsFilteredComboboxQuestion(question) &&
                   question.Answers != null && question.Answers.Count > 200;
        }

        private static bool FilteredComboboxContainsMoreThan5000Options(IQuestion question)
        {
            return IsFilteredComboboxQuestion(question) && question.Answers != null && question.Answers.Count > 5000;
        }

        private static bool FilteredComboboxIsLinked(IQuestion question)
        {
            return IsFilteredComboboxQuestion(question) && question.LinkedToQuestionId.HasValue;
        }

        private static bool StaticTextIsEmpty(IStaticText staticText)
        {
            return string.IsNullOrWhiteSpace(staticText.Text);
        }

        private static bool QuestionTypeIsNotAllowed(IQuestion question)
        {
            return !WhiteListOfQuestionTypes.Contains(question.QuestionType);
        }

        private static bool OptionTitlesMustBeUniqueForCategoricalQuestion(IQuestion question)
        {
            if (question.Answers != null && !question.CascadeFromQuestionId.HasValue)
            {
                return question.Answers.Where(x => x.AnswerText != null).Select(x => x.AnswerText.Trim()).Distinct().Count() != question.Answers.Count;
            }
            return false;
        }

        private static bool OptionValuesMustBeUniqueForCategoricalQuestion(IQuestion question)
        {
            if (question.Answers != null)
            {
                return question.Answers.Where(x => x.AnswerValue != null).Select(x => x.AnswerValue.Trim()).Distinct().Count() != question.Answers.Count;
            }
            return false;
        }

        private static bool CategoricalMultiAnswersQuestionHasMaxAllowedAnswersLessThan2(IMultyOptionsQuestion question)
        {
            return question.MaxAllowedAnswers.HasValue && question.MaxAllowedAnswers < 2;
        }

        private static bool CategoricalQuestionHasLessThan2Options(IQuestion question)
        {
            if (!IsCategoricalSingleAnswerQuestion(question) && !IsCategoricalMultiAnswersQuestion(question))
                return false;

            if (question.LinkedToQuestionId.HasValue)
                return false;

            return question.Answers == null || question.Answers.Count < 2;
        }

        private bool QuestionHasVariableNameReservedForServiceNeeds(IQuestion question)
        {
            return keywordsProvider.GetAllReservedKeywords().Contains(question.StataExportCaption);
        }

        private bool RosterHasVariableNameReservedForServiceNeeds(IGroup roster)
        {
            if (!IsRosterGroup(roster))
                return false;

            var keywords = keywordsProvider.GetAllReservedKeywords();
            return keywords.Contains(roster.VariableName);
        }

        public IEnumerable<QuestionnaireVerificationError> Verify(QuestionnaireDocument questionnaire)
        {
            questionnaire.ConnectChildrenWithParent();

            var state = new VerificationState();

            return
                from verifier in this.AtomicVerifiers
                let errors = verifier.Invoke(questionnaire, state)
                from error in errors
                select error;
        }

        private static Func<QuestionnaireDocument, VerificationState, IEnumerable<QuestionnaireVerificationError>> Verifier(
            Func<QuestionnaireDocument, bool> hasError, string code, string message)
        {
            return (questionnaire, state) =>
                hasError(questionnaire)
                    ? new[] { new QuestionnaireVerificationError(code, message) }
                    : Enumerable.Empty<QuestionnaireVerificationError>();
        }

        private static Func<QuestionnaireDocument, VerificationState, IEnumerable<QuestionnaireVerificationError>> Verifier<TArg>(
            Func<QuestionnaireDocument, Tuple<bool, TArg>> hasError, string code, Func<TArg, string> messageBuilder)
        {
            return (questionnaire, state) =>
            {
                var errorCheckResult = hasError(questionnaire);
                return errorCheckResult.Item1
                    ? new[] { new QuestionnaireVerificationError(code, messageBuilder.Invoke(errorCheckResult.Item2)) }
                    : Enumerable.Empty<QuestionnaireVerificationError>();
            };
        }

        private static Func<QuestionnaireDocument, VerificationState, IEnumerable<QuestionnaireVerificationError>> Verifier<TEntity>(
            Func<TEntity, bool> hasError, string code, string message)
            where TEntity : class, IComposite
        {
            return (questionnaire, state) =>
                questionnaire
                    .Find<TEntity>(hasError)
                    .Select(entity => new QuestionnaireVerificationError(code, message, CreateReference(entity)));
        }

        private static Func<QuestionnaireDocument, VerificationState, IEnumerable<QuestionnaireVerificationError>> Verifier<TEntity>(
            Func<TEntity, VerificationState, bool> hasError, string code, string message)
            where TEntity : class, IComposite
        {
            return (questionnaire, state) =>
                questionnaire
                    .Find<TEntity>(entity => hasError(entity, state))
                    .Select(entity => new QuestionnaireVerificationError(code, message, CreateReference(entity)));
        }

        private static Func<QuestionnaireDocument, VerificationState, IEnumerable<QuestionnaireVerificationError>> Verifier<TEntity>(
            Func<TEntity, QuestionnaireDocument, bool> hasError, string code, string message)
            where TEntity : class, IComposite
        {
            return (questionnaire, state) =>
                questionnaire
                    .Find<TEntity>(entity => hasError(entity, questionnaire))
                    .Select(entity => new QuestionnaireVerificationError(code, message, CreateReference(entity)));
        }

        private static Func<QuestionnaireDocument, VerificationState, IEnumerable<QuestionnaireVerificationError>> Verifier<TEntity, TReferencedEntity>(
            Func<TEntity, QuestionnaireDocument, EntityVerificationResult<TReferencedEntity>> verifyEntity, string code, string message)
            where TEntity : class, IComposite
            where TReferencedEntity : class, IComposite
        {
            return (questionnaire, state) =>
                from entity in questionnaire.Find<TEntity>(_ => true)
                let verificationResult = verifyEntity(entity, questionnaire)
                where verificationResult.HasErrors
                select new QuestionnaireVerificationError(code, message, verificationResult.ReferencedEntities.Select(CreateReference));
        }

        private static bool CategoricalMultiAnswersQuestionHasOptionsCountLessThanMaxAllowedAnswersCount(IMultyOptionsQuestion question)
        {
            return question.MaxAllowedAnswers.HasValue && !question.LinkedToQuestionId.HasValue &&
                   (question.MaxAllowedAnswers.Value > question.Answers.Count);
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
            return IsRosterByQuestion(group) && group.FixedRosterTitles.Any();
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

            Guid[] rosterScopeForGroup = GetAllRosterSizeQuestionsAsVectorOrNullIfSomeAreMissing(group, questionnaire);
            Guid[] rosterScopeForTitleQuestion = GetAllRosterSizeQuestionsAsVectorOrNullIfSomeAreMissing(rosterTitleQuestion, questionnaire);

            if (!Enumerable.SequenceEqual(rosterScopeForGroup, rosterScopeForTitleQuestion))
                return true;

            return false;
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
            if (group.FixedRosterTitles == null)
                return false;
            if (group.FixedRosterTitles.Length == 0)
                return false;

            return group.FixedRosterTitles.Any(title => string.IsNullOrWhiteSpace(title.Title));
        }

        private static bool GroupWhereRosterSizeSourceIsFixedTitlesHaveDuplicateValues(IGroup group)
        {
            if (!IsRosterByFixedTitles(group))
                return false;
            if (group.FixedRosterTitles == null)
                return false;
            if (group.FixedRosterTitles.Length == 0)
                return false;
            return group.FixedRosterTitles.Select(x => x.Value).Distinct().Count() != group.FixedRosterTitles.Length;
        }

        private static bool RosterFixedTitlesHaveMoreThanAllowedItems(IGroup group)
        {
            if (!IsRosterByFixedTitles(group))
                return false;

            return group.FixedRosterTitles.Length > 40;
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

        private static bool RosterHasRosterLevelMoreThan4(IGroup roster)
        {
            if (!IsRosterGroup(roster))
                return false;

            return GetRosterLevel(roster) > 4;
        }

        private static int GetRosterLevel(IComposite questionnaireItem)
        {
            int rosterLevel = 0;
            while (questionnaireItem != null)
            {
                if (IsGroup(questionnaireItem) && IsRosterGroup((IGroup)questionnaireItem))
                    rosterLevel++;

                questionnaireItem = questionnaireItem.GetParent();
            }

            return rosterLevel;
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
            return IsCategoricalMultiAnswersQuestion(question) && !question.LinkedToQuestionId.HasValue;
        }

        private static bool IsCategoricalMultiAnswersQuestion(IQuestion question)
        {
            return question is MultyOptionsQuestion;
        }

        private static bool IsCategoricalSingleAnswerQuestion(IQuestion question)
        {
            return question is SingleQuestion;
        }

        private static bool PrefilledQuestionCantBeInsideOfRoster(IQuestion question, QuestionnaireDocument questionnaire)
        {
            return IsPreFilledQuestion(question) && GetAllParentGroupsForQuestion(question, questionnaire).Any(IsRosterGroup);
        }

        private static bool TextListQuestionCannotBePrefilled(ITextListQuestion question)
        {
            return IsPreFilledQuestion(question);
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

        private static bool QuestionnaireTitleHasInvalidCharacters(QuestionnaireDocument questionnaire)
        {
            if (string.IsNullOrWhiteSpace(questionnaire.Title))
            {
                return false;
            }

            return !QuestionnaireNameRegex.IsMatch(questionnaire.Title);
        }

        private static bool QuestionHasInvalidVariableName(IQuestion arg)
        {
            return !IsVariableNameValid(arg.StataExportCaption);
        }

        private static bool IsVariableNameValid(string variableName)
        {
            if (string.IsNullOrEmpty(variableName))
                return true;

            if (variableName.Length > 32)
                return false;
            return VariableNameRegex.IsMatch(variableName);
        }

        private static bool RosterHasEmptyVariableName(IGroup group)
        {
            if (!group.IsRoster)
                return false;
            return string.IsNullOrWhiteSpace(group.VariableName);
        }

        private static bool QuestionHasEmptyVariableName(IQuestion question)
        {
            return string.IsNullOrEmpty(question.StataExportCaption);
        }

        private static bool RosterHasInvalidVariableName(IGroup group)
        {
            if (!group.IsRoster)
                return false;
            return !IsVariableNameValid(group.VariableName);
        }

        private bool RosterHasVariableNameEqualToQuestionnaireTitle(IGroup group, QuestionnaireDocument questionnaire)
        {
            if (!group.IsRoster)
                return false;

            if (string.IsNullOrEmpty(group.VariableName))
                return false;

            var questionnaireVariableName = this.fileSystemAccessor.MakeValidFileName(questionnaire.Title);

            return group.VariableName.Equals(questionnaireVariableName, StringComparison.InvariantCultureIgnoreCase);
        }

        private EntityVerificationResult<IComposite> QuestionShouldNotHaveCircularReferences(IQuestion entity, QuestionnaireDocument questionnaire)
        {
            if (String.IsNullOrEmpty(entity.ConditionExpression))
                return new EntityVerificationResult<IComposite> { HasErrors = false };

            var referencedEntities = new List<IComposite>();

            var referencedQuestion = this.ReferencedQuestion(entity.ConditionExpression, questionnaire);

            foreach (var question in referencedQuestion)
            {
                if (String.IsNullOrWhiteSpace(question.ConditionExpression))
                {
                    continue;
                }
                var internalReferencedQuestion = this.ReferencedQuestion(question.ConditionExpression, questionnaire);

                foreach (var internalQuestion in internalReferencedQuestion)
                {
                    if (internalQuestion.PublicKey == entity.PublicKey)
                    {
                        referencedEntities.Add(entity);
                        if (entity.PublicKey != question.PublicKey)
                        {
                            referencedEntities.Add(question);
                        }
                    }
                }
            }

            if (referencedEntities.Any())
            {
                return new EntityVerificationResult<IComposite>
                {
                    HasErrors = true,
                    ReferencedEntities = referencedEntities
                };
            }

            return new EntityVerificationResult<IComposite> { HasErrors = false };
        }

        private static bool TextListQuestionCannotBeFilledBySupervisor(ITextListQuestion question)
        {
            return IsSupervisorQuestion(question);
        }

        private static bool QRBarcodeQuestionIsPreFilledQuestion(IQRBarcodeQuestion question)
        {
            return IsPreFilledQuestion(question);
        }

        private static bool LinkedQuestionIsInterviewersOnly(IQuestion question)
        {
            if (!question.LinkedToQuestionId.HasValue)
                return false;

            return question.QuestionScope != QuestionScope.Interviewer || IsPreFilledQuestion(question);
        }

        private static bool MultimediaQuestionIsInterviewersOnly(IMultimediaQuestion question)
        {
            return question.QuestionScope != QuestionScope.Interviewer || IsPreFilledQuestion(question);
        }

        private static bool QRBarcodeQuestionIsSupervisorQuestion(IQRBarcodeQuestion question)
        {
            return IsSupervisorQuestion(question);
        }

        private static bool MultimediaShouldNotHaveValidationExpression(IMultimediaQuestion question)
        {
            return !string.IsNullOrEmpty(question.ValidationExpression);
        }

        private EntityVerificationResult<IComposite> MultimediaQuestionsCannotBeUsedInValidationExpression(IQuestion question, QuestionnaireDocument questionnaire)
        {
            return this.VerifyWhetherEntityExpressionReferencesIncorrectQuestions(
                question, question.ValidationExpression, questionnaire,
                isReferencedQuestionIncorrect: referencedQuestion => referencedQuestion.QuestionType == QuestionType.Multimedia);
        }

        private EntityVerificationResult<IComposite> MultimediaQuestionsCannotBeUsedInQuestionEnablementCondition(IQuestion question, QuestionnaireDocument questionnaire)
        {
            return this.VerifyWhetherEntityExpressionReferencesIncorrectQuestions(
               question, question.ConditionExpression, questionnaire,
               isReferencedQuestionIncorrect: referencedQuestion => referencedQuestion.QuestionType == QuestionType.Multimedia);
        }

        private static EntityVerificationResult<IComposite> QuestionsCannotBeUsedAsRosterTitle(IGroup group, QuestionnaireDocument questionnaire)
        {
            var noErrors=new EntityVerificationResult<IComposite> { HasErrors = false };

            if (!group.RosterTitleQuestionId.HasValue)
                return noErrors;

            var rosterTitleQuestion = questionnaire.FirstOrDefault<IQuestion>(q => q.PublicKey == group.RosterTitleQuestionId.Value);
            if (rosterTitleQuestion == null)
                return noErrors;

            if (QuestionTypesValidToBeRosterTitles.Contains(rosterTitleQuestion.QuestionType))
                return noErrors;

            return new EntityVerificationResult<IComposite>()
            {
                HasErrors = true,
                ReferencedEntities = new IComposite[] { group, rosterTitleQuestion }
            };
        }

        private EntityVerificationResult<IComposite> MultimediaQuestionsCannotBeUsedInGroupEnablementCondition(IGroup group, QuestionnaireDocument questionnaire)
        {
            return this.VerifyWhetherEntityExpressionReferencesIncorrectQuestions(
               group, group.ConditionExpression, questionnaire,
               isReferencedQuestionIncorrect: referencedQuestion => referencedQuestion.QuestionType == QuestionType.Multimedia);
        }

        private EntityVerificationResult<IComposite> CategoricalLinkedQuestionUsedInValidationExpression(IQuestion question, QuestionnaireDocument questionnaire)
        {
            return this.VerifyWhetherEntityExpressionReferencesIncorrectQuestions(question, question.ValidationExpression,
                questionnaire, isReferencedQuestionIncorrect: IsCategoricalLinkedQuestion);
        }

        private EntityVerificationResult<IComposite> CategoricalLinkedQuestionUsedInQuestionEnablementCondition(IQuestion question, QuestionnaireDocument questionnaire)
        {
            return this.VerifyWhetherEntityExpressionReferencesIncorrectQuestions(question, question.ConditionExpression,
                questionnaire, isReferencedQuestionIncorrect: IsCategoricalLinkedQuestion);
        }

        private EntityVerificationResult<IComposite> CategoricalLinkedQuestionUsedInGroupEnablementCondition(IGroup group, QuestionnaireDocument questionnaire)
        {
            return this.VerifyWhetherEntityExpressionReferencesIncorrectQuestions(group, group.ConditionExpression,
                questionnaire, isReferencedQuestionIncorrect: IsCategoricalLinkedQuestion);
        }


        private EntityVerificationResult<IComposite> VerifyWhetherEntityExpressionReferencesIncorrectQuestions(
            IComposite entity, string expression, QuestionnaireDocument questionnaire, Func<IQuestion, bool> isReferencedQuestionIncorrect)
        {
            if (string.IsNullOrEmpty(expression))
                return new EntityVerificationResult<IComposite> { HasErrors = false };

            IEnumerable<IQuestion> incorrectReferencedQuestions = this.expressionProcessor
                .GetIdentifiersUsedInExpression(expression)
                .Select(identifier => GetQuestionByIdentifier(identifier, questionnaire))
                .Where(referencedQuestion => referencedQuestion != null)
                .Where(isReferencedQuestionIncorrect)
                .ToList();

            if (!incorrectReferencedQuestions.Any())
                return new EntityVerificationResult<IComposite> { HasErrors = false };

            return new EntityVerificationResult<IComposite>
            {
                HasErrors = true,
                ReferencedEntities = Enumerable.Concat(entity.ToEnumerable(), incorrectReferencedQuestions),
            };
        }

        private static EntityVerificationResult<IComposite> RosterSizeQuestionHasDeeperRosterLevelThanDependentRoster(IGroup roster, QuestionnaireDocument questionnaire)
        {
            if (!IsRosterGroup(roster))
                return new EntityVerificationResult<IComposite> { HasErrors = false };

            var rosterSizeQuestion = GetRosterSizeQuestionByRosterGroup(roster, questionnaire);
            if (rosterSizeQuestion == null)
                return new EntityVerificationResult<IComposite> { HasErrors = false };

            var rosterLevelForRoster = GetAllRosterSizeQuestionsAsVectorOrNullIfSomeAreMissing(roster, questionnaire);
            if (rosterLevelForRoster == null)
                return new EntityVerificationResult<IComposite> { HasErrors = false };

            var rosterLevelWithoutOwnRosterSizeQuestion = rosterLevelForRoster;
            if (rosterLevelWithoutOwnRosterSizeQuestion.Length > 0)
            {
                rosterLevelWithoutOwnRosterSizeQuestion = rosterLevelForRoster.Skip(1).ToArray();
            }

            if (QuestionHasDeeperRosterLevelThenVectorOfRosterQuestions(rosterSizeQuestion, rosterLevelWithoutOwnRosterSizeQuestion, questionnaire))
            {
                return new EntityVerificationResult<IComposite>
                {
                    HasErrors = true,
                    ReferencedEntities = new IComposite[] { roster, rosterSizeQuestion },
                };
            }

            return new EntityVerificationResult<IComposite> { HasErrors = false };
        }

        private static IEnumerable<QuestionnaireVerificationError> ErrorsByLinkedQuestions(
            QuestionnaireDocument questionnaire, VerificationState state)
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

        private static IEnumerable<QuestionnaireVerificationError> ErrorsByQuestionsWithDuplicateVariableName(
            QuestionnaireDocument questionnaire, VerificationState state)
        {
            var questionsDuplicates = questionnaire.Find<IQuestion>(q => true)
                .Where(x => !string.IsNullOrEmpty(x.StataExportCaption))
                .GroupBy(s => s.StataExportCaption, StringComparer.InvariantCultureIgnoreCase)
                .SelectMany(group => group.Skip(1));

            foreach (IQuestion questionsDuplicate in questionsDuplicates)
                yield return VariableNameIsUsedAsOtherQuestionVariableName(questionsDuplicate);
        }

        private static IEnumerable<QuestionnaireVerificationError> ErrorsByRostersWithDuplicateVariableName(
            QuestionnaireDocument questionnaire, VerificationState state)
        {
            var rosterVariableNameMappedOnRosters = questionnaire.Find<IGroup>(g => g.IsRoster && !string.IsNullOrEmpty(g.VariableName))
                .GroupBy(s => s.VariableName, StringComparer.InvariantCultureIgnoreCase).ToDictionary(r => r.Key, r => r.ToArray());

            var questionsVariableNamesMappedOnQuestions = questionnaire.Find<IQuestion>(q => true)
             .Where(x => !string.IsNullOrEmpty(x.StataExportCaption))
             .GroupBy(s => s.StataExportCaption, StringComparer.InvariantCultureIgnoreCase)
             .ToDictionary(r => r.Key, r => r.ToArray());

            foreach (var rosterVariableNameMappedOnRoster in rosterVariableNameMappedOnRosters)
            {
                if (questionsVariableNamesMappedOnQuestions.ContainsKey(rosterVariableNameMappedOnRoster.Key))
                    yield return
                        new QuestionnaireVerificationError("WB0093",
                            VerificationMessages.WB0093_QuestionWithTheSameVariableNameAlreadyExists,
                            CreateReference(rosterVariableNameMappedOnRoster.Value.First()),
                            CreateReference(questionsVariableNamesMappedOnQuestions[rosterVariableNameMappedOnRoster.Key].First()));

                if (rosterVariableNameMappedOnRoster.Value.Length < 2)
                    continue;

                yield return new QuestionnaireVerificationError("WB0068",
                    VerificationMessages.WB0068_RosterHasNotUniqueVariableName,
                    rosterVariableNameMappedOnRoster.Value.Select(CreateReference).ToArray());
            }

        }

        private IEnumerable<QuestionnaireVerificationError> ErrorsByQuestionsWithSubstitutions(
            QuestionnaireDocument questionnaire, VerificationState state)
        {
            IEnumerable<IQuestion> questionsWithSubstitutions =
                questionnaire.Find<IQuestion>(question => substitutionService.GetAllSubstitutionVariableNames(question.QuestionText).Length > 0);

            var errorByAllQuestionsWithSubstitutions = new List<QuestionnaireVerificationError>();

            foreach (var questionWithSubstitution in questionsWithSubstitutions)
            {
                if (IsPreFilledQuestion(questionWithSubstitution))
                {
                    errorByAllQuestionsWithSubstitutions.Add(QuestionWithTitleSubstitutionCantBePrefilled(questionWithSubstitution));
                    continue;
                }

                var substitutionReferences = substitutionService.GetAllSubstitutionVariableNames(questionWithSubstitution.QuestionText);

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

        private IEnumerable<IQuestion> ReferencedQuestion(string expression, QuestionnaireDocument questionnaire)
        {
            IEnumerable<IQuestion> incorrectReferencedQuestions = this.expressionProcessor
                .GetIdentifiersUsedInExpression(expression)
                .Select(identifier => GetQuestionByIdentifier(identifier, questionnaire))
                .Where(referencedQuestion => referencedQuestion != null)
                .ToList();
            return incorrectReferencedQuestions;
        }

        private IEnumerable<QuestionnaireVerificationError> ErrorsByQuestionsWithCustomConditionReferencingQuestionsWithDeeperRosterLevel(
            QuestionnaireDocument questionnaire, VerificationState state)
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
                     identifier => GetVerificationErrorByConditionsInGroupsReferencedChildQuestionsOrNull(
                         itemWithConditionExpression, identifier, questionnaire));
                }
            }

            return errorByAllItemsWithCustomCondition;
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
                entity is IGroup
                    ? QuestionnaireVerificationReferenceType.Group
                    : (entity is IStaticText
                        ? QuestionnaireVerificationReferenceType.StaticText
                        : QuestionnaireVerificationReferenceType.Question),
                entity.PublicKey);
        }

        private static bool QuestionHasValidationExpressionWithoutValidationMessage(IQuestion question)
        {
            if (string.IsNullOrWhiteSpace(question.ValidationExpression))
                return false;

            return string.IsNullOrWhiteSpace(question.ValidationMessage);
        }
        
        private static bool CategoricalMultianswerQuestionIsFeatured(IMultyOptionsQuestion question, QuestionnaireDocument questionnaire)
        {
            return IsPreFilledQuestion(question);
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

        private static QuestionnaireVerificationError GetVerificationErrorByConditionsInGroupsReferencedChildQuestionsOrNull(
            IComposite itemWithExpression, string identifier, QuestionnaireDocument questionnaire)
        {
            if (itemWithExpression is IQuestion)
            {
                return null;
            }

            if (IsSpecialThisIdentifier(identifier))
            {
                return null;
            }

            IQuestion questionsReferencedInExpression = GetQuestionByIdentifier(identifier, questionnaire);

            if (questionsReferencedInExpression == null)
            {
                return null;
            }

            var parentIds = GetSpecifiedGroupAndAllItsParentGroupsStartingFromBottom((IGroup)questionsReferencedInExpression.GetParent(), questionnaire)
                .Select(x => x.PublicKey)
                .ToList();

            if (parentIds.Contains(itemWithExpression.PublicKey))
            {
                return new QuestionnaireVerificationError("WB0051",
                    VerificationMessages.WB0051_GroupsCustomConditionExpressionReferencesChildQuestion,
                    CreateReference(itemWithExpression),
                    CreateReference(questionsReferencedInExpression));
            }

            return null;
        }

        private static IGroup GetRosterByIdentifier(string identifier, QuestionnaireDocument questionnaire)
        {
            Guid parsedId;

            return Guid.TryParse(identifier, out parsedId)
                ? questionnaire.FirstOrDefault<IGroup>(q => q.PublicKey == parsedId)
                : questionnaire.FirstOrDefault<IGroup>(q => q.VariableName == identifier);
        }

        private static IQuestion GetQuestionByIdentifier(string identifier, QuestionnaireDocument questionnaire)
        {
            Guid parsedId;

            return Guid.TryParse(identifier, out parsedId)
                ? questionnaire.FirstOrDefault<IQuestion>(q => q.PublicKey == parsedId)
                : questionnaire.FirstOrDefault<IQuestion>(q => q.StataExportCaption == identifier);
        }

        private  QuestionnaireVerificationError GetVerificationErrorBySubstitutionReferenceOrNull(IQuestion questionWithSubstitution,
            string substitutionReference, Guid[] vectorOfAutopropagatedQuestionsByQuestionWithSubstitutions,
            QuestionnaireDocument questionnaire)
        {
            if (substitutionReference == questionWithSubstitution.StataExportCaption)
            {
                return QuestionWithTitleSubstitutionCantReferenceSelf(questionWithSubstitution);
            }

            if (substitutionReference == substitutionService.RosterTitleSubstitutionReference)
            {
                if (vectorOfAutopropagatedQuestionsByQuestionWithSubstitutions.Length == 0)
                {
                    return QuestionUsesRostertitleInTitleItNeedToBePlacedInsideRoster(questionWithSubstitution);
                }
                return null;
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

        private static QuestionnaireVerificationError QuestionUsesRostertitleInTitleItNeedToBePlacedInsideRoster(
            IQuestion questionsWithSubstitution)
        {
            return new QuestionnaireVerificationError("WB0059",
                VerificationMessages.WB0059_IfQuestionUsesRostertitleInTitleItNeedToBePlacedInsideRoster,
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

        private static QuestionnaireVerificationError VariableNameIsUsedAsOtherQuestionVariableName(IQuestion sourseQuestion)
        {
            return new QuestionnaireVerificationError("WB0062",
                VerificationMessages.WB0062_VariableNameForQuestionIsNotUnique,
                CreateReference(sourseQuestion));
        }

        private static QuestionnaireVerificationError CreateExpressionSyntaxError(ExpressionLocation expressionLocation)
        {
            if(expressionLocation.ExpressionType == ExpressionLocationType.General)
            {
                return new QuestionnaireVerificationError("WB0096", VerificationMessages.WB0096_GeneralCompilationError);
            }

            var reference = new QuestionnaireVerificationReference(
                expressionLocation.ItemType == ExpressionLocationItemType.Question ? 
                    QuestionnaireVerificationReferenceType.Question : 
                    QuestionnaireVerificationReferenceType.Group, expressionLocation.Id);

            if(expressionLocation.ExpressionType == ExpressionLocationType.Validation)
            {
                return new QuestionnaireVerificationError("WB0002", VerificationMessages.WB0002_CustomValidationExpressionHasIncorrectSyntax, reference);
            }
            else 
            {
                return new QuestionnaireVerificationError("WB0003",
                    VerificationMessages.WB0003_CustomEnablementConditionHasIncorrectSyntax, reference);
            }
        }


        private static void VerifyEnumerableAndAccumulateErrorsToList<T>(IEnumerable<T> enumerableToVerify,
            List<QuestionnaireVerificationError> errorList, 
            Func<T, QuestionnaireVerificationError> getErrorOrNull)
        {
            var resultErrors = enumerableToVerify
                .Select(getErrorOrNull)
                .Where(errorOrNull => errorOrNull != null);

            foreach (var error in resultErrors)
            {
                var verificationError = error;
                if (!errorList.Any(e => e.Code == verificationError.Code && e.References.SequenceEqual(verificationError.References)))
                {
                    errorList.Add(error);
                }
            }
        }

        private static bool NoQuestionsExist(QuestionnaireDocument questionnaire)
        {
            return !questionnaire.Find<IQuestion>(_ => true).Any();
        }

        private static bool IsRosterGroup(IGroup group)
        {
            return group.IsRoster;
        }

        private static bool IsGroup(IComposite questionnaireItem)
        {
            return questionnaireItem is IGroup;
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
                ? (IGroup)item.GetParent()
                : (IGroup)item;

            Guid[] rosterSizeQuestions =
                GetSpecifiedGroupAndAllItsParentGroupsStartingFromBottom(parent, questionnaire)
                    .Where(IsRosterGroup)
                    .Select(g => g.RosterSizeQuestionId.HasValue ? g.RosterSizeQuestionId.Value : g.PublicKey)
                    .ToArray();

            return rosterSizeQuestions.ToArray();
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

            if (rosterQuestionsAsVectorForQuestionSourceOfSubstitution == null) return false;
            if (rosterQuestionsAsVectorForQuestionSourceOfSubstitution.Length == 0) return false;

            if (rosterQuestionsAsVectorForQuestionSourceOfSubstitution.Length <= vectorOfRosterSizeQuestions.Length)
            {
                //reverse roster vectors 
                var rosterQuestionsAsVectorForQuestionSourceOfSubstitutionReverse =
                    rosterQuestionsAsVectorForQuestionSourceOfSubstitution.Reverse();
                var vectorOfRosterSizeQuestionsReverse = vectorOfRosterSizeQuestions.Reverse().ToArray();

                return
                    rosterQuestionsAsVectorForQuestionSourceOfSubstitutionReverse.Where(
                        (rosterSizeQuestionId, indexInArray) =>
                            vectorOfRosterSizeQuestionsReverse[indexInArray] != rosterSizeQuestionId).Any();
            }
            else
            {
                return true;
            }

        }

        private static bool IsSpecialThisIdentifier(string identifier)
        {
            return identifier.ToLower() == "this";
        }

        private static bool IsSupervisorQuestion(IQuestion question)
        {
            return question.QuestionScope == QuestionScope.Supervisor;
        }

        private static bool IsPreFilledQuestion(IQuestion question)
        {
            return question.Featured;
        }

        private static bool IsCategoricalLinkedQuestion(IQuestion question)
        {
            return (IsCategoricalMultiAnswersQuestion(question) || IsCategoricalSingleAnswerQuestion(question)) &&
                   question.LinkedToQuestionId.HasValue;
        }

        private static bool IsFilteredComboboxQuestion(IQuestion question)
        {
            return IsCategoricalSingleAnswerQuestion(question) && question.IsFilteredCombobox.HasValue &&
                   question.IsFilteredCombobox.Value;
        }
    }
}