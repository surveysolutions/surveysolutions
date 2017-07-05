using WB.Core.BoundedContexts.Designer.CodeGenerationV2.Models;

namespace WB.Core.BoundedContexts.Designer.CodeGenerationV2.CodeTemplates
{
    public partial class ConditionMethodTemplate
    {
        public ConditionMethodTemplate(ConditionMethodModel model)
        {
            this.Model = model;
        }

        protected ConditionMethodModel Model { get; set; }
    }
}
