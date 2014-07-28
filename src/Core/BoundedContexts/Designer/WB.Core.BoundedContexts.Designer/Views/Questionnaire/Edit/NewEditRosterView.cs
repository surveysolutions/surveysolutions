using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit
{
    public class NewEditRosterView
    {
        public string ItemId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string EnablementCondition { get; set; }
        public string VariableName { get; set; }

        public RosterType Type { get; set; }

        public Guid? RosterSizeListQuestionId { get; set; }
        public Guid? RosterSizeNumericQuestionId { get; set; }
        public Guid? RosterSizeMultiQuestionId { get; set; }
        public Guid? RosterTitleQuestionId { get; set; }
        public string[] RosterFixedTitles { get; set; }

        public QuestionnaireInfoFactory.SelectOption[] RosterTypeOptions { get; set; }

        public List<DropdownQuestionView> NumericIntegerQuestions { get; set; }
        public List<DropdownQuestionView> NumericIntegerTitles { get; set; }
        public List<DropdownQuestionView> NotLinkedMultiOptionQuestions { get; set; }
        public List<DropdownQuestionView> TextListsQuestions { get; set; }
        public Breadcrumb[] Breadcrumbs { get; set; }
    }
}