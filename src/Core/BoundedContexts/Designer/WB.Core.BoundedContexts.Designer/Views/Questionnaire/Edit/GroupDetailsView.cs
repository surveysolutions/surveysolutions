using System;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit
{
    public class GroupDetailsView
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string EnablementCondition { get; set; }
        public string VariableName { get; set; }
    }
}