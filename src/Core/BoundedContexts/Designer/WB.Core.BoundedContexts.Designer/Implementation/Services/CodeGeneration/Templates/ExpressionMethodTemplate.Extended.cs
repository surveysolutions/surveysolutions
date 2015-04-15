using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Templates
{
    public partial class ExpressionMethodTemplate
    {
        public ExpressionMethodTemplate(ExpressionMethodModel model)
        {
            Model = model;
        }

        protected ExpressionMethodModel Model { get; set; }
    }
}
