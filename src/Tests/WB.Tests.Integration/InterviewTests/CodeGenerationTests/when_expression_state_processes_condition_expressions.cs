using System;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.SharedKernels.DataCollection;
using It = Machine.Specifications.It;

namespace WB.Tests.Integration.InterviewTests.CodeGenerationTests
{
    internal class when_expression_state_processes_condition_expressions : CodeGenerationTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Guid questionnaireId = Guid.Parse("21111111111111111111111111111111");
                Guid questionId = Guid.Parse("11111111111111111111111111111112");
                Guid group1Id = Guid.Parse("23232323232323232323232323232111");

                AssemblyContext.SetupServiceLocator();

                QuestionnaireDocument questionnaireDocument = CreateQuestionnairDocumenteWithTwoNumericIntegerQuestionAndConditionalGroup(questionnaireId,
                    questionId, group1Id);

                IInterviewExpressionState state = GetInterviewExpressionState(questionnaireDocument);

                state.UpdateNumericIntegerAnswer(questionId, new decimal[0], 4);
                EnablementChanges enablementChanges = state.ProcessEnablementConditions();

                return new InvokeResults
                {
                    QuestionsToBeDisabledCount = enablementChanges.QuestionsToBeDisabled.Count,
                    QuestionsToBeEnabledCount = enablementChanges.QuestionsToBeEnabled.Count,
                    GroupsToBeDisabledCount = enablementChanges.GroupsToBeDisabled.Count,
                    DisabledGroupId = enablementChanges.GroupsToBeDisabled.Single().Id,
                    GroupsToBeEnabledCount = enablementChanges.GroupsToBeEnabled.Count,
                };
            });

        It should_disabled_question_count_equal_0 = () =>
            results.QuestionsToBeDisabledCount.ShouldEqual(0);

        It should_enabled_question_count_equal_0 = () =>
            results.QuestionsToBeEnabledCount.ShouldEqual(0);

        It should_disabled_group_count_equal_1 = () =>
            results.GroupsToBeDisabledCount.ShouldEqual(1);

        It should_disabled_group_id_equal_group1id = () =>
            results.DisabledGroupId.ShouldEqual(Guid.Parse("23232323232323232323232323232111"));

        It should_enable_group_count_equal_0 = () =>
            results.GroupsToBeEnabledCount.ShouldEqual(0);

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
            public int QuestionsToBeDisabledCount { get; set; }
            public int QuestionsToBeEnabledCount { get; set; }
            public int GroupsToBeDisabledCount { get; set; }
            public Guid DisabledGroupId { get; set; }
            public int GroupsToBeEnabledCount { get; set; }
        }
    }
}
