using System;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit
{
    public class VariableName
    {
        public VariableName(string? id, string name, string? type)
        {
            Id = id;
            Name = name;
            Type = type;
        }

        public string? Id { set; get; }
        public string Name { set; get; }
        public string? Type { get; set; }
    }
}
