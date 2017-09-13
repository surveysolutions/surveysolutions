using WB.Core.BoundedContexts.Designer.CodeGenerationV2.Models;

namespace WB.Core.BoundedContexts.Designer.CodeGenerationV2.CodeTemplates
{
    public partial class LinkedFilterMethodTemplate
    {
        public LinkedFilterMethodTemplate(GroupedModel<LinkedFilterMethodModel> model)
        {
            this.Model = model;
        }

        protected GroupedModel<LinkedFilterMethodModel> Model { get; set; }
    }
}