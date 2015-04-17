using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo
{
    public class GroupAndRosterDetailsView : DescendantItemView
    {
        public string Title { get; set; }

        public string EnablementCondition { get; set; }
        public Guid? RosterSizeQuestionId { get; set; }
        public bool IsRoster { get; set; }
        public RosterSizeSourceType RosterSizeSourceType { get; set; }
        public FixedRosterTitle[] FixedRosterTitles { get; set; }
        public Guid? RosterTitleQuestionId { get; set; }
        public string VariableName { get; set; }

        public GroupAndRosterDetailsView()
        {
            FixedRosterTitles = new FixedRosterTitle[0];
        }
    }
}