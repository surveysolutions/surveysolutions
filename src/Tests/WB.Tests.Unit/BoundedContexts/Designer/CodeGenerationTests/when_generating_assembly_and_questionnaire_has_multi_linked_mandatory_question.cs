using System;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Documents;
using Microsoft.Practices.ServiceLocation;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Services;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.CodeGenerationTests
{
    internal class when_generating_assembly_and_questionnaire_has_multi_linked_mandatory_question : CodeGenerationTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Guid id = Guid.Parse("11111111111111111111111111111111");
                string resultAssembly;

                AssemblyContext.SetupServiceLocator();

                var expressionProcessorGenerator = CreateExpressionProcessorGenerator();

                QuestionnaireDocument questionnaireDocument = CreateQuestionnaireWithCategoricalMultiLinkedMandatoryQuestion();

                GenerationResult emitResult = expressionProcessorGenerator.GenerateProcessorStateAssembly(questionnaireDocument, CreateQuestionnaireVersion(),out resultAssembly);

                return new InvokeResults
                {
                    Success = emitResult.Success
                };
            });

        It should_result_succeded = () =>
            results.Success.ShouldEqual(true);
        
        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        private static AppDomainContext appDomainContext;
        private static InvokeResults results;

        [Serializable]
        internal class InvokeResults
        {
            public bool Success { get; set; }
        }
    }
}
