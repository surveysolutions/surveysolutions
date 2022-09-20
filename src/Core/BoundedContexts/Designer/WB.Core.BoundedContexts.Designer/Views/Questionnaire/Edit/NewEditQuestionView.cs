using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit
{
    public class NewEditQuestionView
    {
        public NewEditQuestionView(Guid id, 
            Guid parentGroupId, 
            bool isPreFilled,
            QuestionnaireInfoFactory.SelectOption[] questionTypeOptions,
            QuestionScope questionScope,
            QuestionType type,
            bool useFormatting = false,
            bool hideInstructions = false,
            GeometryType geometryType = 0,
            GeometryInputMode geometryInputMode = 0,
            bool isTimestamp = false,
            string? optionsFilterExpression = null,  string? variableName = null, 
            string? variableLabel = null, string? title = null,  
            string? linkedToEntityId = null, string? linkedFilterExpression = null,  
            bool? isFilteredCombobox = null,  DateTime? defaultDate = null, 
            string? cascadeFromQuestionId = null,
            bool hideIfDisabled = false,
            string? enablementCondition = null,
            bool? showAsList = null,
            List<DropdownEntityView>? sourceOfLinkedEntities = null,
            List<DropdownEntityView>? sourceOfSingleQuestions = null,
            QuestionnaireInfoFactory.SelectOption[]? allQuestionScopeOptions = null,
            QuestionnaireInfoFactory.SelectOption[]? geometryTypeOptions = null,
            List<QualityOption>? qualityOptions = null,
            Breadcrumb[]? breadcrumbs = null,
            int? quality = null,
            int? optionsCount = null,
            bool? wereOptionsTruncated = null,
            bool? isInteger = null,
            bool? yesNoView = null,
            bool? isSignature = null,
            string? instructions = null,
            int? maxAllowedAnswers = null,
            int? showAsListThreshold = null, Guid[]? parentGroupsIds = null, Guid[]? rosterScopeIds = null,
            string? categoriesId = null, string? mask = null, int? countOfDecimalPlaces = null, int? maxAnswerCount = null, 
            List<ValidationCondition>? validationConditions = null, CategoricalOption[]? options = null, bool? areAnswersOrdered = null,
            QuestionnaireInfoFactory.SelectOption[]? geometryInputModeOptions = null,
            bool? geometryShowNeighbours = null)
        {

            this.SourceOfLinkedEntities = sourceOfLinkedEntities ?? new List<DropdownEntityView>();
            this.SourceOfSingleQuestions = sourceOfSingleQuestions ?? new List<DropdownEntityView>();
            this.ValidationConditions = validationConditions ?? new List<ValidationCondition>();
            this.Options = options ?? new CategoricalOption[0];
            this.AllQuestionScopeOptions = allQuestionScopeOptions ?? new QuestionnaireInfoFactory.SelectOption[0];
            this.GeometryTypeOptions = geometryTypeOptions ?? new QuestionnaireInfoFactory.SelectOption[0];
            this.GeometryInputModeOptions = geometryInputModeOptions ?? new QuestionnaireInfoFactory.SelectOption[0];
            this.Breadcrumbs = breadcrumbs ?? new Breadcrumb[0];
            QualityOptions = qualityOptions?? new List<QualityOption>();
            Id = id;
            ParentGroupsIds = parentGroupsIds;
            RosterScopeIds = rosterScopeIds;
            ParentGroupId = parentGroupId;
            EnablementCondition = enablementCondition;
            IsPreFilled = isPreFilled;
            Instructions = instructions;
            HideInstructions = hideInstructions;
            UseFormatting = useFormatting;
            OptionsFilterExpression = optionsFilterExpression;
            QuestionScope = questionScope;
            VariableName = variableName;
            VariableLabel = variableLabel;
            Title = title;
            Type = type;
            LinkedToEntityId = linkedToEntityId;
            LinkedFilterExpression = linkedFilterExpression;
            
            AreAnswersOrdered = areAnswersOrdered;
            MaxAllowedAnswers = maxAllowedAnswers;
            IsInteger = isInteger;
            Mask = mask;
            CountOfDecimalPlaces = countOfDecimalPlaces;
            MaxAnswerCount = maxAnswerCount;
            YesNoView = yesNoView;
            IsFilteredCombobox = isFilteredCombobox;
            IsSignature = isSignature;
            IsTimestamp = isTimestamp;
            DefaultDate = defaultDate;
            CascadeFromQuestionId = cascadeFromQuestionId;
            QuestionTypeOptions = questionTypeOptions;
            GeometryType = geometryType;
            WereOptionsTruncated = wereOptionsTruncated;
            OptionsCount = optionsCount;
            HideIfDisabled = hideIfDisabled;
            Quality = quality;
            
            ShowAsList = showAsList;
            ShowAsListThreshold = showAsListThreshold;
            CategoriesId = categoriesId;
            
            GeometryInputMode = geometryInputMode;
            GeometryShowNeighbours = geometryShowNeighbours;
        }

        public bool? GeometryShowNeighbours { get; set; }

        public Guid Id { get; set; }
        public Guid[]? ParentGroupsIds { get; set; }
        public Guid[]? RosterScopeIds { get; set; }
        public Guid ParentGroupId { get; set; }
        public Guid ChapterId { get; set; }
        public string? EnablementCondition { get; set; }
        public bool IsPreFilled { get; set; }
        public string? Instructions { get; set; }
        public bool HideInstructions { get; set; }
        public bool UseFormatting { get; set; }
        public string? OptionsFilterExpression { get; set; }
        public QuestionScope QuestionScope { get; set; }
        public string? VariableName { get; set; }
        public string? VariableLabel { get; set; }
        public string? Title { get; set; }

        public List<ValidationCondition> ValidationConditions { get; private set; }

        public QuestionType Type { get; set; }
        public string? LinkedToEntityId { get; set; }
        public string? LinkedFilterExpression { get; set; }
        public CategoricalOption[] Options { get; set; }
        public bool? AreAnswersOrdered { get; set; }
        public int? MaxAllowedAnswers { get; set; }
        public bool? IsInteger { get; set; }
        public string? Mask { get; set; }
        public int? CountOfDecimalPlaces { get; set; }
        public int? MaxAnswerCount { get; set; }
        public bool? YesNoView { get; set; }
        public bool? IsFilteredCombobox { get; set; }
        public bool? IsSignature { get; set; }
        public bool IsTimestamp { get; set; }
        public DateTime? DefaultDate { get; set; }

        public string? CascadeFromQuestionId { get; set; }

        public List<DropdownEntityView> SourceOfLinkedEntities { get; set; }
        public List<DropdownEntityView> SourceOfSingleQuestions { get; set; }

        public QuestionnaireInfoFactory.SelectOption[] QuestionTypeOptions { get; set; }
        public QuestionnaireInfoFactory.SelectOption[] AllQuestionScopeOptions { get; set; }
        public QuestionnaireInfoFactory.SelectOption[] GeometryTypeOptions { get; set; }

        public GeometryType GeometryType { get; set; }

        public Breadcrumb[] Breadcrumbs { get; set; }
        public bool? WereOptionsTruncated { get; set; }
        public int? OptionsCount { get; set; }
        public bool HideIfDisabled { get; set; }
        public int? Quality { get; set; }
        public List<QualityOption> QualityOptions { get; set; }

        public bool? ShowAsList { get; set; }
        public int? ShowAsListThreshold { get; set; }
        public string? CategoriesId { get; set; }
        public QuestionnaireInfoFactory.SelectOption[] GeometryInputModeOptions { get; set; }

        public GeometryInputMode GeometryInputMode { get; set; }
    }
}
