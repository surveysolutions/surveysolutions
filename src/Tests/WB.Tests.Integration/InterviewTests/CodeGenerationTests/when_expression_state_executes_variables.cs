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
                /*        Guid questionId = Guid.Parse("11111111111111111111111111111112");
                        Guid rosterId = Guid.Parse("21111111111111111111111111111112");*/

                AssemblyContext.SetupServiceLocator();

                QuestionnaireDocument questionnaireDocument = Create.QuestionnaireDocument(questionnaireId/*, children: new[]
            {
                Create.Chapter(children: new IComposite[]
                {
                    Create.Roster(id: rosterId, variable: "fixed_roster", rosterSizeSourceType: RosterSizeSourceType.FixedTitles, fixedTitles: new string[] {"1", "2", "3"}),
                    Create.SingleOptionQuestion(questionId: questionId, variable: "a", linkedToRosterId: rosterId,linkedFilterExpression: "@rowcode>1" ),

                })
            }*/);
                IInterviewExpressionStateV9 state = GetInterviewExpressionState(questionnaireDocument, version: new Version(15, 0, 0)) as IInterviewExpressionStateV9;
            //    state.SerVariablePreviousValue(Create.Identity(variableId), 4);
                var variables = state.ProcessVariables();
                return new InvokeResults()
                {
                    BooleanVariableResult = (int?)variables.ChangedVariableValues[Create.Identity(variableId)]
                };
            });

        It should_result_variable_be_true = () =>
             results.BooleanVariableResult.ShouldEqual(4);

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        private static AppDomainContext appDomainContext;
        private static InvokeResults results;

        [Serializable]
        public class InvokeResults
        {
            public int? BooleanVariableResult { get; set; }
        }
    }
}