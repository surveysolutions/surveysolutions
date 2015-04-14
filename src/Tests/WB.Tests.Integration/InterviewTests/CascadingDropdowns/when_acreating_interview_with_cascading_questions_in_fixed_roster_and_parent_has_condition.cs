using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Tests.Integration.InterviewTests.CascadingDropdowns
{
    internal class when_acreating_interview_with_cascading_questions_in_fixed_roster_and_parent_has_condition : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();

                var parentSingleOptionQuestionId = Guid.Parse("00000000000000000000000000000000");
                var childCascadedComboboxId = Guid.Parse("11111111111111111111111111111111");
                var grandChildCascadedComboboxId = Guid.Parse("22222222222222222222222222222222");
                var questionnaireId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                var actorId = Guid.Parse("33333333333333333333333333333333");
                var topRosterId = Guid.Parse("44444444444444444444444444444444");
                var numericId = Guid.Parse("55555555555555555555555555555555");

                var questionnaire = Create.QuestionnaireDocument(questionnaireId,
                    Create.NumericIntegerQuestion(numericId, variable: "numeric"),
                    Create.Roster(topRosterId,
                        variable: "varRoster",
                        rosterSizeSourceType: RosterSizeSourceType.FixedTitles,
                        fixedTitles: new []{ "a", "b"},
                        children: new List<IComposite>
                        {
                            Create.SingleQuestion(parentSingleOptionQuestionId, "q1", enablementCondition: "numeric > 10",
                                options: new List<Answer>
                                {
                                    Create.Option(text: "parent option 1", value: "1"),
                                    Create.Option(text: "parent option 2", value: "2")
                                }),
                            Create.SingleQuestion(childCascadedComboboxId, "q2", cascadeFromQuestionId: parentSingleOptionQuestionId,
                                options:
                                    new List<Answer>
                                    {
                                        Create.Option(text: "child 1 for parent option 1", value: "1", parentValue: "1"),
                                        Create.Option(text: "child 1 for parent option 2", value: "3", parentValue: "2")
                                    }
                                ),
                            Create.SingleQuestion(grandChildCascadedComboboxId, "q3", cascadeFromQuestionId: childCascadedComboboxId,
                                options:
                                    new List<Answer>
                                    {
                                        Create.Option(text: "child 1 for parent option 1", value: "1", parentValue: "1"),
                                        Create.Option(text: "child 1 for parent option 2", value: "3", parentValue: "2")
                                    }
                                ),

                        })
                    );
                
                using (var eventContext = new EventContext())
                {
                    Interview interview = SetupInterview(questionnaire);

                    return new InvokeResults
                    {
                        WasAnyQuestionEnabled = eventContext.AnyEvent<QuestionsEnabled>(),
                        WasAnyParentQuestionDisabled = eventContext.AnyEvent<QuestionsDisabled>(x => x.Questions.Any(q => q.Id == parentSingleOptionQuestionId)),
                        WasAnyChildQuestionDisabled = eventContext.AnyEvent<QuestionsDisabled>(x => x.Questions.Any(q => q.Id == childCascadedComboboxId)),
                        WasAnyGrandChildQuestionDisabled = eventContext.AnyEvent<QuestionsDisabled>(x => x.Questions.Any(q => q.Id == grandChildCascadedComboboxId)),
                    };
                }
            });

        It should_not_enable_any_question = () =>
            results.WasAnyQuestionEnabled.ShouldBeFalse();

        It should_disable_all_parent_questions = () =>
            results.WasAnyParentQuestionDisabled.ShouldBeTrue();

        It should_disable_all_child_questions = () =>
            results.WasAnyChildQuestionDisabled.ShouldBeTrue();

        It should_disable_all_grand_child_questions = () =>
            results.WasAnyGrandChildQuestionDisabled.ShouldBeTrue();

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        private static InvokeResults results;
        private static AppDomainContext appDomainContext;

        [Serializable]
        internal class InvokeResults
        {
            public bool WasAnyQuestionEnabled { get; set; }
            public bool WasAnyParentQuestionDisabled { get; set; }
            public bool WasAnyChildQuestionDisabled { get; set; }
            public bool WasAnyGrandChildQuestionDisabled { get; set; }
        }
    }
}