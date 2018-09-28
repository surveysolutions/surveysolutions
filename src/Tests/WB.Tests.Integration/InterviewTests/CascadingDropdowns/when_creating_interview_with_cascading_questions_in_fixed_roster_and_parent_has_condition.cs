using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.CascadingDropdowns
{
    internal class when_creating_interview_with_cascading_questions_in_fixed_roster_and_parent_has_condition : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        private void BecauseOf() =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var parentSingleOptionQuestionId = Guid.Parse("00000000000000000000000000000000");
                var childCascadedComboboxId = Guid.Parse("11111111111111111111111111111111");
                var grandChildCascadedComboboxId = Guid.Parse("22222222222222222222222222222222");
                var questionnaireId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                var topRosterId = Guid.Parse("44444444444444444444444444444444");
                var numericId = Guid.Parse("55555555555555555555555555555555");

                var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Create.Entity.NumericIntegerQuestion(numericId, "numeric"),
                    Create.Entity.Roster(topRosterId,
                        variable: "varRoster",
                        rosterSizeSourceType: RosterSizeSourceType.FixedTitles,
                        fixedTitles: new[] {"a", "b"},
                        children: new List<IComposite>
                        {
                            Create.Entity.SingleQuestion(parentSingleOptionQuestionId, "q1", "numeric > 10",
                                options: new List<Answer>
                                {
                                    Create.Entity.Option("1", "parent option 1"),
                                    Create.Entity.Option("2", "parent option 2")
                                }),
                            Create.Entity.SingleQuestion(childCascadedComboboxId, "q2",
                                cascadeFromQuestionId: parentSingleOptionQuestionId,
                                options:
                                new List<Answer>
                                {
                                    Create.Entity.Option("1", "child 1 for parent option 1", "1"),
                                    Create.Entity.Option("3", "child 1 for parent option 2", "2")
                                }
                            ),
                            Create.Entity.SingleQuestion(grandChildCascadedComboboxId, "q3",
                                cascadeFromQuestionId: childCascadedComboboxId,
                                options:
                                new List<Answer>
                                {
                                    Create.Entity.Option("1", "child 1 for parent option 1", "1"),
                                    Create.Entity.Option("3", "child 1 for parent option 2", "2")
                                }
                            )
                        })
                );

                using (var eventContext = new EventContext())
                {
                    SetupInterview(questionnaire);

                    return new InvokeResults
                    {
                        WasAnyQuestionEnabled = eventContext.AnyEvent<QuestionsEnabled>(),
                        WasAnyParentQuestionDisabled = eventContext.AnyEvent<QuestionsDisabled>(x => x.Questions.Any(q => q.Id == parentSingleOptionQuestionId)),
                        WasAnyChildQuestionDisabled = eventContext.AnyEvent<QuestionsDisabled>(x => x.Questions.Any(q => q.Id == childCascadedComboboxId)),
                        WasAnyGrandChildQuestionDisabled = eventContext.AnyEvent<QuestionsDisabled>(x => x.Questions.Any(q => q.Id == grandChildCascadedComboboxId))
                    };
                }
            });

        [NUnit.Framework.Test] public void should_not_enable_any_question () =>
            results.WasAnyQuestionEnabled.Should().BeFalse();

        [NUnit.Framework.Test] public void should_disable_all_parent_questions () =>
            results.WasAnyParentQuestionDisabled.Should().BeTrue();

        [NUnit.Framework.Test] public void should_disable_all_child_questions () =>
            results.WasAnyChildQuestionDisabled.Should().BeTrue();

        [NUnit.Framework.Test] public void should_disable_all_grand_child_questions () =>
            results.WasAnyGrandChildQuestionDisabled.Should().BeTrue();

        private static InvokeResults results;
        private AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;

        [Serializable]
        internal class InvokeResults
        {
            public bool WasAnyQuestionEnabled { get; set; }
            public bool WasAnyParentQuestionDisabled { get; set; }
            public bool WasAnyChildQuestionDisabled { get; set; }
            public bool WasAnyGrandChildQuestionDisabled { get; set; }
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }
    }
}
