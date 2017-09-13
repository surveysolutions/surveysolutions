using WB.Core.BoundedContexts.Designer.CodeGenerationV2.Models;

namespace WB.Core.BoundedContexts.Designer.CodeGenerationV2.CodeTemplates
{
    public partial class OptionsFilterMethodTemplate
    {
        public OptionsFilterMethodTemplate(GroupedModel<OptionsFilterMethodModel> model)
        {
            this.Model = model;
        }

        protected GroupedModel<OptionsFilterMethodModel> Model { get; set; }
    }
}