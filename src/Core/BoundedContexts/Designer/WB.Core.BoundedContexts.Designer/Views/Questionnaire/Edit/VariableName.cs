using System;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit
{
    public class VariableName
    {
        public VariableName(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public string Id { set; get; }
        public string Name { set; get; }
    }
}
