using WB.Core.BoundedContexts.Designer.CodeGenerationV2.Models;

namespace WB.Core.BoundedContexts.Designer.CodeGenerationV2.CodeTemplates
{
    public partial class LinkedFilterMethodTemplate
    {
        public LinkedFilterMethodTemplate(LinkedFilterMethodModel model)
        {
            this.Model = model;
        }

        protected LinkedFilterMethodModel Model { get; set; }
    }
}