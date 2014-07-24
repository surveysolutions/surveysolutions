using System;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    public class GroupTemplateModel
    {
        public Guid Id { set; get; }

        public string VariableName { set; get; }
        
        public string Conditions { set; get; }

        public string GeneratedGroupStateName { set; get; }
    }
}