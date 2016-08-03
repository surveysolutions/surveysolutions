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
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.QuestionnaireEntities;
using Translation = WB.Core.SharedKernels.SurveySolutions.Documents.Translation;

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
            QuestionType.Text,
            QuestionType.QRBarcode
        };

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
        private readonly IAttachmentService attachmentService;
        private readonly ITranslationsService translationService;
        private readonly IQuestionnaireTranslator questionnaireTranslator;
        private readonly ITopologicalSorter<string> topologicalSorter;

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
            ILookupTableService lookupTableService,
            IAttachmentService attachmentService,
            ITopologicalSorter<string> topologicalSorter, 
            ITranslationsService translationService, 
            IQuestionnaireTranslator questionnaireTranslator)
        {
            this.expressionProcessor = expressionProcessor;
            this.fileSystemAccessor = fileSystemAccessor;
            this.substitutionService = substitutionService;
            this.keywordsProvider = keywordsProvider;
            this.expressionProcessorGenerator = expressionProcessorGenerator;
            this.engineVersionService = engineVersionService;
            this.macrosSubstitutionService = macrosSubstitutionService;
            this.lookupTableService = lookupTableService;
            this.attachmentService = attachmentService;
            this.topologicalSorter = topologicalSorter;
            this.translationService = translationService;
            this.questionnaireTranslator = questionnaireTranslator;
        }

        private IEnumerable<Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>>> AtomicVerifiers
            => this.ErrorsVerifiers.Concat(this.WarningsVerifiers);

        private IEnumerable<Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>>> ErrorsVerifiers => new[]
        {
            Verifier(NoQuestionsExist, "WB0001", VerificationMessages.WB0001_NoQuestions),
            
            Verifier<IGroup>(GroupWhereRosterSizeSourceIsQuestionHasNoRosterSizeQuestion, "WB0009", VerificationMessages.WB0009_GroupWhereRosterSizeSourceIsQuestionHasNoRosterSizeQuestion),
            Verifier<IGroup>(RosterSizeSourceQuestionTypeIsIncorrect, "WB0023", VerificationMessages.WB0023_RosterSizeSourceQuestionTypeIsIncorrect),
            Verifier<IGroup>(GroupWhereRosterSizeSourceIsQuestionHaveFixedTitles, "WB0032", VerificationMessages.WB0032_GroupWhereRosterSizeSourceIsQuestionHaveFixedTitles),
            Verifier<IGroup>(GroupWhereRosterSizeSourceIsFixedTitlesHaveRosterSizeQuestion, "WB0033", VerificationMessages.WB0033_GroupWhereRosterSizeSourceIsFixedTitlesHaveRosterSizeQuestion),
            Verifier<IGroup>(GroupWhereRosterSizeSourceIsFixedTitlesHaveRosterTitleQuestion, "WB0034", VerificationMessages.WB0034_GroupWhereRosterSizeSourceIsFixedTitlesHaveRosterTitleQuestion),
            Verifier<IGroup>(GroupWhereRosterSizeSourceIsQuestionHasInvalidRosterTitleQuestion, "WB0035", VerificationMessages.WB0035_GroupWhereRosterSizeSourceIsQuestionHasInvalidRosterTitleQuestion),
            Verifier<IGroup>(GroupWhereRosterSizeIsCategoricalMultyAnswerQuestionHaveRosterTitleQuestion, "WB0036", VerificationMessages.WB0036_GroupWhereRosterSizeIsCategoricalMultyAnswerQuestionHaveRosterTitleQuestion),
            Verifier<IGroup>(GroupWhereRosterSizeSourceIsFixedTitlesHaveEmptyTitles, "WB0037", VerificationMessages.WB0037_GroupWhereRosterSizeSourceIsFixedTitlesHaveEmptyTitles),
            Verifier<IGroup>(GroupWhereRosterSizeSourceIsFixedTitlesHaveDuplicateValues, "WB0041", VerificationMessages.WB0041_GroupWhereRosterSizeSourceIsFixedTitlesHaveDuplicateValues),
            Verifier<IGroup>(GroupWhereRosterSizeSourceIsFixedTitlesValuesHaveNonIntegerValues, "WB0115", VerificationMessages.WB0115_FixRosterSupportsOnlyIntegerTitleValues),
            Verifier<IGroup>(RosterFixedTitlesHaveMoreThanAllowedItems, "WB0038", VerificationMessages.WB0038_RosterFixedTitlesHaveMoreThan40Items),
            Verifier<IGroup, IComposite>(RosterSizeQuestionHasDeeperRosterLevelThanDependentRoster, "WB0054", VerificationMessages.WB0054_RosterSizeQuestionHasDeeperRosterLevelThanDependentRoster),
            Verifier<IGroup>(RosterHasRosterLevelMoreThan4, "WB0055", VerificationMessages.WB0055_RosterHasRosterLevelMoreThan4),
            Verifier<IGroup>(RosterHasEmptyVariableName, "WB0067", VerificationMessages.WB0067_RosterHasEmptyVariableName),
            Verifier<IGroup>(RosterHasInvalidVariableName, "WB0069", VerificationMessages.WB0069_RosterHasInvalidVariableName),
            Verifier<IGroup>(this.RosterHasVariableNameEqualToQuestionnaireTitle, "WB0070", VerificationMessages.WB0070_RosterHasVariableNameEqualToQuestionnaireTitle),
            Verifier<IGroup>(this.RosterHasVariableNameReservedForServiceNeeds, "WB0058", VerificationMessages.WB0058_QuestionHasVariableNameReservedForServiceNeeds),
            Verifier<IGroup>(GroupHasLevelDepthMoreThan10, "WB0101", VerificationMessages.WB0101_GroupHasLevelDepthMoreThan10),
            TranslationVerifier<IGroup>(GroupTitleIsTooLong, "WB0260", VerificationMessages.WB0260_GroupTitleIsTooLong),
            Verifier<IGroup>(LongMultiRosterCannotBeNested, "WB0081", string.Format(VerificationMessages.WB0081_LongRosterCannotBeNested, Constants.MaxRosterRowCount)),
            Verifier<IGroup>(LongListRosterCannotBeNested, "WB0081", string.Format(VerificationMessages.WB0081_LongRosterCannotBeNested, Constants.MaxRosterRowCount)),
            Verifier<IGroup>(LongFixedRosterCannotBeNested, "WB0081", string.Format(VerificationMessages.WB0081_LongRosterCannotBeNested, Constants.MaxRosterRowCount)),
            Verifier<IGroup>(LongFixedRosterCannotHaveNestedRosters, "WB0080", string.Format(VerificationMessages.WB0080_LongRosterCannotHaveNestedRosters,Constants.MaxRosterRowCount)),
            Verifier<IGroup>(LongMultiRosterCannotHaveNestedRosters, "WB0080", string.Format(VerificationMessages.WB0080_LongRosterCannotHaveNestedRosters,Constants.MaxRosterRowCount)),
            Verifier<IGroup>(LongListRosterCannotHaveNestedRosters, "WB0080", string.Format(VerificationMessages.WB0080_LongRosterCannotHaveNestedRosters,Constants.MaxRosterRowCount)),

            Verifier<IMultyOptionsQuestion>(CategoricalMultiAnswersQuestionHasOptionsCountLessThanMaxAllowedAnswersCount, "WB0021", VerificationMessages.WB0021_CategoricalMultiAnswersQuestionHasOptionsCountLessThanMaxAllowedAnswersCount),
            Verifier<IMultyOptionsQuestion>(CategoricalMultianswerQuestionIsFeatured, "WB0022",VerificationMessages.WB0022_PrefilledQuestionsOfIllegalType),
            Verifier<IQuestion>((q, document)=>RosterSizeQuestionMaxValueCouldBeInRange1And40(q,document, GetMultyOptionRosterSizeOptionCountWhenMaxAllowedAnswersIsEmpty), "WB0099", VerificationMessages.WB0099_MaxNumberOfAnswersForRosterSizeQuestionCannotBeEmptyWhenQuestionHasMoreThan40Options),
            Verifier<IQuestion>((q, document)=>RosterSizeQuestionMaxValueCouldBeInRange1And40(q,document, GetMaxNumberOfAnswersForRosterSizeQuestionWhenMore40Options), "WB0100", VerificationMessages.WB0100_MaxNumberOfAnswersForRosterSizeQuestionCannotBeGreaterThen40),
            Verifier<IQuestion>(PrefilledQuestionCantBeInsideOfRoster, "WB0030", VerificationMessages.WB0030_PrefilledQuestionCantBeInsideOfRoster),
            Verifier<ITextListQuestion>(TextListQuestionCannotBePrefilled, "WB0039", VerificationMessages.WB0039_TextListQuestionCannotBePrefilled),
            Verifier<ITextListQuestion>(TextListQuestionCannotBeFilledBySupervisor, "WB0040", VerificationMessages.WB0040_TextListQuestionCannotBeFilledBySupervisor),
            Verifier<ITextListQuestion>(TextListQuestionMaxAnswerNotInRange1And40, "WB0042", VerificationMessages.WB0042_TextListQuestionMaxAnswerInRange1And40),
            TranslationVerifier<IQuestion>(QuestionHasOptionsWithEmptyValue, "WB0045", VerificationMessages.WB0045_QuestionHasOptionsWithEmptyValue),
            Verifier<IQRBarcodeQuestion>(QRBarcodeQuestionIsSupervisorQuestion, "WB0049", VerificationMessages.WB0049_QRBarcodeQuestionIsSupervisorQuestion),
            Verifier<IQRBarcodeQuestion>(QRBarcodeQuestionIsPreFilledQuestion, "WB0050", VerificationMessages.WB0050_QRBarcodeQuestionIsPreFilledQuestion),
            Verifier<IQuestion>(QuestionHasEmptyVariableName, "WB0057", VerificationMessages.WB0057_QuestionHasEmptyVariableName),
            Verifier<IQuestion>(QuestionHasInvalidVariableName, "WB0077", VerificationMessages.WB0077_QuestionHasInvalidVariableName),
            Verifier<IQuestion>(this.QuestionHasVariableNameReservedForServiceNeeds, "WB0058", VerificationMessages.WB0058_QuestionHasVariableNameReservedForServiceNeeds),
            Verifier<IQuestion>(CategoricalQuestionHasLessThan2Options, "WB0060", VerificationMessages.WB0060_CategoricalQuestionHasLessThan2Options),
            Verifier<IMultyOptionsQuestion>(CategoricalMultiAnswersQuestionHasMaxAllowedAnswersLessThan2, "WB0061", VerificationMessages.WB0061_CategoricalMultiAnswersQuestionHasMaxAllowedAnswersLessThan2),
            Verifier<IMultyOptionsQuestion>(this.MultiOptionQuestionYesNoQuestionCantBeLinked, "WB0007", VerificationMessages.WB0007_MultiOptionQuestionYesNoQuestionCantBeLinked),
            Verifier<IMultyOptionsQuestion>(this.MultiOptionQuestionHasNonIntegerOptionsValues, "WB0008", VerificationMessages.WB0008_MultiOptionQuestionSupportsOnlyIntegerPositiveValues),
            Verifier<IQuestion>(QuestionTypeIsNotAllowed, "WB0066", VerificationMessages.WB0066_QuestionTypeIsNotAllowed),
            TranslationVerifier<IQuestion>(OptionTitlesMustBeUniqueForCategoricalQuestion, "WB0072", VerificationMessages.WB0072_OptionTitlesMustBeUniqueForCategoricalQuestion),
            Verifier<IQuestion>(OptionValuesMustBeUniqueForCategoricalQuestion, "WB0073", VerificationMessages.WB0073_OptionValuesMustBeUniqueForCategoricalQuestion),
            Verifier<IQuestion>(FilteredComboboxIsLinked, "WB0074", VerificationMessages.WB0074_FilteredComboboxIsLinked),
            Verifier<IQuestion>(FilteredComboboxContainsMoreThanMaxOptions, "WB0075", VerificationMessages.WB0075_FilteredComboboxContainsMoreThan5000Options),
            Verifier<IQuestion>(CategoricalOneAnswerOptionsCountMoreThanMaxOptionCount, "WB0076", VerificationMessages.WB0076_CategoricalOneAnswerOptionsCountMoreThan200),
            Verifier<IMultimediaQuestion>(MultimediaQuestionIsInterviewersOnly, "WB0078", VerificationMessages.WB0078_MultimediaQuestionIsInterviewersOnly),
            Verifier<IMultimediaQuestion>(MultimediaShouldNotHaveValidationExpression, "WB0079", VerificationMessages.WB0079_MultimediaShouldNotHaveValidationExpression),
            Verifier<IGroup, IComposite>(QuestionsCannotBeUsedAsRosterTitle, "WB0083", VerificationMessages.WB0083_QuestionCannotBeUsedAsRosterTitle),
            Verifier<IQuestion, IComposite>(CascadingComboboxOptionsHasNoParentOptions, "WB0084", VerificationMessages.WB0084_CascadingOptionsShouldHaveParent),
            Verifier<IQuestion, IComposite>(ParentShouldNotHaveDeeperRosterLevelThanCascadingQuestion, "WB0085", VerificationMessages.WB0085_CascadingQuestionWrongParentLevel),
            Verifier<SingleQuestion>(this.SingleOptionQuestionHasNonIntegerOptionsValues, "WB0114", VerificationMessages.WB0114_SingleOptionQuestionSupportsOnlyIntegerPositiveValues),
            Verifier<SingleQuestion>(CascadingQuestionReferencesMissingParent, "WB0086", VerificationMessages.WB0086_ParentCascadingQuestionShouldExist),
            Verifier<SingleQuestion, SingleQuestion>(CascadingHasCircularReference, "WB0087", VerificationMessages.WB0087_CascadingQuestionHasCicularReference),
            Verifier<SingleQuestion>(CascadingQuestionHasMoreThanAllowedOptions, "WB0088", VerificationMessages.WB0088_CascadingQuestionShouldHaveAllowedAmountOfAnswers),
            Verifier<SingleQuestion>(CascadingQuestionOptionsWithParentValuesShouldBeUnique, "WB0089", VerificationMessages.WB0089_CascadingQuestionOptionWithParentShouldBeUnique),
            Verifier<IQuestion>(LinkedQuestionIsInterviewersOnly, "WB0090", VerificationMessages.WB0090_LinkedQuestionIsInterviewersOnly),
            Verifier<SingleQuestion>(CascadingQuestionHasEnablementCondition, "WB0091", VerificationMessages.WB0091_CascadingChildQuestionShouldNotContainCondition),
            Verifier<SingleQuestion>(CascadingQuestionHasValidationExpresssion, "WB0092", VerificationMessages.WB0092_CascadingChildQuesionShouldNotContainValidation),
            Verifier<IComposite>(this.ConditionExpresssionHasLengthMoreThan10000Characters, "WB0094", VerificationMessages.WB0094_ConditionExpresssionHasLengthMoreThan10000Characters),
            Verifier(QuestionnaireTitleHasInvalidCharacters, "WB0097", VerificationMessages.WB0097_QuestionnaireTitleHasInvalidCharacters),
            Verifier(QuestionnaireHasSizeMoreThan5MB, "WB0098", size => VerificationMessages.WB0098_QuestionnaireHasSizeMoreThan5MB.FormatString(size)),
            Verifier<IQuestion>(LinkedQuestionFilterExpressionHasLengthMoreThan10000Characters, "WB0108", VerificationMessages.WB0108_LinkedQuestionFilterExpresssionHasLengthMoreThan10000Characters),
            Verifier<IVariable>(VariableExpressionHasLengthMoreThan10000Characters, "WB0005", VerificationMessages.WB0005_VariableExpressionHasLengthMoreThan10000Characters),
            Verifier<IQuestion, IComposite>(this.CategoricalLinkedQuestionUsedInFilterExpression, "WB0109", VerificationMessages.WB0109_CategoricalLinkedQuestionUsedInLinkedQuestionFilterExpresssion),
            Verifier<IComposite, ValidationCondition>(GetValidationConditionsOrEmpty, ValidationConditionIsTooLong, "WB0104", index => string.Format(VerificationMessages.WB0104_ValidationConditionIsTooLong, index)),
            TranslationVerifier<IComposite, ValidationCondition>(GetValidationConditionsOrEmpty, ValidationMessageIsTooLong, "WB0105", index => string.Format(VerificationMessages.WB0105_ValidationMessageIsTooLong, index)),
            Verifier<IComposite, ValidationCondition>(GetValidationConditionsOrEmpty, ValidationConditionIsEmpty, "WB0106", index => string.Format(VerificationMessages.WB0106_ValidationConditionIsEmpty, index)),
            TranslationVerifier<IComposite, ValidationCondition>(GetValidationConditionsOrEmpty, ValidationMessageIsEmpty, "WB0107", index => string.Format(VerificationMessages.WB0107_ValidationMessageIsEmpty, index)),
            Verifier<IQuestion>(OptionFilterExpressionHasLengthMoreThan10000Characters, "WB0028", VerificationMessages.WB0028_OptionsFilterExpressionHasLengthMoreThan10000Characters),
            Verifier<IQuestion>(QuestionWithOptionsFilterCannotBePrefilled, "WB0029", VerificationMessages.WB0029_QuestionWithOptionsFilterCannotBePrefilled),
            TranslationVerifier<IQuestion>(QuestionTitleIsTooLong, "WB0259", VerificationMessages.WB0259_QuestionTitleIsTooLong),

            TranslationVerifier<IStaticText>(StaticTextIsEmpty, "WB0071", VerificationMessages.WB0071_StaticTextIsEmpty),
            Verifier<IStaticText>(StaticTextRefersAbsentAttachment, "WB0095", VerificationMessages.WB0095_StaticTextRefersAbsentAttachment),

            Verifier<IVariable>(VariableHasInvalidName, "WB0112", VerificationMessages.WB0112_VariableHasInvalidName),
            Verifier<IVariable>(VariableHasEmptyVariableName, "WB0113", VerificationMessages.WB0113_VariableHasEmptyVariableName, VerificationMessageLevel.Critical),
            Verifier<IVariable>(VariableHasEmptyExpression, "WB0004", VerificationMessages.WB0004_VariableHasEmptyExpression, VerificationMessageLevel.Critical),

            MacrosVerifier(MacroHasEmptyName, "WB0014", VerificationMessages.WB0014_MacroHasEmptyName),
            MacrosVerifier(MacroHasInvalidName, "WB0010", VerificationMessages.WB0010_MacroHasInvalidName),

            LookupVerifier(LookupTableNameIsKeyword, "WB0052", VerificationMessages.WB0052_LookupNameIsKeyword),
            LookupVerifier(LookupTableHasInvalidName, "WB0024", VerificationMessages.WB0024_LookupHasInvalidName),
            LookupVerifier(LookupTableHasEmptyName, "WB0025", VerificationMessages.WB0025_LookupHasEmptyName),
            LookupVerifier(LookupTableHasEmptyContent, "WB0048", VerificationMessages.WB0048_LookupHasEmptyContent),
            LookupVerifier(LookupTableHasInvalidHeaders, "WB0031", VerificationMessages.WB0031_LookupTableHasInvalidHeaders),
            LookupVerifier(LookupTableMoreThan10Columns, "WB0043", VerificationMessages.WB0043_LookupTableMoreThan11Columns),
            LookupVerifier(LookupTableMoreThan5000Rows, "WB0044", VerificationMessages.WB0044_LookupTableMoreThan5000Rows),
            LookupVerifier(LookupTableNotUniqueRowcodeValues, "WB0047", VerificationMessages.WB0047_LookupTableNotUniqueRowcodeValues),

            AttachmentVerifier(AttachmentHasEmptyContent, "WB0111", VerificationMessages.WB0111_AttachmentHasEmptyContent),

            TranslationVerifier(TranslationNameIsInvalid, "WB0256", VerificationMessages.WB0256_TranslationNameIsInvalid),
            TranslationVerifier(TranslationHasEmptyContent, "WB0257", VerificationMessages.WB0257_TranslationHasEmptyContent),
            TranslationVerifier(TranslationsHasDuplicatedNames, "WB0258", VerificationMessages.WB0258_TranslationsHasDuplicatedNames),

            VerifyGpsPrefilledQuestions,
            ErrorsByCircularReferences,
            ErrorsByLinkedQuestions,
            this.ErrorsBySubstitutions,
            ErrorsByMacrosWithDuplicateName,
            ErrorsByAttachmentsWithDuplicateName,
            ErrorsByLookupTablesWithDuplicateVariableName,
            ErrorsByQuestionnaireEntitiesShareSameInternalId,
        };

        public IEnumerable<QuestionnaireVerificationMessage> Verify(QuestionnaireDocument questionnaire)
        {
            var readOnlyQuestionnaireDocument = questionnaire.AsReadOnly();

            List<ReadOnlyQuestionnaireDocument> translatedQuestionnaires = new List<ReadOnlyQuestionnaireDocument>();
            questionnaire.Translations.ForEach(t =>
            {
                var translation = this.translationService.Get(questionnaire.PublicKey, t.Id);
                var translatedQuestionnaireDocument = this.questionnaireTranslator.Translate(questionnaire, translation);
                translatedQuestionnaires.Add(new ReadOnlyQuestionnaireDocument(translatedQuestionnaireDocument, t.Name));
            });

            var multiLanguageQuestionnaireDocument = new MultiLanguageQuestionnaireDocument(readOnlyQuestionnaireDocument,
                translatedQuestionnaires.ToArray());

            var verificationMessagesByQuestionnaire =
                (from verifier in this.AtomicVerifiers
                    let errors = verifier.Invoke(multiLanguageQuestionnaireDocument)
                    from error in errors
                    select error).ToList();

            if (verificationMessagesByQuestionnaire.Any(e => e.MessageLevel == VerificationMessageLevel.Critical))
                return verificationMessagesByQuestionnaire;

            if (this.HasQuestionnaireExpressionsWithExceedLength(multiLanguageQuestionnaireDocument))
                return verificationMessagesByQuestionnaire;

            var verificationMessagesByCompiler = this.ErrorsByCompiler(questionnaire).ToList();

            return verificationMessagesByQuestionnaire.Concat(verificationMessagesByCompiler);
        }

        public IEnumerable<QuestionnaireVerificationMessage> CheckForErrors(QuestionnaireDocument questionnaire)
        {
            return this.Verify(questionnaire).Where(x => x.MessageLevel != VerificationMessageLevel.Warning);
        }

        private static IEnumerable<QuestionnaireVerificationMessage> ErrorsByQuestionnaireEntitiesShareSameInternalId(MultiLanguageQuestionnaireDocument questionnaire)
        {
            return questionnaire
                    .GetAllEntitiesIdAndTypePairsInQuestionnaireFlowOrder()
                    .GroupBy(x => x.Id)
                    .Where(group => group.Count() > 1)
                    .Select(group =>
                            QuestionnaireVerificationMessage.Error(
                                "WB0102",
                                VerificationMessages.WB0102_QuestionnaireEntitiesShareSameInternalId,
                                group.Select(x => new QuestionnaireVerificationReference(GetReferenceTypeByItemTypeAndId(questionnaire, x.Id, x.Type), x.Id)).ToArray()));
        }

        private static QuestionnaireVerificationReferenceType GetReferenceTypeByItemTypeAndId(MultiLanguageQuestionnaireDocument questionnaire, Guid id, Type entityType)
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
            return question.YesNoView && (question.LinkedToQuestionId.HasValue || question.LinkedToRosterId.HasValue);
        }

        private bool MultiOptionQuestionHasNonIntegerOptionsValues(IMultyOptionsQuestion question)
            => HaveOptionsNonIntegerValues(question.Answers);

        private bool SingleOptionQuestionHasNonIntegerOptionsValues(SingleQuestion question)
            => HaveOptionsNonIntegerValues(question.Answers);

        private static bool HaveOptionsNonIntegerValues(List<Answer> answers)
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


        private IEnumerable<QuestionnaireVerificationMessage> VerifyGpsPrefilledQuestions(MultiLanguageQuestionnaireDocument document)
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

        private bool ConditionExpresssionHasLengthMoreThan10000Characters(IComposite entity, MultiLanguageQuestionnaireDocument questionnaire)
            => this.DoesExpressionExceed1000CharsLimit(questionnaire, GetCustomEnablementCondition(entity));

        private bool OptionFilterExpressionHasLengthMoreThan10000Characters(IQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
            => this.DoesExpressionExceed1000CharsLimit(questionnaire, question.Properties.OptionsFilterExpression);

        private bool VariableExpressionHasLengthMoreThan10000Characters(IVariable variable, MultiLanguageQuestionnaireDocument questionnaire)
            => this.DoesExpressionExceed1000CharsLimit(questionnaire, variable.Expression);

        private bool LinkedQuestionFilterExpressionHasLengthMoreThan10000Characters(IQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (!(question.LinkedToQuestionId.HasValue || question.LinkedToRosterId.HasValue))
                return false;

            return this.DoesExpressionExceed1000CharsLimit(questionnaire, question.LinkedFilterExpression);
        }

        private bool DoesExpressionExceed1000CharsLimit(MultiLanguageQuestionnaireDocument questionnaire, string expression)
        {
            if (string.IsNullOrEmpty(expression))
                return false;

            var expressionWithInlinedMacroses = this.macrosSubstitutionService.InlineMacros(expression, questionnaire.Macros.Values);

            return expressionWithInlinedMacroses.Length > MaxExpressionLength;
        }

        private static bool CascadingQuestionHasValidationExpresssion(SingleQuestion question)
        {
            return question.CascadeFromQuestionId.HasValue && !string.IsNullOrWhiteSpace(question.ValidationExpression);
        }

        private static bool CascadingQuestionHasEnablementCondition(SingleQuestion question)
        {
            return question.CascadeFromQuestionId.HasValue && !string.IsNullOrWhiteSpace(question.ConditionExpression);
        }

        private IEnumerable<QuestionnaireVerificationMessage> ErrorsByCompiler(QuestionnaireDocument questionnaire)
        {
            var compilationResult = GetCompilationResult(questionnaire);

            if (compilationResult.Success)
                yield break;

            var elementsWithErrorMessages = compilationResult.Diagnostics.GroupBy(x => x.Location, x => x.Message);
            foreach (var elementWithErrors in elementsWithErrorMessages)
            {
                yield return CreateExpressionSyntaxError(new ExpressionLocation(elementWithErrors.Key), elementWithErrors.ToList());
            }
        }

        private static Tuple<bool, decimal> QuestionnaireHasSizeMoreThan5MB(MultiLanguageQuestionnaireDocument questionnaire)
        {
            var jsonQuestionnaire = JsonConvert.SerializeObject(questionnaire.Questionnaire, Formatting.None);
            var questionnaireByteCount = Encoding.UTF8.GetByteCount(jsonQuestionnaire);
            var isOversized = questionnaireByteCount > 5 * 1024 * 1024; // 5MB
            var questionnaireMegaByteCount = (decimal)questionnaireByteCount / 1024 / 1024;
            return new Tuple<bool, decimal>(isOversized, questionnaireMegaByteCount);
        }

        private GenerationResult GetCompilationResult(QuestionnaireDocument questionnaire)
        {
            string resultAssembly;

            return this.expressionProcessorGenerator.GenerateProcessorStateAssembly(
                questionnaire, this.engineVersionService.GetQuestionnaireContentVersion(questionnaire),
                out resultAssembly);
        }

        private static bool CascadingQuestionOptionsWithParentValuesShouldBeUnique(SingleQuestion question)
        {
            if (question.Answers != null && question.CascadeFromQuestionId.HasValue)
            {
                var enumerable = question.Answers.Select(x => new { x.AnswerText, x.ParentValue })
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

        private static EntityVerificationResult<SingleQuestion> CascadingHasCircularReference(SingleQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
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

        private static bool CascadingQuestionReferencesMissingParent(SingleQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (!question.CascadeFromQuestionId.HasValue)
                return false;

            return questionnaire.Find<SingleQuestion>(question.CascadeFromQuestionId.Value) == null;
        }

        private static EntityVerificationResult<IComposite> ParentShouldNotHaveDeeperRosterLevelThanCascadingQuestion(IQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (!question.CascadeFromQuestionId.HasValue)
                return new EntityVerificationResult<IComposite> { HasErrors = false };

            var parentQuestion = questionnaire.Find<IQuestion>(question.CascadeFromQuestionId.Value);
            if (parentQuestion == null)
                return new EntityVerificationResult<IComposite> { HasErrors = false };

            var parentRosters = GetAllRosterSizeQuestionsAsVector(parentQuestion, questionnaire);

            parentRosters = parentRosters.Reverse().ToArray();

            var questionRosters = GetAllRosterSizeQuestionsAsVector(question, questionnaire).Reverse().ToArray();

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

        private static EntityVerificationResult<IComposite> CascadingComboboxOptionsHasNoParentOptions(IQuestion question, MultiLanguageQuestionnaireDocument document)
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
            return IsFilteredComboboxQuestion(question) && (question.LinkedToQuestionId.HasValue || question.LinkedToRosterId.HasValue);
        }

        private static bool StaticTextRefersAbsentAttachment(IStaticText staticText, MultiLanguageQuestionnaireDocument document)
        {
            if (string.IsNullOrWhiteSpace(staticText.AttachmentName))
                return false;

            return document.Attachments.All(x => x.Name != staticText.AttachmentName);
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
                var answersValues =  question.Answers.Where(x => !string.IsNullOrWhiteSpace(x.AnswerValue)).Select(x => x.AnswerValue.Trim()).ToList();
                return answersValues.Distinct().Count() != answersValues.Count();
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

            if (question.LinkedToQuestionId.HasValue || question.LinkedToRosterId.HasValue)
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

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> MacrosVerifier(
            Func<Macro, MultiLanguageQuestionnaireDocument, bool> hasError, string code, string message)
        {
            return (questionnaire) => questionnaire
                    .Macros
                    .Where(entity => hasError(entity.Value, questionnaire))
                    .Select(entity => QuestionnaireVerificationMessage.Error(code, message, QuestionnaireVerificationReference.CreateForMacro(entity.Key)));
        }

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> AttachmentVerifier(
            Func<Attachment, MultiLanguageQuestionnaireDocument, bool> hasError, string code, string message)
        {
            return (questionnaire) => questionnaire
                    .Attachments
                    .Where(entity => hasError(entity, questionnaire))
                    .Select(entity => QuestionnaireVerificationMessage.Error(code, message, QuestionnaireVerificationReference.CreateForAttachment(entity.AttachmentId)));
        }

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> TranslationVerifier(
            Func<Translation, MultiLanguageQuestionnaireDocument, bool> hasError, string code, string message)
        {
            return (questionnaire) => questionnaire
                    .Translations
                    .Where(entity => hasError(entity, questionnaire))
                    .Select(entity => QuestionnaireVerificationMessage.Error(code, message, QuestionnaireVerificationReference.CreateForTranslation(entity.Id)));
        }

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> LookupVerifier(
            Func<Guid, LookupTable, MultiLanguageQuestionnaireDocument, bool> hasError, string code, string message)
        {
            return (questionnaire) => questionnaire
                    .LookupTables
                    .Where(entity => hasError(entity.Key, entity.Value, questionnaire))
                    .Select(entity => QuestionnaireVerificationMessage.Critical(code, message, QuestionnaireVerificationReference.CreateForLookupTable(entity.Key)));
        }

        private Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> LookupVerifier(
            Func<LookupTable, LookupTableContent, MultiLanguageQuestionnaireDocument, bool> hasError, string code, string message)
        {
            return (questionnaire) =>
                from lookupTable in questionnaire.LookupTables
                let lookupTableContent = this.lookupTableService.GetLookupTableContent(questionnaire.PublicKey, lookupTable.Key)
                where lookupTableContent != null
                where hasError(lookupTable.Value, lookupTableContent, questionnaire)
                select QuestionnaireVerificationMessage.Critical(code, message, QuestionnaireVerificationReference.CreateForLookupTable(lookupTable.Key));
        }

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> Verifier(
            Func<MultiLanguageQuestionnaireDocument, bool> hasError, string code, string message)
        {
            return (questionnaire) =>
                hasError(questionnaire)
                    ? new[] { QuestionnaireVerificationMessage.Error(code, message) }
                    : Enumerable.Empty<QuestionnaireVerificationMessage>();
        }

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> Verifier<TArg>(
            Func<MultiLanguageQuestionnaireDocument, Tuple<bool, TArg>> hasError, string code, Func<TArg, string> messageBuilder)
        {
            return (questionnaire) =>
            {
                var errorCheckResult = hasError(questionnaire);
                return errorCheckResult.Item1
                    ? new[] { QuestionnaireVerificationMessage.Error(code, messageBuilder.Invoke(errorCheckResult.Item2)) }
                    : Enumerable.Empty<QuestionnaireVerificationMessage>();
            };
        }

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> Verifier<TEntity>(Func<TEntity, bool> hasError, string code, string message, VerificationMessageLevel level = VerificationMessageLevel.General)
            where TEntity : class, IComposite
        {
            return (questionnaire) =>
                questionnaire
                    .Find<TEntity>(hasError)
                    .Select(entity => level == VerificationMessageLevel.General 
                        ? QuestionnaireVerificationMessage.Error(code, message, CreateReference(entity))
                        : QuestionnaireVerificationMessage.Critical(code, message, CreateReference(entity)));
        }

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> TranslationVerifier<TEntity>(Func<TEntity, bool> hasError, string code, string message, VerificationMessageLevel level = VerificationMessageLevel.General)
            where TEntity : class, IComposite
        {
            return (questionnaire) =>
                    questionnaire
                        .FindWithTranslations<TEntity>(hasError)
                            .Select(translatedEntity =>
                            {
                                var translationMessage = translatedEntity.TranslationName == null
                                    ? message
                                    : translatedEntity.TranslationName + ": " + message;
                                var questionnaireVerificationReference = CreateReference(translatedEntity.Entity);
                                return level == VerificationMessageLevel.General
                                    ? QuestionnaireVerificationMessage.Error(code, translationMessage, questionnaireVerificationReference)
                                    : QuestionnaireVerificationMessage.Critical(code, translationMessage, questionnaireVerificationReference);
                            }
                      );
        }

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> Verifier<TEntity, TSubEntity>(
            Func<TEntity, IEnumerable<TSubEntity>> getSubEnitites, Func<TSubEntity, bool> hasError, string code, Func<int, string> getMessageBySubEntityIndex)
            where TEntity : class, IComposite
        {
            return Verifier(getSubEnitites, (entity, subEntity, questionnaire) => hasError(subEntity), code, getMessageBySubEntityIndex);
        }

        private Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> TranslationVerifier<TEntity, TSubEntity>(
            Func<TEntity, IEnumerable<TSubEntity>> getSubEnitites, Func<TSubEntity, bool> hasError, string code, Func<int, string> getMessageBySubEntityIndex)
            where TEntity : class, IComposite
        {
            return TranslationVerifier(getSubEnitites, (entity, subEntity, questionnaire) => hasError(subEntity), code, getMessageBySubEntityIndex);
        }

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> Verifier<TEntity, TSubEntity>(
            Func<TEntity, IEnumerable<TSubEntity>> getSubEnitites, Func<TEntity, TSubEntity, MultiLanguageQuestionnaireDocument, bool> hasError, string code, Func<int, string> getMessageBySubEntityIndex)
            where TEntity : class, IComposite
        {
            return (questionnaire) =>
                questionnaire
                    .Find<TEntity>(entity => true)
                    .SelectMany(entity => getSubEnitites(entity).Select((subEntity, index) => new { Entity = entity, SubEntity = subEntity, Index = index }))
                    .Where(descriptor => hasError(descriptor.Entity, descriptor.SubEntity, questionnaire))
                    .Select(descriptor => QuestionnaireVerificationMessage.Error(code, getMessageBySubEntityIndex(descriptor.Index + 1), CreateReference(descriptor.Entity, descriptor.Index)));
        }

        private Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> TranslationVerifier<TEntity, TSubEntity>(
            Func<TEntity, IEnumerable<TSubEntity>> getSubEnitites, Func<TEntity, TSubEntity, MultiLanguageQuestionnaireDocument, bool> hasError, string code, Func<int, string> getMessageBySubEntityIndex)
            where TEntity : class, IComposite
        {
            return (questionnaire) =>
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

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> Verifier<TEntity>(
            Func<TEntity, MultiLanguageQuestionnaireDocument, bool> hasError, string code, string message)
            where TEntity : class, IComposite
        {
            return (questionnaire) =>
                questionnaire
                    .Find<TEntity>(entity => hasError(entity, questionnaire))
                    .Select(entity => QuestionnaireVerificationMessage.Error(code, message, CreateReference(entity)));
        }

        private static Func<MultiLanguageQuestionnaireDocument, IEnumerable<QuestionnaireVerificationMessage>> Verifier<TEntity, TReferencedEntity>(
            Func<TEntity, MultiLanguageQuestionnaireDocument, EntityVerificationResult<TReferencedEntity>> verifyEntity, string code, string message)
            where TEntity : class, IComposite
            where TReferencedEntity : class, IComposite
        {
            return (questionnaire) =>
                from entity in questionnaire.Find<TEntity>(_ => true)
                let verificationResult = verifyEntity(entity, questionnaire)
                where verificationResult.HasErrors
                select QuestionnaireVerificationMessage.Error(code, message, verificationResult.ReferencedEntities.Select(x => CreateReference(x)).ToArray());
        }

        private static bool MacroHasEmptyName(Macro macro, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return string.IsNullOrWhiteSpace(macro.Name);
        }

        private static bool MacroHasInvalidName(Macro macro, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return !IsVariableNameValid(macro.Name);
        }

        private bool AttachmentHasEmptyContent(Attachment attachment, MultiLanguageQuestionnaireDocument questionnaire)
        {
            var attachmentContent = this.attachmentService.GetContentDetails(attachment.ContentId);
            return attachmentContent == null || attachmentContent.Size == 0;
        }

        private bool TranslationNameIsInvalid(Translation translation, MultiLanguageQuestionnaireDocument questionnaire)
        {
            var names = questionnaire.Translations.Select(t => t.Name);
            return names.All(name =>string.IsNullOrWhiteSpace(name) || name.Length > 32);
        }

        private bool TranslationHasEmptyContent(Translation translation, MultiLanguageQuestionnaireDocument questionnaire)
        {
            var trans = this.translationService.Get(questionnaire.PublicKey, translation.Id);
            return trans?.IsEmpty() ?? true;
        }

        private bool TranslationsHasDuplicatedNames(Translation translation, MultiLanguageQuestionnaireDocument questionnaire)
        {
            var trimedTranslationName = translation.Name.Trim();
            var countNames = questionnaire.Questionnaire.Translations.Count(t => t.Name.Trim() == trimedTranslationName);
            return countNames > 1;
        }

        private static bool LookupTableHasEmptyName(Guid tableId, LookupTable table, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return string.IsNullOrWhiteSpace(table.TableName);
        }

        private static bool LookupTableHasInvalidName(Guid tableId, LookupTable table, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return !IsVariableNameValid(table.TableName);
        }

        private bool LookupTableNameIsKeyword(Guid tableId, LookupTable table, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (string.IsNullOrWhiteSpace(table.FileName))
                return false;
            return keywordsProvider.GetAllReservedKeywords().Contains(table.TableName.ToLower());
        }

        private bool LookupTableHasEmptyContent(Guid tableId, LookupTable table, MultiLanguageQuestionnaireDocument questionnaire)
        {
            var lookupTableContent = this.lookupTableService.GetLookupTableContent(questionnaire.PublicKey, tableId);
            return lookupTableContent == null;
        }

        private static bool LookupTableHasInvalidHeaders(LookupTable table, LookupTableContent tableContent, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return !tableContent.VariableNames.All(IsVariableNameValid);
        }

        private static bool LookupTableMoreThan10Columns(LookupTable table, LookupTableContent tableContent, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return tableContent.VariableNames.Count() > 10;
        }

        private static bool LookupTableMoreThan5000Rows(LookupTable table, LookupTableContent tableContent, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return tableContent.Rows.Count() > 5000;
        }

        private static bool LookupTableNotUniqueRowcodeValues(LookupTable table, LookupTableContent tableContent, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return tableContent.Rows.Select(x => x.RowCode).Distinct().Count() != tableContent.Rows.Count();
        }

        private static bool CategoricalMultiAnswersQuestionHasOptionsCountLessThanMaxAllowedAnswersCount(IMultyOptionsQuestion question)
        {
            return question.MaxAllowedAnswers.HasValue && !(question.LinkedToQuestionId.HasValue || question.LinkedToRosterId.HasValue) &&
                   (question.MaxAllowedAnswers.Value > question.Answers.Count);
        }

        private static bool GroupWhereRosterSizeSourceIsQuestionHasNoRosterSizeQuestion(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return IsRosterByQuestion(group) && GetRosterSizeQuestionByRosterGroup(group, questionnaire) == null;
        }

        private static bool RosterSizeSourceQuestionTypeIsIncorrect(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
        {
            var rosterSizeQuestion = GetRosterSizeQuestionByRosterGroup(group, questionnaire);
            if (rosterSizeQuestion == null)
                return false;

            return !IsQuestionAllowedToBeRosterSizeSource(rosterSizeQuestion);
        }

        private static bool LongMultiRosterCannotBeNested(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return IsRosterByQuestion(@group) && IsLongRosterNested(group, questionnaire, (g, q) => (GetRosterSizeQuestionByRosterGroup(g, q) as MultyOptionsQuestion)?.MaxAllowedAnswers);
        }

        private static bool LongListRosterCannotBeNested(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return IsRosterByQuestion(@group) && IsLongRosterNested(group, questionnaire, (g, q) => (GetRosterSizeQuestionByRosterGroup(g, q) as TextListQuestion)?.MaxAnswerCount);
        }

        private static bool LongFixedRosterCannotBeNested(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return IsFixedRoster(group) && IsLongRosterNested(group, questionnaire, (g, q) => group.FixedRosterTitles.Length);
        }

        private static bool LongMultiRosterCannotHaveNestedRosters(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return IsRosterByQuestion(@group) && IsLongRosterHasNestedRosters(group, questionnaire, (g, q) => (GetRosterSizeQuestionByRosterGroup(g, q) as TextListQuestion)?.MaxAnswerCount);
        }

        private static bool LongListRosterCannotHaveNestedRosters(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return IsRosterByQuestion(@group) && IsLongRosterHasNestedRosters(group, questionnaire, (g, q) => (GetRosterSizeQuestionByRosterGroup(g, q) as MultyOptionsQuestion)?.MaxAllowedAnswers);
        }

        private static bool LongFixedRosterCannotHaveNestedRosters(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return IsFixedRoster(@group) && IsLongRosterHasNestedRosters(group, questionnaire, (g, q) => @group.FixedRosterTitles.Length);
        }

        private static bool IsLongRosterNested(IGroup group, MultiLanguageQuestionnaireDocument questionnaire, 
            Func<IGroup, MultiLanguageQuestionnaireDocument, int?> getRosterSizeLimitSetByUser)
        {
            var rosterSizeLimitSetByUser = getRosterSizeLimitSetByUser(@group, questionnaire);
            if (rosterSizeLimitSetByUser == null)
                return false;

            return rosterSizeLimitSetByUser > Constants.MaxRosterRowCount && GetRosterLevel(@group) > 1;
        }

        private static bool IsLongRosterHasNestedRosters(IGroup group, MultiLanguageQuestionnaireDocument questionnaire,
            Func<IGroup, MultiLanguageQuestionnaireDocument, int?> getRosterSizeLimitSetByUser)
        {
            var rosterSizeLimitSetByUser = getRosterSizeLimitSetByUser(@group, questionnaire);
            if (rosterSizeLimitSetByUser == null)
                return false;

            return rosterSizeLimitSetByUser > Constants.MaxRosterRowCount && GroupHasNestedRosters(@group, questionnaire);
        }

        private static bool GroupHasNestedRosters(IGroup @group, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return questionnaire
                .Find<IGroup>()
                .Any(x => GetSpecifiedGroupAndAllItsParentGroupsStartingFromBottom(x, questionnaire)
                        .Select(r => r.PublicKey)
                        .Contains(@group.PublicKey));
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

        private static bool GroupWhereRosterSizeSourceIsQuestionHasInvalidRosterTitleQuestion(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
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

            Guid[] rosterScopeForGroup = GetAllRosterSizeQuestionsAsVector(group, questionnaire);
            Guid[] rosterScopeForTitleQuestion = GetAllRosterSizeQuestionsAsVector(rosterTitleQuestion, questionnaire);

            if (!Enumerable.SequenceEqual(rosterScopeForGroup, rosterScopeForTitleQuestion))
                return true;

            return false;
        }

        private static bool GroupWhereRosterSizeIsCategoricalMultyAnswerQuestionHaveRosterTitleQuestion(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
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

        private static bool GroupWhereRosterSizeSourceIsFixedTitlesValuesHaveNonIntegerValues(IGroup group)
        {
            if (!IsRosterByFixedTitles(group))
                return false;

            if (group.FixedRosterTitles == null)
                return false;

            if (group.FixedRosterTitles.Length == 0)
                return false;

            foreach (var rosterTitle in group.FixedRosterTitles)
            {
                if((rosterTitle.Value % 1) != 0)
                    return true;
            }

            return false;
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

        private static bool RosterSizeQuestionMaxValueCouldBeInRange1And40(IQuestion question, MultiLanguageQuestionnaireDocument questionnaire, Func<IQuestion, int?> getRosterSizeQuestionMaxValue)
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

        private static bool ValidationConditionIsTooLong(ValidationCondition validationCondition)
            => validationCondition.Expression?.Length > MaxExpressionLength;

        private static bool ValidationMessageIsTooLong(ValidationCondition validationCondition)
            => validationCondition.Message?.Length > 250;

        private static bool QuestionTitleIsTooLong(IQuestion question)
            => question.QuestionText?.Length > 500;

        private static bool GroupTitleIsTooLong(IGroup group)
            => group.Title?.Length > 500;

        private static bool ValidationConditionIsEmpty(ValidationCondition validationCondition)
            => string.IsNullOrWhiteSpace(validationCondition.Expression);

        private static bool ValidationMessageIsEmpty(ValidationCondition validationCondition)
            => string.IsNullOrWhiteSpace(validationCondition.Message);

        private static bool IsQuestionAllowedToBeRosterSizeSource(IQuestion question)
            => IsNumericRosterSizeQuestion(question) ||
               IsCategoricalRosterSizeQuestion(question) ||
               IsTextListQuestion(question);

        private static bool IsTextListQuestion(IQuestion question)
            => question.QuestionType == QuestionType.TextList;

        private static bool IsNumericRosterSizeQuestion(IQuestion question)
        {
            var numericQuestion = question as NumericQuestion;
            return numericQuestion != null && numericQuestion.IsInteger;
        }

        private static bool IsCategoricalRosterSizeQuestion(IQuestion question)
        {
            return IsCategoricalMultiAnswersQuestion(question) && !(question.LinkedToQuestionId.HasValue || question.LinkedToRosterId.HasValue);
        }

        private static bool IsCategoricalMultiAnswersQuestion(IQuestion question)
        {
            return question is MultyOptionsQuestion;
        }

        private static bool IsCategoricalSingleAnswerQuestion(IQuestion question)
        {
            return question is SingleQuestion;
        }

        private static bool PrefilledQuestionCantBeInsideOfRoster(IQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return IsPreFilledQuestion(question) && GetAllParentGroupsForQuestion(question, questionnaire).Any(IsRosterGroup);
        }

        private static bool QuestionWithOptionsFilterCannotBePrefilled(IQuestion question)
        {
            return !string.IsNullOrWhiteSpace(question.Properties.OptionsFilterExpression) && IsPreFilledQuestion(question);
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

            if (question.LinkedToQuestionId.HasValue || question.LinkedToRosterId.HasValue)
                return false;

            return question.Answers.Any(option => string.IsNullOrWhiteSpace(option.AnswerValue));
        }

        private static bool QuestionnaireTitleHasInvalidCharacters(MultiLanguageQuestionnaireDocument questionnaire)
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
        
        private static bool VariableHasEmptyExpression(IVariable variable)
        {
            return string.IsNullOrWhiteSpace(variable.Expression);
        }

        private static bool VariableHasEmptyVariableName(IVariable variable)
        {
            return string.IsNullOrWhiteSpace(variable.Name);
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

        private static bool VariableHasInvalidName(IVariable variable)
        {
            if (string.IsNullOrWhiteSpace(variable.Name))
                return false;
            return !IsVariableNameValid(variable.Name);
        }

        private static bool RosterHasInvalidVariableName(IGroup group)
        {
            if (!group.IsRoster)
                return false;
            return !IsVariableNameValid(group.VariableName);
        }

        private bool RosterHasVariableNameEqualToQuestionnaireTitle(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (!group.IsRoster)
                return false;

            if (string.IsNullOrEmpty(group.VariableName))
                return false;

            var questionnaireVariableName = this.fileSystemAccessor.MakeStataCompatibleFileName(questionnaire.Title);

            return group.VariableName.Equals(questionnaireVariableName, StringComparison.InvariantCultureIgnoreCase);
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
            if (!(question.LinkedToQuestionId.HasValue || question.LinkedToRosterId.HasValue))
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

        private static EntityVerificationResult<IComposite> QuestionsCannotBeUsedAsRosterTitle(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
        {
            var noErrors = new EntityVerificationResult<IComposite> { HasErrors = false };

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

        private EntityVerificationResult<IComposite> VerifyWhetherEntityExpressionReferencesIncorrectQuestions(
            IComposite entity, string expression, MultiLanguageQuestionnaireDocument questionnaire, Func<IComposite, bool> isReferencedQuestionIncorrect)
        {
            if (string.IsNullOrEmpty(expression))
                return new EntityVerificationResult<IComposite> { HasErrors = false };

            IEnumerable<IComposite> incorrectReferencedQuestions = this.expressionProcessor
                .GetIdentifiersUsedInExpression(macrosSubstitutionService.InlineMacros(expression, questionnaire.Macros.Values))
                .Select(identifier => GetEntityByVariable(identifier, questionnaire))
                .Where(referencedQuestion => referencedQuestion != null)
                .Where(isReferencedQuestionIncorrect)
                .ToList();

            if (!incorrectReferencedQuestions.Any())
                return new EntityVerificationResult<IComposite> { HasErrors = false };

            var referencedEntities =
                Enumerable.Concat(entity.ToEnumerable(), incorrectReferencedQuestions).Distinct().ToArray();

            return new EntityVerificationResult<IComposite>
            {
                HasErrors = true,
                ReferencedEntities = referencedEntities
            };
        }

        private static EntityVerificationResult<IComposite> RosterSizeQuestionHasDeeperRosterLevelThanDependentRoster(IGroup roster, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (!IsRosterGroup(roster))
                return new EntityVerificationResult<IComposite> { HasErrors = false };

            var rosterSizeQuestion = GetRosterSizeQuestionByRosterGroup(roster, questionnaire);
            if (rosterSizeQuestion == null)
                return new EntityVerificationResult<IComposite> { HasErrors = false };

            var rosterLevelForRoster = GetAllRosterSizeQuestionsAsVector(roster, questionnaire);

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

        private EntityVerificationResult<IComposite> CategoricalLinkedQuestionUsedInFilterExpression(IQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            if (!(question.LinkedToQuestionId.HasValue || question.LinkedToRosterId.HasValue))
                new EntityVerificationResult<IComposite> { HasErrors = false };

            if (string.IsNullOrEmpty(question.LinkedFilterExpression))
                new EntityVerificationResult<IComposite> { HasErrors = false };

            return this.VerifyWhetherEntityExpressionReferencesIncorrectQuestions(question,
                question.LinkedFilterExpression,
                questionnaire, isReferencedQuestionIncorrect: (q) => q.PublicKey == question.PublicKey);
        }


        private IEnumerable<QuestionnaireVerificationMessage> ErrorsByCircularReferences(
            MultiLanguageQuestionnaireDocument questionnaire)
        {
            var dependencies = new Dictionary<string, string[]>();
            var questionsWithConditions = questionnaire.Find<IQuestion>(question => !string.IsNullOrWhiteSpace(question.ConditionExpression));
            var questionsWithOptionsFilter = questionnaire.Find<IQuestion>(question => !string.IsNullOrWhiteSpace(question.Properties.OptionsFilterExpression));
            var variables = questionnaire.Find<IVariable>(question => !string.IsNullOrWhiteSpace(question.Expression));

            foreach (var question in questionsWithConditions)
            {
                if (question.StataExportCaption != null && !dependencies.ContainsKey(question.StataExportCaption))
                    dependencies.Add(question.StataExportCaption, this.expressionProcessor.GetIdentifiersUsedInExpression(question.ConditionExpression).ToArray());
            }

            foreach (var question in questionsWithOptionsFilter)
            {
                if (question.StataExportCaption != null && !dependencies.ContainsKey(question.StataExportCaption))
                    dependencies.Add(question.StataExportCaption, this.expressionProcessor.GetIdentifiersUsedInExpression(question.Properties.OptionsFilterExpression).ToArray());
            }

            foreach (var variable in variables)
            {
                if (variable.Name != null && !dependencies.ContainsKey(variable.Name))
                    dependencies.Add(variable.Name, this.expressionProcessor.GetIdentifiersUsedInExpression(variable.Expression).ToArray());
            }

            var cycles = topologicalSorter.DetectCycles(dependencies);

            foreach (var cycle in cycles)
            {
                var references =
                    cycle.Select(variable => GetEntityByVariable(variable, questionnaire))
                        .Where(x => x != null)
                        .Select(x => CreateReference(x))
                        .ToArray();

                yield return QuestionnaireVerificationMessage.Error("WB0056", VerificationMessages.WB0056_EntityShouldNotHaveCircularReferences, references);
            }
        }

        private static IEnumerable<QuestionnaireVerificationMessage> ErrorsByLinkedQuestions(
            MultiLanguageQuestionnaireDocument questionnaire)
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

            var questionsLinkedOnRoster = questionnaire.Find<IQuestion>(
                question => question.LinkedToRosterId.HasValue);

            foreach (var questionLinkedOnRoster in questionsLinkedOnRoster)
            {
                var sourceRoster = questionnaire.Find<IGroup>(questionLinkedOnRoster.LinkedToRosterId.Value);
                if (sourceRoster == null)
                {
                    yield return QuestionnaireVerificationMessage.Error("WB0053",
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

        private static IEnumerable<QuestionnaireVerificationMessage> ErrorsByMacrosWithDuplicateName(MultiLanguageQuestionnaireDocument questionnaire)
        {
            return questionnaire
                    .Macros
                    .Where(x => !string.IsNullOrEmpty(x.Value.Name))
                    .GroupBy(x => x.Value.Name, StringComparer.InvariantCultureIgnoreCase)
                    .Where(group => group.Count() > 1)
                    .Select(group =>
                             QuestionnaireVerificationMessage.Error(
                                "WB0020",
                                VerificationMessages.WB0020_NameForMacrosIsNotUnique,
                                group.Select(e => QuestionnaireVerificationReference.CreateForMacro(e.Key)).ToArray()));
        }


        private static IEnumerable<QuestionnaireVerificationMessage> ErrorsByAttachmentsWithDuplicateName(
            MultiLanguageQuestionnaireDocument questionnaire)
        {
            return questionnaire
                    .Attachments
                    .Where(x => !string.IsNullOrEmpty(x.Name))
                    .GroupBy(x => x.Name, StringComparer.InvariantCultureIgnoreCase)
                    .Where(group => group.Count() > 1)
                    .Select(group =>
                            QuestionnaireVerificationMessage.Error(
                                "WB0065",
                                VerificationMessages.WB0065_NameForAyyachmentIsNotUnique,
                                group.Select(e => QuestionnaireVerificationReference.CreateForAttachment(e.AttachmentId)).ToArray()));
        }

        private static IEnumerable<QuestionnaireVerificationMessage> ErrorsByLookupTablesWithDuplicateVariableName(
            MultiLanguageQuestionnaireDocument questionnaire)
        {
            var rosterVariableNameMappedOnRosters = questionnaire
                .Find<IGroup>(g => g.IsRoster && !string.IsNullOrEmpty(g.VariableName))
                .Select(r => new
                {
                    Name = r.VariableName,
                    Reference = QuestionnaireVerificationReference.CreateForRoster(r.PublicKey)
                })
                .Union(questionnaire.Find<IQuestion>(q => true)
                        .Where(x => !string.IsNullOrEmpty(x.StataExportCaption))
                        .Select(r => new
                        {
                            Name = r.StataExportCaption,
                            Reference = CreateReference(r)
                        }))
                .Union(questionnaire.LookupTables.Where(x => !string.IsNullOrEmpty(x.Value.TableName))
                        .Select(r => new
                        {
                            Name = r.Value.TableName,
                            Reference = QuestionnaireVerificationReference.CreateForLookupTable(r.Key)
                        }))
                .Union(questionnaire.Find<IVariable>(x => !string.IsNullOrEmpty(x.Name))
                        .Where(x => !string.IsNullOrEmpty(x.Name))
                        .Select(r => new
                        {
                            Name = r.Name,
                            Reference = CreateReference(r)
                        })
                ).ToList();


            return rosterVariableNameMappedOnRosters
                .GroupBy(s => s.Name, StringComparer.InvariantCultureIgnoreCase)
                .Where(group => group.Count() > 1)
                .Select(group => QuestionnaireVerificationMessage.Critical(
                                "WB0026",
                                VerificationMessages.WB0026_ItemsWithTheSameNamesFound,
                                group.Select(x => x.Reference).ToArray()));
        }

        private IEnumerable<QuestionnaireVerificationMessage> ErrorsBySubstitutions(
            MultiLanguageQuestionnaireDocument questionnaire)
        {
            var foundErrors = new List<QuestionnaireVerificationMessage>();

            var entitiesSupportingSubstitutions = 
                questionnaire.FindWithTranslations<IComposite>(SupportsSubstitutions)
                .ToList();

            foreach (var translatedEntity in entitiesSupportingSubstitutions)
            {
                foundErrors.AddRange(this.GetErrorsBySubstitutionsInEntityTitle(translatedEntity, translatedEntity.Entity.GetTitle(), questionnaire));

                var entityAsValidatable = translatedEntity.Entity as IValidatable;

                if (entityAsValidatable != null)
                {
                    var validationConditions = entityAsValidatable.ValidationConditions;

                    for (int validationConditionIndex = 0; validationConditionIndex < validationConditions.Count; validationConditionIndex++)
                    {
                        var validationCondition = validationConditions[validationConditionIndex];
                        foundErrors.AddRange(this.GetErrorsBySubstitutionsInValidationCondition(
                            translatedEntity, validationCondition.Message, validationConditionIndex, questionnaire));
                    }
                }
            }

            return foundErrors.Distinct(new QuestionnaireVerificationMessage.CodeAndReferencesAndTranslationComparer());
        }

        private IEnumerable<QuestionnaireVerificationMessage> GetErrorsBySubstitutionsInEntityTitle(MultiLanguageQuestionnaireDocument.TranslatedEntity<IComposite> translatedEntity, string title, MultiLanguageQuestionnaireDocument questionnaire)
        {
            string[] substitutionReferences = this.substitutionService.GetAllSubstitutionVariableNames(title);

            if (!substitutionReferences.Any())
                return Enumerable.Empty<QuestionnaireVerificationMessage>();

            var question = translatedEntity.Entity as IQuestion;
            if (question != null && IsPreFilledQuestion(question))
                return QuestionWithTitleSubstitutionCantBePrefilled(question).ToEnumerable();

            Guid[] vectorOfRosterSizeQuestionsForEntityWithSubstitution = GetAllRosterSizeQuestionsAsVector(translatedEntity.Entity, questionnaire);

            IEnumerable<QuestionnaireVerificationMessage> entityErrors = substitutionReferences
                .Select(identifier => this.GetVerificationErrorsBySubstitutionReferenceOrNull(
                    translatedEntity, null, identifier, vectorOfRosterSizeQuestionsForEntityWithSubstitution, questionnaire))
                .Where(errorOrNull => errorOrNull != null);

            return entityErrors;
        }

        private IEnumerable<QuestionnaireVerificationMessage> GetErrorsBySubstitutionsInValidationCondition(MultiLanguageQuestionnaireDocument.TranslatedEntity<IComposite> translatedEntity, string validationCondition, int validationConditionIndex, MultiLanguageQuestionnaireDocument questionnaire)
        {
            string[] substitutionReferences = this.substitutionService.GetAllSubstitutionVariableNames(validationCondition);

            if (!substitutionReferences.Any())
                return Enumerable.Empty<QuestionnaireVerificationMessage>();

            Guid[] vectorOfRosterSizeQuestionsForEntityWithSubstitution = GetAllRosterSizeQuestionsAsVector(translatedEntity.Entity, questionnaire);

            IEnumerable<QuestionnaireVerificationMessage> entityErrors = substitutionReferences
                .Select(identifier => this.GetVerificationErrorsBySubstitutionReferenceOrNull(
                    translatedEntity, validationConditionIndex, identifier, vectorOfRosterSizeQuestionsForEntityWithSubstitution, questionnaire))
                .Where(errorOrNull => errorOrNull != null);

            return entityErrors;
        }

        private static bool SupportsSubstitutions(IComposite entity)
            => entity is IQuestion
            || entity is IStaticText
            || entity is IGroup;

        private static QuestionnaireVerificationReference CreateReference(IComposite entity)
        {
            if (entity is IVariable)
                return QuestionnaireVerificationReference.CreateForVariable(entity.PublicKey);

            if (entity is IGroup)
                return ((IGroup)entity).IsRoster
                    ? QuestionnaireVerificationReference.CreateForRoster(entity.PublicKey)
                    : QuestionnaireVerificationReference.CreateForGroup(entity.PublicKey);

            if (entity is IQuestion)
                return QuestionnaireVerificationReference.CreateForQuestion(entity.PublicKey);

            return QuestionnaireVerificationReference.CreateForStaticText(entity.PublicKey);
        }

        private static QuestionnaireVerificationReference CreateReference(IComposite entity, int? failedValidationIndex)
        {
            return new QuestionnaireVerificationReference(
                entity is IGroup
                    ? QuestionnaireVerificationReferenceType.Group
                    : (entity is IStaticText
                        ? QuestionnaireVerificationReferenceType.StaticText
                        : QuestionnaireVerificationReferenceType.Question),
                entity.PublicKey)
            {
                FailedValidationConditionIndex = failedValidationIndex
            };
        }


        private static bool CategoricalMultianswerQuestionIsFeatured(IMultyOptionsQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return IsPreFilledQuestion(question);
        }

        private static string GetCustomEnablementCondition(IComposite entity)
        {
            var entityAsIConditional = entity as IConditional;

            return entityAsIConditional?.ConditionExpression;
        }

        private static IEnumerable<ValidationCondition> GetValidationConditionsOrEmpty(IComposite entity)
        {
            var entityAsIConditional = entity as IValidatable;

            return entityAsIConditional != null ? entityAsIConditional.ValidationConditions : Enumerable.Empty<ValidationCondition>();
        }

        private static IComposite GetEntityByVariable(string identifier, MultiLanguageQuestionnaireDocument questionnaire)
            => questionnaire.FirstOrDefault<IQuestion>(q => q.StataExportCaption == identifier) as IComposite
            ?? questionnaire.FirstOrDefault<IVariable>(v => v.Name == identifier) as IComposite
            ?? questionnaire.FirstOrDefault<IGroup>(g => g.VariableName == identifier) as IComposite;

        private QuestionnaireVerificationMessage GetVerificationErrorsBySubstitutionReferenceOrNull(
            MultiLanguageQuestionnaireDocument.TranslatedEntity<IComposite> traslatedEntityWithSubstitution,
            int? validationConditionIndex,
            string substitutionReference,
            Guid[] vectorOfRosterQuestionsByEntityWithSubstitutions,
            MultiLanguageQuestionnaireDocument questionnaire)
        {
            bool isTitle = validationConditionIndex == null;
            var referenceToEntityWithSubstitution = CreateReference(traslatedEntityWithSubstitution.Entity, validationConditionIndex);

            if (traslatedEntityWithSubstitution.Entity is IQuestion && isTitle && substitutionReference == ((IQuestion)traslatedEntityWithSubstitution.Entity).StataExportCaption)
            {
                return QuestionnaireVerificationMessage.Error("WB0016",
                    VerificationMessages.WB0016_QuestionWithTitleSubstitutionCantReferenceSelf,
                    traslatedEntityWithSubstitution.TranslationName,
                    referenceToEntityWithSubstitution);
            }

            if (substitutionReference == this.substitutionService.RosterTitleSubstitutionReference)
            {
                if (vectorOfRosterQuestionsByEntityWithSubstitutions.Length == 0)
                {
                    return QuestionnaireVerificationMessage.Error("WB0059",
                        VerificationMessages.WB0059_EntityUsesRostertitleSubstitutionAndNeedsToBePlacedInsideRoster,
                        traslatedEntityWithSubstitution.TranslationName,
                        referenceToEntityWithSubstitution);
                }
                return null;
            }

            var entityToSubstitute = GetEntityByVariable(substitutionReference, questionnaire);
            if (entityToSubstitute == null)
            {
                return QuestionnaireVerificationMessage.Error("WB0017",
                    VerificationMessages.WB0017_SubstitutionReferencesNotExistingQuestionOrVariable,
                    traslatedEntityWithSubstitution.TranslationName,
                    referenceToEntityWithSubstitution);
            }

            var referenceToEntityBeingSubstituted = CreateReference(entityToSubstitute);

            var isVariable = entityToSubstitute is IVariable;
            var isQuestion = entityToSubstitute is IQuestion;
            var isNotVariableNorQuestions = !(isVariable || isQuestion);
            var isQuestionOfNotSupportedType = isQuestion && !QuestionTypesValidToBeSubstitutionReferences.Contains(((IQuestion)entityToSubstitute).QuestionType);
            if (isNotVariableNorQuestions || isQuestionOfNotSupportedType)
            {
                return QuestionnaireVerificationMessage.Error("WB0018",
                    VerificationMessages.WB0018_SubstitutionReferencesUnsupportedEntity,
                    traslatedEntityWithSubstitution.TranslationName,
                    referenceToEntityWithSubstitution,
                    referenceToEntityBeingSubstituted);
            }

            if (QuestionHasDeeperRosterLevelThenVectorOfRosterQuestions(entityToSubstitute,
                vectorOfRosterQuestionsByEntityWithSubstitutions, questionnaire))
            {
                return QuestionnaireVerificationMessage.Error("WB0019",
                    VerificationMessages.WB0019_SubstitutionCantReferenceItemWithDeeperRosterLevel,
                    traslatedEntityWithSubstitution.TranslationName,
                    referenceToEntityWithSubstitution,
                    referenceToEntityBeingSubstituted);
            }

            return null;
        }

        private static QuestionnaireVerificationMessage QuestionWithTitleSubstitutionCantBePrefilled(IQuestion questionsWithSubstitution)
            => QuestionnaireVerificationMessage.Error("WB0015",
                VerificationMessages.WB0015_QuestionWithTitleSubstitutionCantBePrefilled,
                CreateReference(questionsWithSubstitution));

        private static QuestionnaireVerificationMessage LinkedQuestionReferencesNotExistingQuestion(IQuestion linkedQuestion)
            => QuestionnaireVerificationMessage.Error("WB0011",
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

        private static QuestionnaireVerificationMessage CreateExpressionSyntaxError(ExpressionLocation expressionLocation, IEnumerable<string> compilationErrorMessages)
        {
            QuestionnaireVerificationReference reference;

            switch (expressionLocation.ItemType)
            {
                case ExpressionLocationItemType.Group:
                    reference = QuestionnaireVerificationReference.CreateForGroup(expressionLocation.Id);
                    break;
                case ExpressionLocationItemType.Roster:
                    reference = QuestionnaireVerificationReference.CreateForRoster(expressionLocation.Id);
                    break;
                case ExpressionLocationItemType.Question:
                    reference = QuestionnaireVerificationReference.CreateForQuestion(expressionLocation.Id);
                    break;
                case ExpressionLocationItemType.StaticText:
                    reference = QuestionnaireVerificationReference.CreateForStaticText(expressionLocation.Id);
                    break;
                case ExpressionLocationItemType.LookupTable:
                    reference = QuestionnaireVerificationReference.CreateForLookupTable(expressionLocation.Id);
                    break;
                case ExpressionLocationItemType.Variable:
                    reference = QuestionnaireVerificationReference.CreateForVariable(expressionLocation.Id);
                    break;
                case ExpressionLocationItemType.Questionnaire:
                    return QuestionnaireVerificationMessage.Error("WB0096", VerificationMessages.WB0096_GeneralCompilationError);
                default:
                    throw new ArgumentException("expressionLocation");
            }

            switch (expressionLocation.ExpressionType)
            {
                case ExpressionLocationType.Validation:
                    reference.FailedValidationConditionIndex = expressionLocation.ExpressionPosition;
                    return QuestionnaireVerificationMessage.Error("WB0002",
                        VerificationMessages.WB0004_VariableHasEmptyExpression, compilationErrorMessages, reference);
                case ExpressionLocationType.Condition:
                    return QuestionnaireVerificationMessage.Error("WB0003",
                        VerificationMessages.WB0003_CustomEnablementConditionHasIncorrectSyntax, compilationErrorMessages, reference);
                case ExpressionLocationType.Filter:
                    return QuestionnaireVerificationMessage.Error("WB0110",
                        VerificationMessages.WB0110_LinkedQuestionFilterExpresssionHasIncorrectSyntax, compilationErrorMessages, reference);
                case ExpressionLocationType.Expression:
                    return QuestionnaireVerificationMessage.Error("WB0027",
                        VerificationMessages.WB0027_VariableExpresssionHasIncorrectSyntax, compilationErrorMessages, reference);
                case ExpressionLocationType.CategoricalFilter:
                    return QuestionnaireVerificationMessage.Error("WB0062",
                        VerificationMessages.WB0062_OptionFilterExpresssionHasIncorrectSyntax, compilationErrorMessages, reference);
            }

            return QuestionnaireVerificationMessage.Error("WB0096", VerificationMessages.WB0096_GeneralCompilationError);
        }
        
        private static bool NoQuestionsExist(MultiLanguageQuestionnaireDocument questionnaire)
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

        private static bool IsRosterSizeQuestion(IQuestion question, MultiLanguageQuestionnaireDocument questionnaire)
        {
            var rosterSizeQuestionIds =
                questionnaire.Find<IGroup>(IsRosterByQuestion).Select(group => group.RosterSizeQuestionId);
            return rosterSizeQuestionIds.Contains(question.PublicKey);
        }

        private static IQuestion GetRosterSizeQuestionByRosterGroup(IGroup group, MultiLanguageQuestionnaireDocument questionnaire)
        {
            return
                questionnaire.FirstOrDefault<IQuestion>(
                    question => group.RosterSizeQuestionId.HasValue && question.PublicKey == group.RosterSizeQuestionId.Value);
        }

        private static IEnumerable<IGroup> GetAllParentGroupsForQuestion(IQuestion question, MultiLanguageQuestionnaireDocument document)
        {
            return GetSpecifiedGroupAndAllItsParentGroupsStartingFromBottom((IGroup)question.GetParent(), document);
        }

        private static Guid[] GetAllRosterSizeQuestionsAsVector(IComposite item, MultiLanguageQuestionnaireDocument questionnaire)
        {
            IGroup nearestGroup = item as IGroup ?? (IGroup)item.GetParent();

            Guid[] rosterSizeQuestions =
                GetSpecifiedGroupAndAllItsParentGroupsStartingFromBottom(nearestGroup, questionnaire)
                    .Where(IsRosterGroup)
                    .Select(g => g.RosterSizeQuestionId ?? g.PublicKey)
                    .ToArray();

            return rosterSizeQuestions;
        }

        private static IEnumerable<IGroup> GetSpecifiedGroupAndAllItsParentGroupsStartingFromBottom(IGroup group, MultiLanguageQuestionnaireDocument document)
        {
            var parentGroups = new List<IGroup>();

            while (group.PublicKey != document.PublicKey)
            {
                parentGroups.Add(group);
                group = (IGroup)group.GetParent();
            }

            return parentGroups;
        }

        private static bool QuestionHasDeeperRosterLevelThenVectorOfRosterQuestions(
            IComposite entity,
            Guid[] vectorOfRosterSizeQuestions,
            MultiLanguageQuestionnaireDocument questionnaire)
        {
            Guid[] rosterQuestionsAsVectorForQuestionSourceOfSubstitution =
                GetAllRosterSizeQuestionsAsVector(entity, questionnaire);

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
            return true;
        }

        private static bool IsSupervisorQuestion(IQuestion question)
        {
            return question.QuestionScope == QuestionScope.Supervisor;
        }

        private static bool IsPreFilledQuestion(IQuestion question)
        {
            return question.Featured;
        }

        private static bool IsFilteredComboboxQuestion(IQuestion question)
        {
            return IsCategoricalSingleAnswerQuestion(question) && question.IsFilteredCombobox.HasValue &&
                   question.IsFilteredCombobox.Value;
        }

        private bool HasQuestionnaireExpressionsWithExceedLength(MultiLanguageQuestionnaireDocument questionnaire)
        {
            Func<IComposite, bool> isValidationExpressionLengthExeeded =
                entity => GetValidationConditionsOrEmpty(entity).Any(ValidationConditionIsTooLong);

            Func<IComposite, bool> isConditionExpressionLengthExeeded =
                entity => this.DoesExpressionExceed1000CharsLimit(questionnaire, GetCustomEnablementCondition(entity));

            Func<IQuestion, bool> isLinkedFilterExpressionLengthExeeded =
                question => this.DoesExpressionExceed1000CharsLimit(questionnaire, question.LinkedFilterExpression);

            Func<IQuestion, bool> isOptionsFilterExpressionLengthExeeded =
                question => this.DoesExpressionExceed1000CharsLimit(questionnaire, question.Properties.OptionsFilterExpression);

            Func<IVariable, bool> isVariableExpressionLengthExeeded =
                variable => this.DoesExpressionExceed1000CharsLimit(questionnaire, variable.Expression);

            return questionnaire.Find<IComposite>(entity =>
                isValidationExpressionLengthExeeded(entity) ||
                isConditionExpressionLengthExeeded(entity) ||
                ((entity is IQuestion) && isLinkedFilterExpressionLengthExeeded((IQuestion)entity)) ||
                ((entity is IQuestion) && isOptionsFilterExpressionLengthExeeded((IQuestion)entity)) ||
                ((entity is IVariable) && isVariableExpressionLengthExeeded((IVariable)entity))).Any();
        }
    }
}