using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.V10.Templates
{
    public partial class LinkedFilterMethodTemplateV10
    {
        public LinkedFilterMethodTemplateV10(LinkedFilterConditionDescriptionModel model)
        {
            this.Model = model;
        }

        protected LinkedFilterConditionDescriptionModel Model { get; set; }
    }
}