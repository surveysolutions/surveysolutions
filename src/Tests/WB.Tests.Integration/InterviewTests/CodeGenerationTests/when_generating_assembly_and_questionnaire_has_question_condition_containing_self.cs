using System;
using System.Linq;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Services;

namespace WB.Tests.Integration.InterviewTests.CodeGenerationTests
{
    internal class when_generating_assembly_and_questionnaire_has_question_condition_containing_self : CodeGenerationTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        private void BecauseOf() =>
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
                    ErrorLocations = emitResult.Diagnostics.Where(l => l.Severity == GenerationDiagnosticSeverity.Error)
                        .Select(l => l.Location).Distinct().Select(s => new ExpressionLocation(s)).ToArray()
                };
            });

        [NUnit.Framework.Test] public void should_result_succeded () =>
            results.Success.Should().Be(false);

        [NUnit.Framework.Test] public void should_errors_locations_count_equals_1 () =>
            results.ErrorLocations.Count().Should().Be(1);

        [NUnit.Framework.Test] public void should_errors_locations_contains_question1 () =>
            results.ErrorLocations.SingleOrDefault(x => x.Id == questionId 
                && x.ItemType == ExpressionLocationItemType.Question 
                && x.ExpressionType == ExpressionLocationType.Condition).Should().NotBeNull();
    
        
        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
        private static InvokeResults results;

        private static string questionStringId = "11111111111111111111111111111113";
        private static Guid questionId = Guid.Parse(questionStringId);

        [Serializable]
        public class InvokeResults
        {
            public bool Success { get; set; }
            public ExpressionLocation[] ErrorLocations { get; set; }
        }
    }
}
