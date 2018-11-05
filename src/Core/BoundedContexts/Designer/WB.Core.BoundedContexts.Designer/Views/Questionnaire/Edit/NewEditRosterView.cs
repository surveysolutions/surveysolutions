using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit
{
    public class NewEditRosterView
    {
        public string ItemId { get; set; }
        public string Title { get; set; }
        public string EnablementCondition { get; set; }
        public bool HideIfDisabled { get; set; }
        public string VariableName { get; set; }

        public RosterType Type { get; set; }

        public string RosterSizeListQuestionId { get; set; }
        public string RosterSizeNumericQuestionId { get; set; }
        public string RosterSizeMultiQuestionId { get; set; }
        public string RosterTitleQuestionId { get; set; }
        public FixedRosterTitle[] FixedRosterTitles { get; set; }

        public QuestionnaireInfoFactory.SelectOption[] RosterTypeOptions { get; set; }

        public List<DropdownEntityView> NumericIntegerQuestions { get; set; }
        public List<DropdownEntityView> NumericIntegerTitles { get; set; }
        public List<DropdownEntityView> NotLinkedMultiOptionQuestions { get; set; }
        public List<DropdownEntityView> TextListsQuestions { get; set; }
        public Breadcrumb[] Breadcrumbs { get; set; }
    }
}
