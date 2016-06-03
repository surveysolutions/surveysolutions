using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.V10.Templates
{
    public partial class OptionsFilterMethodTemplate
    {
        public OptionsFilterMethodTemplate(ConditionDescriptionModel model)
        {
            this.Model = model;
        }

        protected ConditionDescriptionModel Model { get; set; }
    }
}