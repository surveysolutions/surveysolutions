using System.Linq;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    public static class TemplateModelExtensions
    {
        public static bool HasAnyValidation(this QuestionTemplateModel question)
            => question.ValidationExpressions.Any(x => !string.IsNullOrWhiteSpace(x.ValidationExpression));
    }
}