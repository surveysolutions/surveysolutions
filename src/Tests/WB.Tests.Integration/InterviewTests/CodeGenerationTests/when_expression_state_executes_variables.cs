using System;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.V7;
using WB.Core.SharedKernels.DataCollection.V9;

namespace WB.Tests.Integration.InterviewTests.CodeGenerationTests
{
    internal class when_expression_state_executes_variables : CodeGenerationTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
                Guid variableId = Guid.Parse("11111111111111111111111111111112");
                        Guid questionId = Guid.Parse("21111111111111111111111111111112");

                AssemblyContext.SetupServiceLocator();

                QuestionnaireDocument questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    children: new IComposite[]
                    {
                        Create.TextQuestion(id:questionId, variable:"txt"),
                        Create.Variable(id: variableId, expression: "txt.Length")
                    });
                IInterviewExpressionStateV9 state =
                    GetInterviewExpressionState(questionnaireDocument, version: 15) as
                        IInterviewExpressionStateV9;

                state.UpdateTextAnswer(questionId, new decimal[0], "Nastya");
                var variables = state.ProcessVariables();

                return new InvokeResults()
                {
                    IntVariableResult = (long?) variables.ChangedVariableValues[Create.Identity(variableId)]
                };
            });

        It should_result_of_the_variable_be_equal_to_length_of_answer_on_text_question = () =>
             results.IntVariableResult.ShouldEqual(6);

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
        private static InvokeResults results;

        [Serializable]
        public class InvokeResults
        {
            public long? IntVariableResult { get; set; }
        }
    }
}