using WB.Core.BoundedContexts.Designer.CodeGenerationV2.Models;

namespace WB.Core.BoundedContexts.Designer.CodeGenerationV2.CodeTemplates
{
    public partial class ConditionMethodTemplate
    {
        public ConditionMethodTemplate(GroupedModel<ConditionMethodModel> model)
        {
            this.Model = model;
        }

        protected GroupedModel<ConditionMethodModel> Model { get; set; }
    }
}
