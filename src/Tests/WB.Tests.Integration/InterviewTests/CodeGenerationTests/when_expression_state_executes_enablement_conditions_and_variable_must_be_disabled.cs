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
    internal class when_expression_state_executes_enablement_conditions_and_variable_must_be_disabled : CodeGenerationTestsContext
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
                Guid groupId = Guid.Parse("31111111111111111111111111111112");

                AssemblyContext.SetupServiceLocator();

                QuestionnaireDocument questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    children: new IComposite[]
                    {
                        Abc.Create.Entity.Group(groupId, "Group X", null, "false", false, new IComposite[]
                        {
                            Abc.Create.Entity.TextQuestion(questionId: questionId, variable: "txt"),
                            Create.Entity.Variable(variableId, VariableType.LongInteger, "v1", "txt.Length")
                        })
                    });
                IInterviewExpressionStateV9 state =
                    GetInterviewExpressionState(questionnaireDocument, version: 16) as
                        IInterviewExpressionStateV9;

                state.UpdateTextAnswer(questionId, new decimal[0], "Nastya");
                state.UpdateVariableValue(Abc.Create.Identity(variableId), 6);
                var enablementConditions = state.ProcessEnablementConditions();

                var variables = state.ProcessVariables();

                return new InvokeResults()
                {
                    IntVariableResult = (int?)variables.ChangedVariableValues[Abc.Create.Identity(variableId)],
                    IsVariableDisabled = enablementConditions.VariablesToBeDisabled.Contains(Abc.Create.Identity(variableId))
                };
            });

        [NUnit.Framework.Test] public void should_result_of_the_variable_be_null () =>
             results.IntVariableResult.Should().Be(null);

        [NUnit.Framework.Test] public void should_variable_id_be_returned_as_disabled () =>
           results.IsVariableDisabled.Should().BeTrue();

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
            public bool IsVariableDisabled { get; set; }
        }
    }
}
