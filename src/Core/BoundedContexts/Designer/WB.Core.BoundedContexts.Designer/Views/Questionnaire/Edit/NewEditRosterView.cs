using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit
{
    public class NewEditRosterView
    {
        public string ItemId { get; set; }
        public string Title { get; set; }
        public string EnablementCondition { get; set; }
        public string VariableName { get; set; }

        public RosterType Type { get; set; }

        public string RosterSizeListQuestionId { get; set; }
        public string RosterSizeNumericQuestionId { get; set; }
        public string RosterSizeMultiQuestionId { get; set; }
        public string RosterTitleQuestionId { get; set; }
        public Tuple<decimal,string>[] FixedRosterTitles { get; set; }

        public QuestionnaireInfoFactory.SelectOption[] RosterTypeOptions { get; set; }

        public List<DropdownQuestionView> NumericIntegerQuestions { get; set; }
        public List<DropdownQuestionView> NumericIntegerTitles { get; set; }
        public List<DropdownQuestionView> NotLinkedMultiOptionQuestions { get; set; }
        public List<DropdownQuestionView> TextListsQuestions { get; set; }
        public Breadcrumb[] Breadcrumbs { get; set; }
    }
}