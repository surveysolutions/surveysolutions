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
        public NewEditQuestionView()
        {
            this.SourceOfLinkedEntities = new List<DropdownEntityView>();
            this.SourceOfSingleQuestions = new List<DropdownEntityView>();
            this.ValidationConditions = new List<ValidationCondition>();
        }

        public Guid Id { get; set; }
        public Guid[] ParentGroupsIds { get; set; }
        public Guid[] RosterScopeIds { get; set; }
        public Guid ParentGroupId { get; set; }
        public string EnablementCondition { get; set; }
        public bool IsPreFilled { get; set; }
        public string Instructions { get; set; }
        public bool HideInstructions { get; set; }
        public bool UseFormatting { get; set; }
        public string OptionsFilterExpression { get; set; }
        public QuestionScope QuestionScope { get; set; }
        public string VariableName { get; set; }
        public string VariableLabel { get; set; }
        public string Title { get; set; }

        public List<ValidationCondition> ValidationConditions { get; private set; }

        public QuestionType Type { get; set; }
        public string LinkedToEntityId { get; set; }
        public string LinkedFilterExpression { get; set; }
        public CategoricalOption[] Options { get; set; }
        public bool AreAnswersOrdered { get; set; }
        public int? MaxAllowedAnswers { get; set; }
        public bool IsInteger { get; set; }
        public string Mask { get; set; }
        public int? CountOfDecimalPlaces { get; set; }
        public int? MaxAnswerCount { get; set; }
        public bool YesNoView { get; set; }
        public bool? IsFilteredCombobox { get; set; }
        public bool? IsSignature { get; set; }
        public bool IsTimestamp { get; set; }

        public string CascadeFromQuestionId { get; set; }

        public List<DropdownEntityView> SourceOfLinkedEntities { get; set; }
        public List<DropdownEntityView> SourceOfSingleQuestions { get; set; }

        public QuestionnaireInfoFactory.SelectOption[] QuestionTypeOptions { get; set; }
        public QuestionnaireInfoFactory.SelectOption[] AllQuestionScopeOptions { get; set; }
        public QuestionnaireInfoFactory.SelectOption[] GeometryTypeOptions { get; set; }

        public GeometryType GeometryType { get; set; }

        public Breadcrumb[] Breadcrumbs { get; set; }
        public bool WereOptionsTruncated { get; set; }
        public int OptionsCount { get; set; }
        public bool HideIfDisabled { get; set; }
        public int Quality { get; set; }
        public List<QualityOption> QualityOptions { get; set; }
    }
}
