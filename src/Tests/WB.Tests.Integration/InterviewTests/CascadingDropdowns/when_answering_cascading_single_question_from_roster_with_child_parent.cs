using System;
using System.Collections.Generic;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.CascadingDropdowns
{
    internal class when_answering_cascading_single_question_from_roster_with_child_parent : InterviewTestsContext
    {
        [Test]
        public void should_answer_on_single_option_question_with_selectedValue_equals_1()
        {
            using (var appDomainContext = AppDomainContext.Create())
            {
                results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
                {
                    SetUp.MockedServiceLocator();

                    var parentSingleOptionQuestionId = Guid.Parse("10000000000000000000000000000000");
                    var childCascadedComboboxId = Guid.Parse("11111111111111111111111111111111");
                    var questionnaireId = Guid.Parse("22222222222222222222222222222222");
                    var actorId = Guid.Parse("33333333333333333333333333333333");
                    var topRosterId = Guid.Parse("44444444444444444444444444444444");


                    var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                        Create.Entity.SingleQuestion(parentSingleOptionQuestionId, "q1", options: new List<Answer>
                        {
                            Create.Entity.Option("1", "parent option 1"),
                            Create.Entity.Option("2", "parent option 2")
                        }),
                        Create.Entity.Roster(topRosterId,
                            variable: "varRoster",
                            rosterSizeSourceType: RosterSizeSourceType.FixedTitles,
                            fixedTitles: new[] {"a", "b"},
                            children: new List<IComposite>
                            {
                                Create.Entity.SingleQuestion(childCascadedComboboxId, "q2",
                                    cascadeFromQuestionId: parentSingleOptionQuestionId,
                                    options:
                                    new List<Answer>
                                    {
                                        Create.Entity.Option("1", "child 1 for parent option 1", "1"),
                                        Create.Entity.Option("3", "child 1 for parent option 2", "2")
                                    }
                                )
                            })
                    );

                    Interview interview = SetupInterviewWithExpressionStorage(questionnaire);

                    interview.AnswerSingleOptionQuestion(actorId, parentSingleOptionQuestionId, RosterVector.Empty,
                        DateTime.Now, 1);

                    using (var eventContext = new EventContext())
                    {
                        interview.AnswerSingleOptionQuestion(actorId, childCascadedComboboxId, new decimal[] {0},
                            DateTime.Now, 1);

                        return new InvokeResults
                        {
                            WasChildAnswerSaved =
                                eventContext.AnyEvent<SingleOptionQuestionAnswered>(x => x.SelectedValue == 1)
                        };
                    }
                });
            }

            results.WasChildAnswerSaved.Should().BeTrue();
        }

        private static InvokeResults results;

        [Serializable]
        internal class InvokeResults
        {
            public bool WasChildAnswerSaved { get; set; }
        }
    }
}
