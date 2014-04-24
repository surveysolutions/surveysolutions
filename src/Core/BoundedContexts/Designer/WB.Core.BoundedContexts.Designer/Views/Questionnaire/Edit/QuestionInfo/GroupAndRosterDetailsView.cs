using System;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo
{
    public class GroupAndRosterDetailsView : DescendantItemView
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string EnablementCondition { get; set; }
        public Guid? RosterSizeQuestionId { get; set; }
        public bool IsRoster { get; set; }
        public RosterSizeSourceType RosterSizeSourceType { get; set; }
        public string[] RosterFixedTitles { get; set; }
        public Guid? RosterTitleQuestionId { get; set; }
    }
}