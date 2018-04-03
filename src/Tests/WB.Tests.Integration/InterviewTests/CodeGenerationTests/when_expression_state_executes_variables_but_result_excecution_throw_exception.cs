using System;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.DataCollection.V9;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.CodeGenerationTests
{
    internal class when_expression_state_executes_variables_but_result_excecution_throw_exception : CodeGenerationTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        private void BecauseOf() =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
                Guid variableId = Guid.Parse("11111111111111111111111111111112");
                Guid questionId = Guid.Parse("21111111111111111111111111111112");

                AssemblyContext.SetupServiceLocator();

                QuestionnaireDocument questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    children: new IComposite[]
                    {
                        Create.Entity.NumericIntegerQuestion(id:questionId, variable:"num"),
                        Create.Entity.Variable(variableId, VariableType.LongInteger, "v1", "1/(int)num.Value")
                    });
                IInterviewExpressionStateV9 state =
                    GetInterviewExpressionState(questionnaireDocument, version: 16) as
                        IInterviewExpressionStateV9;

                state.UpdateVariableValue(Create.Identity(variableId), 6);
                state.UpdateNumericIntegerAnswer(questionId, new decimal[0], 0);
                var variables = state.ProcessVariables();

                return new InvokeResults()
                {
                    IntVariableResult = (int?)variables.ChangedVariableValues[Abc.Create.Identity(variableId)]
                };
            });

        [NUnit.Framework.Test] public void should_result_of_the_variable_be_equal_to_null () =>
             results.IntVariableResult.Should().Be(null);

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
        private static InvokeResults results;

        [Serializable]
        public class InvokeResults
        {
            public int? IntVariableResult { get; set; }
        }
    }
}
