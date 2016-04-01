using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.CodeGeneratorTests
{
    internal class CodeGeneratorTestsContext
    {
        protected static QuestionnaireDocument CreateQuestionnaireDocument(params IComposite[] questionnaireChildren)
        {
            return new QuestionnaireDocument
            {
                Children = questionnaireChildren.ToList(),
            };
        }
    }
}