using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit
{
    public class NewEditRosterView
    {
        public NewEditRosterView(RosterDisplayMode displayMode, RosterDisplayMode[] displayModes)
        {
            DisplayMode = displayMode;
            DisplayModes = displayModes;

            FixedRosterTitles = new FixedRosterTitle[0];
            RosterTypeOptions = new QuestionnaireInfoFactory.SelectOption[0];
            NumericIntegerQuestions = new List<DropdownEntityView>();
            NumericIntegerTitles = new List<DropdownEntityView>();
            NotLinkedMultiOptionQuestions = new List<DropdownEntityView>();
            TextListsQuestions = new List<DropdownEntityView>();
            Breadcrumbs = new Breadcrumb[0];
        }

        public string? ItemId { get; set; }
        public string? Title { get; set; }
        public string? EnablementCondition { get; set; }
        public bool HideIfDisabled { get; set; }
        public RosterDisplayMode DisplayMode { get; set; }
        public RosterDisplayMode[] DisplayModes { get; set; }
        public string? VariableName { get; set; }

        public RosterType Type { get; set; }

        public string? RosterSizeListQuestionId { get; set; }
        public string? RosterSizeNumericQuestionId { get; set; }
        public string? RosterSizeMultiQuestionId { get; set; }
        public string? RosterTitleQuestionId { get; set; }
        public FixedRosterTitle[] FixedRosterTitles { get; set; }

        public QuestionnaireInfoFactory.SelectOption[] RosterTypeOptions { get; set; }

        public List<DropdownEntityView> NumericIntegerQuestions { get; set; }
        public List<DropdownEntityView> NumericIntegerTitles { get; set; }
        public List<DropdownEntityView> NotLinkedMultiOptionQuestions { get; set; }
        public List<DropdownEntityView> TextListsQuestions { get; set; }
        public Breadcrumb[] Breadcrumbs { get; set; }
    }
}
