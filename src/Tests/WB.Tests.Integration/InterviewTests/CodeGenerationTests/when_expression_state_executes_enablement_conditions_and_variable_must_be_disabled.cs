using System;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.DataCollection.V9;

namespace WB.Tests.Integration.InterviewTests.CodeGenerationTests
{
    internal class when_expression_state_executes_enablement_conditions_and_variable_must_be_disabled : CodeGenerationTestsContext
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
                Guid groupId = Guid.Parse("31111111111111111111111111111112");

                AssemblyContext.SetupServiceLocator();

                QuestionnaireDocument questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    children: new IComposite[]
                    {
                        Create.Group(id: groupId, enablementCondition:"false", children: new IComposite[]
                        {
                            Create.TextQuestion(id: questionId, variable: "txt"),
                            Create.Variable(id: variableId, expression: "txt.Length")
                        })
                    });
                IInterviewExpressionStateV9 state =
                    GetInterviewExpressionState(questionnaireDocument, version: 15) as
                        IInterviewExpressionStateV9;

                state.UpdateTextAnswer(questionId, new decimal[0], "Nastya");
                state.UpdateVariableValue(Create.Identity(variableId), 6);
                var enablementConditions = state.ProcessEnablementConditions();

                var variables = state.ProcessVariables();

                return new InvokeResults()
                {
                    IntVariableResult = (int?)variables.ChangedVariableValues[Create.Identity(variableId)],
                    IsVariableDisabled = enablementConditions.VariablesToBeDisabled.Contains(Create.Identity(variableId))
                };
            });

        It should_result_of_the_variable_be_null = () =>
             results.IntVariableResult.ShouldEqual(null);

        It should_variable_id_be_returned_as_disabled = () =>
           results.IsVariableDisabled.ShouldBeTrue();

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
            public int? IntVariableResult { get; set; }
            public bool IsVariableDisabled { get; set; }
        }
    }
}