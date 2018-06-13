using System;
using System.Linq;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Documents;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Tests.Integration.InterviewTests.CodeGenerationTests
{
    internal class when_expression_state_processes_condition_expressions : CodeGenerationTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        private void BecauseOf() =>
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

        [NUnit.Framework.Test] public void should_disabled_question_count_equal_0 () =>
            results.QuestionsToBeDisabledCount.Should().Be(0);

        [NUnit.Framework.Test] public void should_enabled_question_count_equal_0 () =>
            results.QuestionsToBeEnabledCount.Should().Be(0);

        [NUnit.Framework.Test] public void should_disabled_group_count_equal_1 () =>
            results.GroupsToBeDisabledCount.Should().Be(1);

        [NUnit.Framework.Test] public void should_disabled_group_id_equal_group1id () =>
            results.DisabledGroupId.Should().Be(Guid.Parse("23232323232323232323232323232111"));

        [NUnit.Framework.Test] public void should_enable_group_count_equal_0 () =>
            results.GroupsToBeEnabledCount.Should().Be(0);

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
            public int QuestionsToBeDisabledCount { get; set; }
            public int QuestionsToBeEnabledCount { get; set; }
            public int GroupsToBeDisabledCount { get; set; }
            public Guid DisabledGroupId { get; set; }
            public int GroupsToBeEnabledCount { get; set; }
        }
    }
}
