using System;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit
{
    public class GroupDetailsView
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? EnablementCondition { get; set; }
        public bool HideIfDisabled { get; set; }
        public string? VariableName { get; set; }
        public RosterDisplayMode DisplayMode { get; set; }
    }
}
