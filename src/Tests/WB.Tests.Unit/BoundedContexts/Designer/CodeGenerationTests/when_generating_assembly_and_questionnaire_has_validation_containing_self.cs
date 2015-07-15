using System;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Tests.Unit.BoundedContexts.Designer.CodeGenerationTests
{
    internal class when_generating_assembly_and_questionnaire_has_validation_containing_self : CodeGenerationTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
                Guid questionId = Guid.Parse("11111111111111111111111111111112");

                AssemblyContext.SetupServiceLocator();

                QuestionnaireDocument questionnaireDocument = CreateQuestionnaireDocumenteWithOneNumericIntegerQuestionWithValidationUsingSelf(questionnaireId, questionId);
                IInterviewExpressionState state = GetInterviewExpressionState(questionnaireDocument);

                state.UpdateNumericIntegerAnswer(questionId, new decimal[0], 4);
                

                ValidityChanges validationChanges = state.ProcessValidationExpressions();

                return new InvokeResults
                {
                    ValidQuestionsCount = validationChanges.AnswersDeclaredValid.Count,
                    InvalidQuestionsCount = validationChanges.AnswersDeclaredInvalid.Count,
                };
            });

        It should_valid_question_count_equal_2 = () =>
            results.ValidQuestionsCount.ShouldEqual(1);

        It should_invalid_question_count_equal_0 = () =>
            results.InvalidQuestionsCount.ShouldEqual(0);

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
