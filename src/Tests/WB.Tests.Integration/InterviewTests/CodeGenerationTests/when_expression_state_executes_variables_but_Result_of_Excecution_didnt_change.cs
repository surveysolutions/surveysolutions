using System;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.DataCollection.V9;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.CodeGenerationTests
{
    internal class when_expression_state_executes_variables_but_result_of_Excecution_didnt_change : CodeGenerationTestsContext
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

                QuestionnaireDocument questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    children: new IComposite[]
                    {
                        Create.Entity.TextQuestion(questionId: questionId, variable: "txt"),
                        Create.Entity.Variable(variableId, VariableType.LongInteger, "v1", "txt.Length")
                    });
                IInterviewExpressionStateV9 state =
                    GetInterviewExpressionState(questionnaireDocument, version: 16) as
                        IInterviewExpressionStateV9;
                state.EnableVariables(new [] { Create.Identity(variableId) });
                state.UpdateTextAnswer(questionId, new decimal[0], "Nastya");
                state.UpdateVariableValue(Create.Identity(variableId), (long)6);
                 var variables = state.ProcessVariables();

                return new InvokeResults
                {
                    CountOfChangedVariables = variables.ChangedVariableValues.Count
                };
            });

        It should_result_not_return_the_variable_changed_result = () =>
             results.CountOfChangedVariables.ShouldEqual(0);

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
        static InvokeResults results;

        [Serializable]
        public class InvokeResults
        {
            public int CountOfChangedVariables { get; set; }
        }
    }
}