using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.SurveySolutions;
using WB.Core.SharedKernels.SurveySolutions.Implementation.Services;

namespace WB.Tests.Unit.BoundedContexts.Designer.CodeGeneratorTests
{
    internal class CodeGeneratorTestsContext
    {
        protected static CodeGenerator CreateCodeGenerator()
        {
            return new CodeGenerator();
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocument(params IComposite[] questionnaireChildren)
        {
            return new QuestionnaireDocument
            {
                Children = questionnaireChildren.ToList(),
            };
        }
    }
}
