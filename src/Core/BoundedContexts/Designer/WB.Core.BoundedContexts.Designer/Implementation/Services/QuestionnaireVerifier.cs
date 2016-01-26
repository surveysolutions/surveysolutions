using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Newtonsoft.Json;

using WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    internal partial class QuestionnaireVerifier : IQuestionnaireVerifier
    {
        private const int MaxExpressionLength = 10000;
        private const int MaxOptionsCountInCascadingQuestion = 15000;
        private const int MaxOptionsCountInFilteredComboboxQuestion = 15000;

        private const int DefaultVariableLengthLimit = 32;
        private const int DefaultRestrictedVariableLengthLimit = 20;

        private static readonly QuestionType[] RestrictedVariableLengthQuestionTypes =
            new QuestionType[]
            {
                QuestionType.GpsCoordinates,
                QuestionType.MultyOption,
                QuestionType.TextList
            };

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
        private readonly IDesignerEngineVersionService engineVersionService;
        private readonly IMacrosSubstitutionService macrosSubstitutionService;
        private readonly ILookupTableService lookupTableService;

        private static readonly Regex VariableNameRegex = new Regex("^(?!.*[_]{2})[A-Za-z][_A-Za-z0-9]*(?<!_)$");
        private static readonly Regex QuestionnaireNameRegex = new Regex(@"^[\w \-\(\)\\/]*$");

        public QuestionnaireVerifier(
            IExpressionProcessor expressionProcessor, 
            IFileSystemAccessor fileSystemAccessor,
            ISubstitutionService substitutionService, 
            IKeywordsProvider keywordsProvider,
            IExpressionProcessorGenerator expressionProcessorGenerator, 
            IDesignerEngineVersionService engineVersionService,
            IMacrosSubstitutionService macrosSubstitutionService, 
            ILookupTableService lookupTableService)
        {
            this.expressionProcessor = expressionProcessor;
            this.fileSystemAccessor = fileSystemAccessor;
            this.substitutionService = substitutionService;
            this.keywordsProvider = keywordsProvider;
            this.expressionProcessorGenerator = expressionProcessorGenerator;
            this.engineVersionService = engineVersionService;
            this.macrosSubstitutionService = macrosSubstitutionService;
            this.lookupTableService = lookupTableService;
        }

        private IEnumerable<Func<CachedQuestionnaireDocument, VerificationState, IEnumerable<QuestionnaireVerificationMessage>>> AtomicVerifiers
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
                    Verifier<IQuestion>((q, document)=>RosterSizeQuestionMaxValueCouldBeInRange1And40(q,document, GetMultyOptionRosterSizeOptionCountWhenMaxAllowedAnswersIsEmpty), "WB0099", VerificationMessages.WB0099_MaxNumberOfAnswersForRosterSizeQuestionCannotBeEmptyWhenQuestionHasMoreThan40Options),
                    Verifier<IQuestion>((q, document)=>RosterSizeQuestionMaxValueCouldBeInRange1And40(q,document, GetMaxNumberOfAnswersForRosterSizeQuestionWhenMore40Options), "WB0100", VerificationMessages.WB0100_MaxNumberOfAnswersForRosterSizeQuestionCannotBeGreaterThen40),
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
                    Verifier<IMultyOptionsQuestion>(MultiOptionQuestionYesNoQuestionCantBeLinked, "WB0007", VerificationMessages.WB0007_MultiOptionQuestionYesNoQuestionCantBeLinked),
                    Verifier<IMultyOptionsQuestion>(MultiOptionQuestionSupportsOnlyIntegerPositiveValues, "WB0008", VerificationMessages.WB0008_MultiOptionQuestionSupportsOnlyIntegerPositiveValues),
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
                    Verifier<IQuestion>(FilteredComboboxContainsMoreThanMaxOptions, "WB0075", VerificationMessages.WB0075_FilteredComboboxContainsMoreThan5000Options),
                    Verifier<IQuestion>(CategoricalOneAnswerOptionsCountMoreThanMaxOptionCount, "WB0076", VerificationMessages.WB0076_CategoricalOneAnswerOptionsCountMoreThan200),
                    Verifier<IMultimediaQuestion>(MultimediaQuestionIsInterviewersOnly, "WB0078", VerificationMessages.WB0078_MultimediaQuestionIsInterviewersOnly),
                    Verifier<IMultimediaQuestion>(MultimediaShouldNotHaveValidationExpression, "WB0079", VerificationMessages.WB0079_MultimediaShouldNotHaveValidationExpression),
                    Verifier<IQuestion, IComposite>(MultimediaQuestionsCannotBeUsedInValidationExpression, "WB0080", VerificationMessages.WB0080_MultimediaQuestionsCannotBeUsedInValidationExpression),
                    Verifier<IGroup, IComposite>(MultimediaQuestionsCannotBeUsedInGroupEnablementCondition, "WB0081", VerificationMessages.WB0081_MultimediaQuestionsCannotBeUsedInGroupEnablementCondition),
                    Verifier<IQuestion, IComposite>(MultimediaQuestionsCannotBeUsedInQuestionEnablementCondition, "WB0082", VerificationMessages.WB0082_MultimediaQuestionsCannotBeUsedInQuestionEnablementCondition),
                    Verifier<IGroup, IComposite>(QuestionsCannotBeUsedAsRosterTitle, "WB0083", VerificationMessages.WB0083_QuestionCannotBeUsedAsRosterTitle),
                    Verifier<IQuestion, IComposite>(CascadingComboboxOptionsHasNoParentOptions, "WB0084", VerificationMessages.WB0084_CascadingOptionsShouldHaveParent),
                    Verifier<IQuestion, IComposite>(ParentShouldNotHaveDeeperRosterLevelThanCascadingQuestion, "WB0085", VerificationMessages.WB0085_CascadingQuestionWrongParentLevel),
                    Verifier<SingleQuestion>(CascadingQuestionReferencesMissingParent, "WB0086", VerificationMessages.WB0086_ParentCascadingQuestionShouldExist),
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
                    Verifier<IGroup>(GroupHasLevelDepthMoreThan10, "WB0101", VerificationMessages.WB0101_GroupHasLevelDepthMoreThan10),
                    

                    MacrosVerifier(MacroHasEmptyName, "WB0014", VerificationMessages.WB0014_MacroHasEmptyName),
                    MacrosVerifier(MacroHasInvalidName, "WB0010", VerificationMessages.WB0010_MacroHasInvalidName),
                    
                    LookupVerifier(LookupTableNameIsKeyword, "WB0052", VerificationMessages.WB0052_LookupNameIsKeyword, VerificationMessageLevel.Critical),
                    LookupVerifier(LookupTableHasInvalidName, "WB0024", VerificationMessages.WB0024_LookupHasInvalidName, VerificationMessageLevel.Critical),
                    LookupVerifier(LookupTableHasEmptyName, "WB0025", VerificationMessages.WB0025_LookupHasEmptyName, VerificationMessageLevel.Critical),
                    LookupVerifier(LookupTableHasEmptyContent, "WB0048", VerificationMessages.WB0048_LookupHasEmptyContent, VerificationMessageLevel.Critical),
                    LookupVerifier(LookupTableHasInvalidHeaders, "WB0031", VerificationMessages.WB0031_LookupTableHasInvalidHeaders, VerificationMessageLevel.Critical),
                    LookupVerifier(LookupTableMoreThan10Columns, "WB0043", VerificationMessages.WB0043_LookupTableMoreThan11Columns),
                    LookupVerifier(LookupTableMoreThan5000Rows, "WB0044", VerificationMessages.WB0044_LookupTableMoreThan5000Rows),
                    LookupVerifier(LookupTableNotUniqueRowcodeValues, "WB0047", VerificationMessages.WB0047_LookupTableNotUniqueRowcodeValues, VerificationMessageLevel.Critical),

                    VerifyGpsPrefilledQuestions,
                    ErrorsByLinkedQuestions,
                    ErrorsByQuestionsWithSubstitutions,
                    ErrorsByQuestionsWithDuplicateVariableName,
                    ErrorsByRostersWithDuplicateVariableName,
                    ErrorsByMacrosWithDuplicateName,
                    ErrorsByLookupTablesWithDuplicateName,
                    ErrorsByLookupTablesWithDuplicateVariableName,
                    ErrorsByQuestionnaireEntitiesShareSameInternalId,

                    VerifyAmountOfRosters
                };
            }
        }

        private static IEnumerable<QuestionnaireVerificationMessage> ErrorsByQuestionnaireEntitiesShareSameInternalId(CachedQuestionnaireDocument questionnaire, VerificationState state)
        {
            return questionnaire
                    .GetAllEntitiesIds()
                    .GroupBy(x => x.Item1)
                    .Where(group => group.Count() > 1)
                    .Select(group =>
                            new QuestionnaireVerificationMessage(
                                "WB0102",
                                VerificationMessages.WB0102_QuestionnaireEntitiesShareSameInternalId,
                                VerificationMessageLevel.General, 
                                group.Select(x => new QuestionnaireVerificationReference(GetItemTypeById(questionnaire, x.Item1, x.Item2), x.Item1))));
        }

        private static QuestionnaireVerificationReferenceType GetItemTypeById(CachedQuestionnaireDocument questionnaire, Guid id, Type entityType)
        {
            if (typeof(IQuestion).IsAssignableFrom(entityType))
                return QuestionnaireVerificationReferenceType.Question;

            if (entityType.IsAssignableFrom(typeof(StaticText)))
                return QuestionnaireVerificationReferenceType.Question;

            var group = questionnaire.Find<IGroup>(id);
           
            return IsRosterGroup(group)
                ? QuestionnaireVerificationReferenceType.Roster
                : QuestionnaireVerificationReferenceType.Group;
        }

        private bool MultiOptionQuestionYesNoQuestionCantBeLinked(IMultyOptionsQuestion question)
        {
            return question.YesNoView && question.LinkedToQuestionId.HasValue;
        }

        private bool MultiOptionQuestionSupportsOnlyIntegerPositiveValues(IMultyOptionsQuestion question)
        {
            if (question.Answers == null)
                return false;
            foreach (var option in question.Answers)
            {
                int optionValue;
                if (int.TryParse(option.AnswerValue, out optionValue))
                {
                    if (optionValue < 0)
                        return true;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }


        private IEnumerable<QuestionnaireVerificationMessage> VerifyGpsPrefilledQuestions(CachedQuestionnaireDocument document, VerificationState state)
        {
            var gpsPrefilledQuestions= document.Find<GpsCoordinateQuestion>(q => q.Featured).ToArray();
            if (gpsPrefilledQuestions.Length < 2)
                return Enumerable.Empty<QuestionnaireVerificationMessage>();

            var gpsPrefilledQuestionsReferences = gpsPrefilledQuestions.Select(CreateReference).ToArray();

            return new[]
            {
                new QuestionnaireVerificationMessage("WB0006",
                    VerificationMessages.WB0006_OnlyOneGpsQuestionCouldBeMarkedAsPrefilled,
                    VerificationMessageLevel.General,
                    gpsPrefilledQuestionsReferences)
            };
        }

        private bool ValidationExpresssionHasLengthMoreThan10000Characters(IQuestion question, VerificationState state, CachedQuestionnaireDocument questionnaire)
        {
            if (string.IsNullOrEmpty(question.ValidationExpression))
                return false;

            var exceeded = macrosSubstitutionService.InlineMacros(question.ValidationExpression, questionnaire.Macros.Values).Length > MaxExpressionLength;

            state.HasExceededLimitByValidationExpresssionCharactersLength |= exceeded;

            return exceeded;
        }

        private bool ConditionExpresssionHasLengthMoreThan10000Characters(IComposite groupOrQuestion, VerificationState state, CachedQuestionnaireDocument questionnaire)
        {
            var customEnablementCondition = GetCustomEnablementCondition(groupOrQuestion);
            
            if (string.IsNullOrEmpty(customEnablementCondition))
                return false;

            var substituteMacroses = this.macrosSubstitutionService.InlineMacros(customEnablementCondition, questionnaire.Macros.Values);

            var exceeded = substituteMacroses.Length > MaxExpressionLength;

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

        private IEnumerable<QuestionnaireVerificationMessage> ErrorsByConditionAndValidationExpressions(
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

        private static Tuple<bool, decimal> QuestionnaireHasSizeMoreThan5MB(CachedQuestionnaireDocument questionnaire)
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

            return this.expressionProcessorGenerator.GenerateProcessorStateAssembly(
                questionnaire,this.engineVersionService.GetLatestSupportedVersion(), 
                out resultAssembly);
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
            return question.CascadeFromQuestionId.HasValue && question.Answers != null && question.Answers.Count > MaxOptionsCountInCascadingQuestion;
        }

        private static EntityVerificationResult<SingleQuestion> CascadingHasCircularReference(SingleQuestion question, CachedQuestionnaireDocument questionnaire)
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

        private static bool CascadingQuestionReferencesMissingParent(SingleQuestion question, CachedQuestionnaireDocument questionnaire)
        {
            if (!question.CascadeFromQuestionId.HasValue)
                return false;

            return questionnaire.Find<SingleQuestion>(question.CascadeFromQuestionId.Value) == null;
        }

        private static EntityVerificationResult<IComposite> ParentShouldNotHaveDeeperRosterLevelThanCascadingQuestion(IQuestion question, CachedQuestionnaireDocument questionnaire)
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

        private static EntityVerificationResult<IComposite> CascadingComboboxOptionsHasNoParentOptions(IQuestion question, CachedQuestionnaireDocument document)
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

        private static bool FilteredComboboxContainsMoreThanMaxOptions(IQuestion question)
        {
            return IsFilteredComboboxQuestion(question) && question.Answers != null && question.Answers.Count > MaxOptionsCountInFilteredComboboxQuestion;
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
            return question.StataExportCaption != null && keywordsProvider.GetAllReservedKeywords().Contains(question.StataExportCaption.ToLower());
        }

        private bool RosterHasVariableNameReservedForServiceNeeds(IGroup roster)
        {
            if (!IsRosterGroup(roster))
                return false;

            return roster.VariableName != null && keywordsProvider.GetAllReservedKeywords().Contains(roster.VariableName.ToLower());
        }

        public IEnumerable<QuestionnaireVerificationMessage> Verify(QuestionnaireDocument questionnaire)
        {
            var cachedQuestionnaire = new CachedQuestionnaireDocument(questionnaire);
            //questionnaire.ConnectChildrenWithParent();

            var state = new VerificationState();

            var staticVerificationErrors =
                (from verifier in this.AtomicVerifiers
                let errors = verifier.Invoke(cachedQuestionnaire, state)
                from error in errors
                select error).ToList();

            if (staticVerificationErrors.Any(e => e.MessageLevel == VerificationMessageLevel.Critical))
                return staticVerificationErrors;

            return staticVerificationErrors.Concat(
                ErrorsByConditionAndValidationExpressions(questionnaire, state));
        }

        private static Func<CachedQuestionnaireDocument, VerificationState, IEnumerable<QuestionnaireVerificationMessage>> MacrosVerifier(
            Func<Macro, CachedQuestionnaireDocument, bool> hasError, string code, string message)
        {
            return (questionnaire, state) => questionnaire
                    .Macros
                    .Where(entity => hasError(entity.Value, questionnaire))
                    .Select(entity => new QuestionnaireVerificationMessage(code, message, VerificationMessageLevel.General, CreateMacrosReference(entity.Key)));
        }

        private static Func<CachedQuestionnaireDocument, VerificationState, IEnumerable<QuestionnaireVerificationMessage>> LookupVerifier(
            Func<Guid, LookupTable, CachedQuestionnaireDocument, bool> hasError, string code, string message, VerificationMessageLevel level = VerificationMessageLevel.General)
        {
            return (questionnaire, state) => questionnaire
                    .LookupTables
                    .Where(entity => hasError(entity.Key, entity.Value, questionnaire))
                    .Select(entity => new QuestionnaireVerificationMessage(code, message, level, CreateLookupReference(entity.Key)));
        }

        private Func<CachedQuestionnaireDocument, VerificationState, IEnumerable<QuestionnaireVerificationMessage>> LookupVerifier(
            Func<LookupTable, LookupTableContent, CachedQuestionnaireDocument, bool> hasError, string code, string message, VerificationMessageLevel level = VerificationMessageLevel.General)
        {
            return (questionnaire, state) => 
                from lookupTable in questionnaire.LookupTables
                let lookupTableContent = this.lookupTableService.GetLookupTableContent(questionnaire.PublicKey, lookupTable.Key)
                where lookupTableContent != null
                where hasError(lookupTable.Value, lookupTableContent, questionnaire)
                select new QuestionnaireVerificationMessage(code, message, level, CreateLookupReference(lookupTable.Key));
        }

        private static Func<CachedQuestionnaireDocument, VerificationState, IEnumerable<QuestionnaireVerificationMessage>> Verifier(
            Func<CachedQuestionnaireDocument, bool> hasError, string code, string message)
        {
            return (questionnaire, state) =>
                hasError(questionnaire)
                    ? new[] { new QuestionnaireVerificationMessage(code, message, VerificationMessageLevel.General) }
                    : Enumerable.Empty<QuestionnaireVerificationMessage>();
        }

        private static Func<CachedQuestionnaireDocument, VerificationState, IEnumerable<QuestionnaireVerificationMessage>> Verifier<TArg>(
            Func<CachedQuestionnaireDocument, Tuple<bool, TArg>> hasError, string code, Func<TArg, string> messageBuilder)
        {
            return (questionnaire, state) =>
            {
                var errorCheckResult = hasError(questionnaire);
                return errorCheckResult.Item1
                    ? new[] { new QuestionnaireVerificationMessage(code, messageBuilder.Invoke(errorCheckResult.Item2), VerificationMessageLevel.General) }
                    : Enumerable.Empty<QuestionnaireVerificationMessage>();
            };
        }

        private static Func<CachedQuestionnaireDocument, VerificationState, IEnumerable<QuestionnaireVerificationMessage>> Verifier<TEntity>(
            Func<TEntity, bool> hasError, string code, string message)
            where TEntity : class, IComposite
        {
            return (questionnaire, state) =>
                questionnaire
                    .Find<TEntity>(hasError)
                    .Select(entity => new QuestionnaireVerificationMessage(code, message, VerificationMessageLevel.General, CreateReference(entity)));
        }

        private static Func<CachedQuestionnaireDocument, VerificationState, IEnumerable<QuestionnaireVerificationMessage>> Verifier<TEntity>(
            Func<TEntity, VerificationState, CachedQuestionnaireDocument, bool> hasError, string code, string message)
            where TEntity : class, IComposite
        {
            return (questionnaire, state) =>
                questionnaire
                    .Find<TEntity>(entity => hasError(entity, state, questionnaire))
                    .Select(entity => new QuestionnaireVerificationMessage(code, message, VerificationMessageLevel.General, CreateReference(entity)));
        }

        private static Func<CachedQuestionnaireDocument, VerificationState, IEnumerable<QuestionnaireVerificationMessage>> Verifier<TEntity>(
            Func<TEntity, CachedQuestionnaireDocument, bool> hasError, string code, string message)
            where TEntity : class, IComposite
        {
            return (questionnaire, state) =>
                questionnaire
                    .Find<TEntity>(entity => hasError(entity, questionnaire))
                    .Select(entity => new QuestionnaireVerificationMessage(code, message, VerificationMessageLevel.General, CreateReference(entity)));
        }

        private static Func<CachedQuestionnaireDocument, VerificationState, IEnumerable<QuestionnaireVerificationMessage>> Verifier<TEntity, TReferencedEntity>(
            Func<TEntity, CachedQuestionnaireDocument, EntityVerificationResult<TReferencedEntity>> verifyEntity, string code, string message)
            where TEntity : class, IComposite
            where TReferencedEntity : class, IComposite
        {
            return (questionnaire, state) =>
                from entity in questionnaire.Find<TEntity>(_ => true)
                let verificationResult = verifyEntity(entity, questionnaire)
                where verificationResult.HasErrors
                select new QuestionnaireVerificationMessage(code, message, VerificationMessageLevel.General, verificationResult.ReferencedEntities.Select(CreateReference));
        }

        private static bool MacroHasEmptyName(Macro macro, CachedQuestionnaireDocument questionnaire)
        {
            return string.IsNullOrWhiteSpace(macro.Name);
        }

        private static bool MacroHasInvalidName(Macro macro, CachedQuestionnaireDocument questionnaire)
        {
            return !IsVariableNameValid(macro.Name);
        }

        private static bool LookupTableHasEmptyName(Guid tableId, LookupTable table, CachedQuestionnaireDocument questionnaire)
        {
            return string.IsNullOrWhiteSpace(table.TableName);
        }
        
        private static bool LookupTableHasInvalidName(Guid tableId, LookupTable table, CachedQuestionnaireDocument questionnaire)
        {
            return !IsVariableNameValid(table.TableName);
        }

        private bool LookupTableNameIsKeyword(Guid tableId, LookupTable table, CachedQuestionnaireDocument questionnaire)
        {
            if (string.IsNullOrWhiteSpace(table.FileName))
                return false;
            return keywordsProvider.GetAllReservedKeywords().Contains(table.TableName.ToLower());
        }

        private bool LookupTableHasEmptyContent(Guid tableId, LookupTable table, CachedQuestionnaireDocument questionnaire)
        {
            var lookupTableContent = this.lookupTableService.GetLookupTableContent(questionnaire.PublicKey, tableId);
            return lookupTableContent == null;
        }

        private static bool LookupTableHasInvalidHeaders(LookupTable table, LookupTableContent tableContent, CachedQuestionnaireDocument questionnaire)
        {
            return !tableContent.VariableNames.All(IsVariableNameValid);
        }

        private static bool LookupTableMoreThan10Columns(LookupTable table, LookupTableContent tableContent, CachedQuestionnaireDocument questionnaire)
        {
            return tableContent.VariableNames.Count() > 10;
        }

        private static bool LookupTableMoreThan5000Rows(LookupTable table, LookupTableContent tableContent, CachedQuestionnaireDocument questionnaire)
        {
            return tableContent.Rows.Count() > 5000;
        }

        private static bool LookupTableNotUniqueRowcodeValues(LookupTable table, LookupTableContent tableContent, CachedQuestionnaireDocument questionnaire)
        {
            return tableContent.Rows.Select(x => x.RowCode).Distinct().Count() != tableContent.Rows.Count();
        }

        private static bool CategoricalMultiAnswersQuestionHasOptionsCountLessThanMaxAllowedAnswersCount(IMultyOptionsQuestion question)
        {
            return question.MaxAllowedAnswers.HasValue && !question.LinkedToQuestionId.HasValue &&
                   (question.MaxAllowedAnswers.Value > question.Answers.Count);
        }

        private static bool GroupWhereRosterSizeSourceIsQuestionHasNoRosterSizeQuestion(IGroup group, CachedQuestionnaireDocument questionnaire)
        {
            return IsRosterByQuestion(group) && GetRosterSizeQuestionByRosterGroup(group, questionnaire) == null;
        }

        private static bool RosterSizeSourceQuestionTypeIsIncorrect(IGroup group, CachedQuestionnaireDocument questionnaire)
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

        private static bool GroupWhereRosterSizeSourceIsQuestionHasInvalidRosterTitleQuestion(IGroup group, CachedQuestionnaireDocument questionnaire)
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

        private static bool GroupWhereRosterSizeIsCategoricalMultyAnswerQuestionHaveRosterTitleQuestion(IGroup group, CachedQuestionnaireDocument questionnaire)
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

            if (group.FixedRosterTitles.Length < 2)
                return true;

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

            return group.FixedRosterTitles.Length > Constants.MaxRosterRowCount;
        }

        private static int? GetMaxNumberOfAnswersForRosterSizeQuestionWhenMore40Options(IQuestion question)
        {
            var multyOptionQuestion = question as IMultyOptionsQuestion;
            if (multyOptionQuestion != null && multyOptionQuestion.Answers.Count > Constants.MaxRosterRowCount)
                return multyOptionQuestion.MaxAllowedAnswers;
            return null;
        }

        private static int? GetMultyOptionRosterSizeOptionCountWhenMaxAllowedAnswersIsEmpty(IQuestion question)
        {
            var multyOptionQuestion = question as IMultyOptionsQuestion;
            if (multyOptionQuestion != null && !multyOptionQuestion.MaxAllowedAnswers.HasValue)
                return multyOptionQuestion.Answers.Count;
            return null;
        }

        private static bool RosterSizeQuestionMaxValueCouldBeInRange1And40(IQuestion question, CachedQuestionnaireDocument questionnaire, Func<IQuestion, int?> getRosterSizeQuestionMaxValue)
        {
            if (!IsRosterSizeQuestion(question, questionnaire))
                return false;
            if (!IsQuestionAllowedToBeRosterSizeSource(question))
                return false;
            var rosterSizeQuestionMaxValue = getRosterSizeQuestionMaxValue(question);
            if (!rosterSizeQuestionMaxValue.HasValue)
                return false;
            return !Enumerable.Range(1, Constants.MaxRosterRowCount).Contains(rosterSizeQuestionMaxValue.Value);
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

        private static bool GroupHasLevelDepthMoreThan10(IGroup group)
        {
            int groupLevel = 0;
            IComposite questionnaireItem = group;
            while (questionnaireItem != null)
            {
                groupLevel++;
                questionnaireItem = questionnaireItem.GetParent();
            }

            return groupLevel > 10 + 1/*questionnaire level*/;
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

        private static bool PrefilledQuestionCantBeInsideOfRoster(IQuestion question, CachedQuestionnaireDocument questionnaire)
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

        private static bool QuestionnaireTitleHasInvalidCharacters(CachedQuestionnaireDocument questionnaire)
        {
            if (string.IsNullOrWhiteSpace(questionnaire.Title))
            {
                return false;
            }

            return !QuestionnaireNameRegex.IsMatch(questionnaire.Title);
        }

        private static bool QuestionHasInvalidVariableName(IQuestion question)
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

        private static bool IsVariableNameValid(string variableName)
        {
            if (string.IsNullOrEmpty(variableName))
                return true;

           if (variableName.Length > DefaultVariableLengthLimit)
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

        private bool RosterHasVariableNameEqualToQuestionnaireTitle(IGroup group, CachedQuestionnaireDocument questionnaire)
        {
            if (!group.IsRoster)
                return false;

            if (string.IsNullOrEmpty(group.VariableName))
                return false;

            var questionnaireVariableName = this.fileSystemAccessor.MakeValidFileName(questionnaire.Title);

            return group.VariableName.Equals(questionnaireVariableName, StringComparison.InvariantCultureIgnoreCase);
        }

        private EntityVerificationResult<IComposite> QuestionShouldNotHaveCircularReferences(IQuestion entity, CachedQuestionnaireDocument questionnaire)
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
        
        private EntityVerificationResult<IComposite> MultimediaQuestionsCannotBeUsedInValidationExpression(IQuestion question, CachedQuestionnaireDocument questionnaire)
        {
            return this.VerifyWhetherEntityExpressionReferencesIncorrectQuestions(
                question, question.ValidationExpression, questionnaire,
                isReferencedQuestionIncorrect: referencedQuestion => referencedQuestion.QuestionType == QuestionType.Multimedia);
        }

        private EntityVerificationResult<IComposite> MultimediaQuestionsCannotBeUsedInQuestionEnablementCondition(IQuestion question, CachedQuestionnaireDocument questionnaire)
        {
            return this.VerifyWhetherEntityExpressionReferencesIncorrectQuestions(
               question, question.ConditionExpression, questionnaire,
               isReferencedQuestionIncorrect: referencedQuestion => referencedQuestion.QuestionType == QuestionType.Multimedia);
        }

        private static EntityVerificationResult<IComposite> QuestionsCannotBeUsedAsRosterTitle(IGroup group, CachedQuestionnaireDocument questionnaire)
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

        private EntityVerificationResult<IComposite> MultimediaQuestionsCannotBeUsedInGroupEnablementCondition(IGroup group, CachedQuestionnaireDocument questionnaire)
        {
            return this.VerifyWhetherEntityExpressionReferencesIncorrectQuestions(
               group, group.ConditionExpression, questionnaire,
               isReferencedQuestionIncorrect: referencedQuestion => referencedQuestion.QuestionType == QuestionType.Multimedia);
        }

        private EntityVerificationResult<IComposite> CategoricalLinkedQuestionUsedInValidationExpression(IQuestion question, CachedQuestionnaireDocument questionnaire)
        {
            return this.VerifyWhetherEntityExpressionReferencesIncorrectQuestions(question, question.ValidationExpression,
                questionnaire, isReferencedQuestionIncorrect: IsCategoricalLinkedQuestion);
        }

        private EntityVerificationResult<IComposite> CategoricalLinkedQuestionUsedInQuestionEnablementCondition(IQuestion question, CachedQuestionnaireDocument questionnaire)
        {
            return this.VerifyWhetherEntityExpressionReferencesIncorrectQuestions(question, question.ConditionExpression,
                questionnaire, isReferencedQuestionIncorrect: IsCategoricalLinkedQuestion);
        }

        private EntityVerificationResult<IComposite> CategoricalLinkedQuestionUsedInGroupEnablementCondition(IGroup group, CachedQuestionnaireDocument questionnaire)
        {
            return this.VerifyWhetherEntityExpressionReferencesIncorrectQuestions(group, group.ConditionExpression,
                questionnaire, isReferencedQuestionIncorrect: IsCategoricalLinkedQuestion);
        }


        private EntityVerificationResult<IComposite> VerifyWhetherEntityExpressionReferencesIncorrectQuestions(
            IComposite entity, string expression, CachedQuestionnaireDocument questionnaire, Func<IQuestion, bool> isReferencedQuestionIncorrect)
        {
            if (string.IsNullOrEmpty(expression))
                return new EntityVerificationResult<IComposite> { HasErrors = false };

            IEnumerable<IQuestion> incorrectReferencedQuestions = this.expressionProcessor
                .GetIdentifiersUsedInExpression(macrosSubstitutionService.InlineMacros(expression, questionnaire.Macros.Values))
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

        private static EntityVerificationResult<IComposite> RosterSizeQuestionHasDeeperRosterLevelThanDependentRoster(IGroup roster, CachedQuestionnaireDocument questionnaire)
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

        private static IEnumerable<QuestionnaireVerificationMessage> ErrorsByLinkedQuestions(
            CachedQuestionnaireDocument questionnaire, VerificationState state)
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

        private static IEnumerable<QuestionnaireVerificationMessage> ErrorsByQuestionsWithDuplicateVariableName(
            CachedQuestionnaireDocument questionnaire, VerificationState state)
        {
            var questionsDuplicates = questionnaire.Find<IQuestion>(q => true)
                .Where(x => !string.IsNullOrEmpty(x.StataExportCaption))
                .GroupBy(s => s.StataExportCaption, StringComparer.InvariantCultureIgnoreCase)
                .SelectMany(group => group.Skip(1));

            foreach (IQuestion questionsDuplicate in questionsDuplicates)
                yield return VariableNameIsUsedAsOtherQuestionVariableName(questionsDuplicate);
        }

        private static IEnumerable<QuestionnaireVerificationMessage> ErrorsByMacrosWithDuplicateName(CachedQuestionnaireDocument questionnaire, VerificationState state)
        {
            return questionnaire
                    .Macros
                    .Where(x => !string.IsNullOrEmpty(x.Value.Name))
                    .GroupBy(x => x.Value.Name, StringComparer.InvariantCultureIgnoreCase)
                    .Where(group => group.Count() > 1)
                    .Select(group =>
                            new QuestionnaireVerificationMessage(
                                "WB0020",
                                VerificationMessages.WB0020_NameForMacrosIsNotUnique,
                                VerificationMessageLevel.General,
                                group.Select(e => CreateMacrosReference(e.Key))));
        }

        private static IEnumerable<QuestionnaireVerificationMessage> ErrorsByLookupTablesWithDuplicateName(CachedQuestionnaireDocument questionnaire, VerificationState state)
        {
            return questionnaire
                    .LookupTables
                    .Where(x => !string.IsNullOrEmpty(x.Value.TableName))
                    .GroupBy(x => x.Value.TableName, StringComparer.InvariantCultureIgnoreCase)
                    .Where(group => group.Count() > 1)
                    .Select(group =>
                            new QuestionnaireVerificationMessage(
                                "WB0026",
                                VerificationMessages.WB0026_NameForLookupTableIsNotUnique,
                                VerificationMessageLevel.Critical,
                                group.Select(e => CreateLookupReference(e.Key))));
        }

        private static IEnumerable<QuestionnaireVerificationMessage> ErrorsByLookupTablesWithDuplicateVariableName(
            CachedQuestionnaireDocument questionnaire, VerificationState state)
        {
            var rosterVariableNameMappedOnRosters = questionnaire
                .Find<IGroup>(g => g.IsRoster && !string.IsNullOrEmpty(g.VariableName))
                .GroupBy(s => s.VariableName, StringComparer.InvariantCultureIgnoreCase)
                .ToDictionary(r => r.Key, r => r.ToArray());

            var questionsVariableNamesMappedOnQuestions = questionnaire.Find<IQuestion>(q => true)
                 .Where(x => !string.IsNullOrEmpty(x.StataExportCaption))
                 .GroupBy(s => s.StataExportCaption, StringComparer.InvariantCultureIgnoreCase)
                 .ToDictionary(r => r.Key, r => r.ToArray());

            var lookupTablesNames = questionnaire.LookupTables.Where(x => !string.IsNullOrEmpty(x.Value.TableName))
                .Select(x => new { LookupTableName = x.Value.TableName, LookupTableId = x.Key })
                .GroupBy(x => x.LookupTableName, StringComparer.InvariantCultureIgnoreCase)
                .ToDictionary(x => x.Key, x => x.Select(l => l.LookupTableId));

            foreach (var lookup in lookupTablesNames)
            {
                if (rosterVariableNameMappedOnRosters.ContainsKey(lookup.Key))
                    yield return
                        new QuestionnaireVerificationMessage("WB0029",
                            VerificationMessages.WB0029_LookupWithTheSameVariableNameAlreadyExists,
                            VerificationMessageLevel.Critical,
                            CreateReference(rosterVariableNameMappedOnRosters[lookup.Key].First()),
                            CreateLookupReference(lookup.Value.First()));

                if (questionsVariableNamesMappedOnQuestions.ContainsKey(lookup.Key))
                    yield return
                        new QuestionnaireVerificationMessage("WB0029",
                            VerificationMessages.WB0029_LookupWithTheSameVariableNameAlreadyExists,
                            VerificationMessageLevel.Critical,
                            CreateReference(questionsVariableNamesMappedOnQuestions[lookup.Key].First()),
                            CreateLookupReference(lookup.Value.First()));
            }
        }

        private static IEnumerable<QuestionnaireVerificationMessage> ErrorsByRostersWithDuplicateVariableName(
            CachedQuestionnaireDocument questionnaire, VerificationState state)
        {
            var rosterVariableNameMappedOnRosters = questionnaire
                .Find<IGroup>(g => g.IsRoster && !string.IsNullOrEmpty(g.VariableName))
                .GroupBy(s => s.VariableName, StringComparer.InvariantCultureIgnoreCase)
                .ToDictionary(r => r.Key, r => r.ToArray());

            var questionsVariableNamesMappedOnQuestions = questionnaire.Find<IQuestion>(q => true)
                 .Where(x => !string.IsNullOrEmpty(x.StataExportCaption))
                 .GroupBy(s => s.StataExportCaption, StringComparer.InvariantCultureIgnoreCase)
                 .ToDictionary(r => r.Key, r => r.ToArray());

            foreach (var rosterVariableNameMappedOnRoster in rosterVariableNameMappedOnRosters)
            {
                if (questionsVariableNamesMappedOnQuestions.ContainsKey(rosterVariableNameMappedOnRoster.Key))
                    yield return
                        new QuestionnaireVerificationMessage("WB0093",
                            VerificationMessages.WB0093_QuestionWithTheSameVariableNameAlreadyExists,
                            VerificationMessageLevel.Critical,
                            CreateReference(rosterVariableNameMappedOnRoster.Value.First()),
                            CreateReference(questionsVariableNamesMappedOnQuestions[rosterVariableNameMappedOnRoster.Key].First()));

                if (rosterVariableNameMappedOnRoster.Value.Length < 2)
                    continue;

                yield return new QuestionnaireVerificationMessage("WB0068",
                    VerificationMessages.WB0068_RosterHasNotUniqueVariableName,
                    VerificationMessageLevel.Critical,
                    rosterVariableNameMappedOnRoster.Value.Select(CreateReference).ToArray());
            }
        }

        private IEnumerable<QuestionnaireVerificationMessage> ErrorsByQuestionsWithSubstitutions(
            CachedQuestionnaireDocument questionnaire, VerificationState state)
        {
            IEnumerable<IQuestion> questionsWithSubstitutions =
                questionnaire.Find<IQuestion>(question => substitutionService.GetAllSubstitutionVariableNames(question.QuestionText).Length > 0);

            var errorByAllQuestionsWithSubstitutions = new List<QuestionnaireVerificationMessage>();

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

        private IEnumerable<IQuestion> ReferencedQuestion(string expression, CachedQuestionnaireDocument questionnaire)
        {
            IEnumerable<IQuestion> incorrectReferencedQuestions = this.expressionProcessor
                .GetIdentifiersUsedInExpression(expression)
                .Select(identifier => GetQuestionByIdentifier(identifier, questionnaire))
                .Where(referencedQuestion => referencedQuestion != null)
                .ToList();
            return incorrectReferencedQuestions;
        }

        private static QuestionnaireVerificationReference CreateMacrosReference(Guid macroId)
        {
            return new QuestionnaireVerificationReference(QuestionnaireVerificationReferenceType.Macro, macroId);
        }

        private static QuestionnaireVerificationReference CreateLookupReference(Guid tableId)
        {
            return new QuestionnaireVerificationReference(QuestionnaireVerificationReferenceType.LookupTable, tableId);
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
        
        private static bool CategoricalMultianswerQuestionIsFeatured(IMultyOptionsQuestion question, CachedQuestionnaireDocument questionnaire)
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

        private static IQuestion GetQuestionByIdentifier(string identifier, CachedQuestionnaireDocument questionnaire)
        {
            Guid parsedId;

            return Guid.TryParse(identifier, out parsedId)
                ? questionnaire.FirstOrDefault<IQuestion>(q => q.PublicKey == parsedId)
                : questionnaire.FirstOrDefault<IQuestion>(q => q.StataExportCaption == identifier);
        }

        private  QuestionnaireVerificationMessage GetVerificationErrorBySubstitutionReferenceOrNull(IQuestion questionWithSubstitution,
            string substitutionReference, Guid[] vectorOfAutopropagatedQuestionsByQuestionWithSubstitutions,
            CachedQuestionnaireDocument questionnaire)
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

        private static QuestionnaireVerificationMessage QuestionWithTitleSubstitutionCantBePrefilled(IQuestion questionsWithSubstitution)
        {
            return new QuestionnaireVerificationMessage("WB0015",
                VerificationMessages.WB0015_QuestionWithTitleSubstitutionCantBePrefilled,
                VerificationMessageLevel.General,
                CreateReference(questionsWithSubstitution));
        }

        private static QuestionnaireVerificationMessage QuestionWithTitleSubstitutionCantReferenceQuestionsWithDeeperPropagationLevel(IQuestion questionsWithSubstitution, IQuestion questionSourceOfSubstitution)
        {
            return new QuestionnaireVerificationMessage("WB0019",
                VerificationMessages.WB0019_QuestionWithTitleSubstitutionCantReferenceQuestionsWithDeeperPropagationLevel,
                VerificationMessageLevel.General,
                CreateReference(questionsWithSubstitution),
                CreateReference(questionSourceOfSubstitution));
        }

        private static QuestionnaireVerificationMessage QuestionWithTitleSubstitutionReferencesQuestionOfNotSupportedType(IQuestion questionsWithSubstitution, IQuestion questionSourceOfSubstitution)
        {
            return new QuestionnaireVerificationMessage("WB0018",
                VerificationMessages.WB0018_QuestionWithTitleSubstitutionReferencesQuestionOfNotSupportedType,
                VerificationMessageLevel.General,
                CreateReference(questionsWithSubstitution),
                CreateReference(questionSourceOfSubstitution));
        }

        private static QuestionnaireVerificationMessage QuestionWithTitleSubstitutionReferencesNotExistingQuestion(IQuestion questionsWithSubstitution)
        {
            return new QuestionnaireVerificationMessage("WB0017",
                VerificationMessages.WB0017_QuestionWithTitleSubstitutionReferencesNotExistingQuestion,
                VerificationMessageLevel.General,
                CreateReference(questionsWithSubstitution));
        }

        private static QuestionnaireVerificationMessage QuestionWithTitleSubstitutionCantReferenceSelf(IQuestion questionsWithSubstitution)
        {
            return new QuestionnaireVerificationMessage("WB0016",
                VerificationMessages.WB0016_QuestionWithTitleSubstitutionCantReferenceSelf,
                VerificationMessageLevel.General,
                CreateReference(questionsWithSubstitution));
        }

        private static QuestionnaireVerificationMessage QuestionUsesRostertitleInTitleItNeedToBePlacedInsideRoster(
            IQuestion questionsWithSubstitution)
        {
            return new QuestionnaireVerificationMessage("WB0059",
                VerificationMessages.WB0059_IfQuestionUsesRostertitleInTitleItNeedToBePlacedInsideRoster,
                VerificationMessageLevel.General,
                CreateReference(questionsWithSubstitution));
        }

        private static QuestionnaireVerificationMessage LinkedQuestionReferencesNotExistingQuestion(IQuestion linkedQuestion)
        {
            return new QuestionnaireVerificationMessage("WB0011",
                VerificationMessages.WB0011_LinkedQuestionReferencesNotExistingQuestion,
                VerificationMessageLevel.General,
                CreateReference(linkedQuestion));
        }

        private static QuestionnaireVerificationMessage LinkedQuestionReferencesQuestionOfNotSupportedType(IQuestion linkedQuestion, IQuestion sourceQuestion)
        {
            return new QuestionnaireVerificationMessage("WB0012",
                VerificationMessages.WB0012_LinkedQuestionReferencesQuestionOfNotSupportedType,
                VerificationMessageLevel.General,
                CreateReference(linkedQuestion),
                CreateReference(sourceQuestion));
        }

        private static QuestionnaireVerificationMessage LinkedQuestionReferenceQuestionNotUnderRosterGroup(IQuestion linkedQuestion, IQuestion sourceQuestion)
        {
            return new QuestionnaireVerificationMessage("WB0013",
                VerificationMessages.WB0013_LinkedQuestionReferencesQuestionNotUnderRosterGroup,
                VerificationMessageLevel.General,
                CreateReference(linkedQuestion),
                CreateReference(sourceQuestion));
        }

        private static QuestionnaireVerificationMessage VariableNameIsUsedAsOtherQuestionVariableName(IQuestion sourseQuestion)
        {
            return new QuestionnaireVerificationMessage("WB0062",
                VerificationMessages.WB0062_VariableNameForQuestionIsNotUnique,
                VerificationMessageLevel.Critical,
                CreateReference(sourseQuestion));
        }

        private static QuestionnaireVerificationMessage CreateExpressionSyntaxError(ExpressionLocation expressionLocation)
        {
            if(expressionLocation.ExpressionType == ExpressionLocationType.General)
            {
                return new QuestionnaireVerificationMessage("WB0096", VerificationMessages.WB0096_GeneralCompilationError, VerificationMessageLevel.General);
            }

            var reference = new QuestionnaireVerificationReference(
                expressionLocation.ItemType == ExpressionLocationItemType.Question ? 
                    QuestionnaireVerificationReferenceType.Question : 
                    QuestionnaireVerificationReferenceType.Group, expressionLocation.Id);

            if(expressionLocation.ExpressionType == ExpressionLocationType.Validation)
            {
                return new QuestionnaireVerificationMessage("WB0002", VerificationMessages.WB0002_CustomValidationExpressionHasIncorrectSyntax, 
                    VerificationMessageLevel.General, reference);
            }
            else 
            {
                return new QuestionnaireVerificationMessage("WB0003",
                    VerificationMessages.WB0003_CustomEnablementConditionHasIncorrectSyntax, 
                    VerificationMessageLevel.General, reference);
            }
        }


        private static void VerifyEnumerableAndAccumulateErrorsToList<T>(IEnumerable<T> enumerableToVerify,
            List<QuestionnaireVerificationMessage> errorList, 
            Func<T, QuestionnaireVerificationMessage> getErrorOrNull)
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

        private static bool NoQuestionsExist(CachedQuestionnaireDocument questionnaire)
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

        private static bool IsRosterSizeQuestion(IQuestion question, CachedQuestionnaireDocument questionnaire)
        {
            var rosterSizeQuestionIds =
                questionnaire.Find<IGroup>(IsRosterByQuestion).Select(group => group.RosterSizeQuestionId);
            return rosterSizeQuestionIds.Contains(question.PublicKey);
        }

        private static IQuestion GetRosterSizeQuestionByRosterGroup(IGroup group, CachedQuestionnaireDocument questionnaire)
        {
            return
                questionnaire.FirstOrDefault<IQuestion>(
                    question => group.RosterSizeQuestionId.HasValue && question.PublicKey == group.RosterSizeQuestionId.Value);
        }

        private static IEnumerable<IGroup> GetAllParentGroupsForQuestion(IQuestion question, CachedQuestionnaireDocument document)
        {
            return GetSpecifiedGroupAndAllItsParentGroupsStartingFromBottom((IGroup)question.GetParent(), document);
        }

        private static Guid[] GetAllRosterSizeQuestionsAsVectorOrNullIfSomeAreMissing(IComposite item, CachedQuestionnaireDocument questionnaire)
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

        private static IEnumerable<IGroup> GetSpecifiedGroupAndAllItsParentGroupsStartingFromBottom(IGroup group, CachedQuestionnaireDocument document)
        {
            var parentGroups = new List<IGroup>();

            while (group != document.Questionnaire)
            {
                parentGroups.Add(group);
                group = (IGroup)group.GetParent();
            }

            return parentGroups;
        }

        private static bool QuestionHasDeeperRosterLevelThenVectorOfRosterQuestions(IQuestion question,
            Guid[] vectorOfRosterSizeQuestions, CachedQuestionnaireDocument questionnaire)
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