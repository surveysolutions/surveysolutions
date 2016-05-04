using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Templates
{
    public partial class ExpressionMethodTemplate
    {
        public ExpressionMethodTemplate(ConditionDescriptionModel model)
        {
            Model = model;
        }

        protected ConditionDescriptionModel Model { get; set; }
    }
}
