﻿using System;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.DataCollection.V9;

namespace WB.Tests.Integration.InterviewTests.CodeGenerationTests
{
    internal class when_expression_state_executes_variables_but_result_excecution_throw_exception : CodeGenerationTestsContext
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
                        Abc.Create.Entity.NumericIntegerQuestion(id:questionId, variable:"num"),
                        Create.Variable(id: variableId, expression: "1/(int)num.Value")
                    });
                IInterviewExpressionStateV9 state =
                    GetInterviewExpressionState(questionnaireDocument, version: 16) as
                        IInterviewExpressionStateV9;

                state.UpdateVariableValue(Create.Identity(variableId), 6);
                state.UpdateNumericIntegerAnswer(questionId, new decimal[0], 0);
                var variables = state.ProcessVariables();

                return new InvokeResults()
                {
                    IntVariableResult = (int?)variables.ChangedVariableValues[Create.Identity(variableId)]
                };
            });

        It should_result_of_the_variable_be_equal_to_null = () =>
             results.IntVariableResult.ShouldEqual(null);

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
        }
    }
}