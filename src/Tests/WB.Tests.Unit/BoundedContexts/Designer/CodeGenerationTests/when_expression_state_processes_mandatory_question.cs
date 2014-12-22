using System;
using System.Collections.Generic;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Documents;
using Microsoft.Practices.ServiceLocation;
using Moq;
using WB.Core.SharedKernels.DataCollection;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.CodeGenerationTests
{
    internal class when_expression_state_processes_mandatory_question : CodeGenerationTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
                Guid question1Id = Guid.Parse("11111111111111111111111111111112");
                Guid question2Id = Guid.Parse("21111111111111111111111111111112");
                Guid question3Id = Guid.Parse("31111111111111111111111111111112");
                Guid question4Id = Guid.Parse("41111111111111111111111111111112");

                AssemblyContext.SetupServiceLocator();

                QuestionnaireDocument questionnaireDocument = CreateQuestionnaireDocumenteHavingMandatoryQuestions(questionnaireId, question1Id, question2Id, question3Id, question4Id);
                IInterviewExpressionState state = GetInterviewExpressionState(questionnaireDocument);

                ValidityChanges validationChanges = state.ProcessValidationExpressions();

                return new InvokeResults
                {
                    ValidQuestionsCount = validationChanges.AnswersDeclaredValid.Count,
                    InvalidQuestionsCount = validationChanges.AnswersDeclaredInvalid.Count,
                };
            });

        It should_valid_question_count_equal_0 = () =>
            results.ValidQuestionsCount.ShouldEqual(0);

        It should_invalid_question_count_equal_4 = () =>
            results.InvalidQuestionsCount.ShouldEqual(4);

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
            public int ValidQuestionsCount { get; set; }
            public int InvalidQuestionsCount { get; set; }
        }
    }
}
