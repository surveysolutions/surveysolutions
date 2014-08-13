using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit
{
    public class NewEditQuestionView
    {
        //public QuestionDetailsView Question { get; set; }
        public NewEditQuestionView()
        {
            this.SourceOfLinkedQuestions = new List<DropdownQuestionView>();
        }

        public Guid Id { get; set; }
        public Guid[] ParentGroupsIds { get; set; }
        public Guid[] RosterScopeIds { get; set; }
        public Guid ParentGroupId { get; set; }
        public string EnablementCondition { get; set; }
        public bool IsPreFilled { get; set; }
        public string Instructions { get; set; }
        public bool IsMandatory { get; set; }
        public QuestionScope QuestionScope { get; set; }
        public string VariableName { get; set; }
        public string VariableLabel { get; set; }
        public string Title { get; set; }
        public string ValidationExpression { get; set; }
        public string ValidationMessage { get; set; }
        public QuestionType Type { get; set; }
        public string LinkedToQuestionId { get; set; }
        public CategoricalOption[] Options { get; set; }
        public bool AreAnswersOrdered { get; set; }
        public int? MaxAllowedAnswers { get; set; }
        public bool IsInteger { get; set; }
        public string Mask { get; set; }
        public int? MaxValue { get; set; }
        public int? CountOfDecimalPlaces { get; set; }
        public int? MaxAnswerCount { get; set; }
        public bool? IsFilteredCombobox { get; set; }

        public List<DropdownQuestionView> SourceOfLinkedQuestions { get; set; }
        public QuestionnaireInfoFactory.SelectOption[] QuestionTypeOptions { get; set; }
        public QuestionnaireInfoFactory.SelectOption[] AllQuestionScopeOptions { get; set; }
        public QuestionnaireInfoFactory.SelectOption[] NotPrefilledQuestionScopeOptions { get; set; }
        public Breadcrumb[] Breadcrumbs { get; set; }
    }
}