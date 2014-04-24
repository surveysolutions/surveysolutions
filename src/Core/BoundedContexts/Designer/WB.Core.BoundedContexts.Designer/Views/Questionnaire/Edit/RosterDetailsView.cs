using System;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit
{
    public class RosterDetailsView : GroupDetailsView
    {
        public Guid? RosterSizeQuestionId { get; set; }
        public RosterSizeSourceType RosterSizeSourceType { get; set; }
        public string[] RosterFixedTitles { get; set; }
        public Guid? RosterTitleQuestionId { get; set; }
    }
}