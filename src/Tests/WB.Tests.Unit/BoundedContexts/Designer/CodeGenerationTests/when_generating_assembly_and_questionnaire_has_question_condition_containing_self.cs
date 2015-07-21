using System;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Services;

namespace WB.Tests.Unit.BoundedContexts.Designer.CodeGenerationTests
{
    internal class when_generating_assembly_and_questionnaire_has_question_condition_containing_self : CodeGenerationTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                string resultAssembly;

                AssemblyContext.SetupServiceLocator();

                IExpressionProcessorGenerator expressionProcessorGenerator = CreateExpressionProcessorGenerator();

                QuestionnaireDocument questionnaireDocument = CreateQuestionnaireWithQuestionAndConditionContainingUsageOfSelf(questionId);

                GenerationResult emitResult = expressionProcessorGenerator.GenerateProcessorStateAssembly(questionnaireDocument, CreateQuestionnaireVersion(), out resultAssembly);

                return new InvokeResults
                {
                    Success = emitResult.Success, 
                    ErrorLocations =  emitResult.Diagnostics.Where(l =>l.Severity == GenerationDiagnosticSeverity.Error).
                        Select(l => l.Location).Distinct().Select(s => new ExpressionLocation(s)).ToArray()
                };
            });

        It should_result_succeded = () =>
            results.Success.ShouldEqual(false);

        It should_errors_locations_count_equals_1 = () =>
            results.ErrorLocations.Count().ShouldEqual(1);

        It should_errors_locations_contains_question1 = () =>
            results.ErrorLocations.SingleOrDefault(x => x.Id == questionId 
                && x.ItemType == ExpressionLocationItemType.Question 
                && x.ExpressionType == ExpressionLocationType.Condition).ShouldNotBeNull();
    
        
        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        private static AppDomainContext appDomainContext;
        private static InvokeResults results;

        private static string questionStringId = "11111111111111111111111111111113";
        private static Guid questionId = Guid.Parse(questionStringId);

        [Serializable]
        internal class InvokeResults
        {
            public bool Success { get; set; }
            public ExpressionLocation[] ErrorLocations { get; set; }
        }
    }
}
