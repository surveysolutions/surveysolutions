using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.V10.Templates
{
    public partial class OptionsFilterMethodTemplateV10
    {
        public OptionsFilterMethodTemplateV10(OptionsFilterConditionDescriptionModel model)
        {
            this.Model = model;
        }

        protected OptionsFilterConditionDescriptionModel Model { get; set; }
    }
}