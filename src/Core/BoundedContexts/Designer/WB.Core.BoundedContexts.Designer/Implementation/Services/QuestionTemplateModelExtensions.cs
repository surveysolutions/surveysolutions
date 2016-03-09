using System.Linq;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    public static class QuestionTemplateModelExtensions
    {
        public static bool HasAnyValidation(this QuestionTemplateModel question)
        {
            return question.ValidationExpressions.Any(x => !string.IsNullOrWhiteSpace(x.ValidationExpression));
        }
    }
}
