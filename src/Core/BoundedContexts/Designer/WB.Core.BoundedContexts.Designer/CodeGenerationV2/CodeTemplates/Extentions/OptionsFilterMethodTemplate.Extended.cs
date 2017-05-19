using WB.Core.BoundedContexts.Designer.CodeGenerationV2.Models;

namespace WB.Core.BoundedContexts.Designer.CodeGenerationV2.CodeTemplates
{
    public partial class OptionsFilterMethodTemplate
    {
        public OptionsFilterMethodTemplate(OptionsFilterMethodModel model)
        {
            this.Model = model;
        }

        protected OptionsFilterMethodModel Model { get; set; }
    }
}