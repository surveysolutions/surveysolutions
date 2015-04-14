using System;
using System.Collections.Generic;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using It = Machine.Specifications.It;

namespace WB.Tests.Integration.InterviewTests.CascadingDropdowns
{
    internal class when_answering_cascading_single_question_from_roster_with_child_parent : InterviewTestsContext
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
                var questionnaireId = Guid.Parse("22222222222222222222222222222222");
                var actorId = Guid.Parse("33333333333333333333333333333333");
                var topRosterId = Guid.Parse("44444444444444444444444444444444");


                var questionnaire = Create.QuestionnaireDocument(questionnaireId,
                    Create.SingleQuestion(parentSingleOptionQuestionId, "q1", options: new List<Answer>
                    {
                        Create.Option(text: "parent option 1", value: "1"),
                        Create.Option(text: "parent option 2", value: "2")
                    }),
                    Create.Roster(topRosterId,
                        variable: "varRoster",
                        rosterSizeSourceType: RosterSizeSourceType.FixedTitles,
                        fixedTitles: new[] { "a", "b" },
                        children: new List<IComposite>
                        {
                            Create.SingleQuestion(childCascadedComboboxId, "q2", cascadeFromQuestionId: parentSingleOptionQuestionId,
                                options:
                                    new List<Answer>
                                    {
                                        Create.Option(text: "child 1 for parent option 1", value: "1", parentValue: "1"),
                                        Create.Option(text: "child 1 for parent option 2", value: "3", parentValue: "2")
                                    }
                                )
                        })
                    );

                Interview interview = SetupInterview(questionnaire, new List<object>
                {
                    Create.Event.SingleOptionQuestionAnswered(questionId: parentSingleOptionQuestionId, answer: 1, propagationVector: new decimal[] { }),
                    Create.Event.QuestionsEnabled(
                        Create.Identity(childCascadedComboboxId, new decimal[] { 0 }),
                        Create.Identity(childCascadedComboboxId, new decimal[] { 0 }))

                });

                using (var eventContext = new EventContext())
                {
                    interview.AnswerSingleOptionQuestion(actorId, childCascadedComboboxId, new decimal[] { 0 }, DateTime.Now, 1);

                    return new InvokeResults
                    {
                        WasChildAnswerSaved = eventContext.AnyEvent<SingleOptionQuestionAnswered>(x => x.SelectedValue == 1)
                    };
                }
            });

        It should_answer_on_single_option_question_with_selectedValue_equals_1 = () =>
           results.WasChildAnswerSaved.ShouldBeTrue();

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
            public bool WasChildAnswerSaved { get; set; }
        }
    }
}