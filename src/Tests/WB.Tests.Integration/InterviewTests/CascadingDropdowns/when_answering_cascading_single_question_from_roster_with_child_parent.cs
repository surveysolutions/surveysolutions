using System;
using System.Collections.Generic;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
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
                        new SingleQuestion
                        {
                            PublicKey = parentSingleOptionQuestionId,
                            QuestionType = QuestionType.SingleOption,
                            Answers = new List<Answer>
                            {
                                new Answer { AnswerText = "parent option 1", AnswerValue = "1", PublicKey = Guid.NewGuid() },
                                new Answer { AnswerText = "parent option 2", AnswerValue = "2", PublicKey = Guid.NewGuid() }
                            }
                        },
                        new Group("roster")
                        {
                            PublicKey = topRosterId,
                            IsRoster = true,
                            RosterSizeSource = RosterSizeSourceType.FixedTitles,
                            RosterFixedTitles = new[] { "a", "b" },
                            Children = new List<IComposite>
                            {
                                new SingleQuestion
                                {
                                    PublicKey = childCascadedComboboxId,
                                    QuestionType = QuestionType.SingleOption,
                                    CascadeFromQuestionId = parentSingleOptionQuestionId,
                                    Answers = new List<Answer>
                                    {
                                        new Answer
                                        {
                                            AnswerText = "child 1 for parent option 1",
                                            AnswerValue = "1",
                                            PublicKey = Guid.NewGuid(),
                                            ParentValue = "1"
                                        },
                                        new Answer
                                        {
                                            AnswerText = "child 1 for parent option 2",
                                            AnswerValue = "2",
                                            PublicKey = Guid.NewGuid(),
                                            ParentValue = "2"
                                        }
                                    }
                                }
                            }
                        });

                var interview = SetupInterview(questionnaire);

                interview.AnswerSingleOptionQuestion(actorId, parentSingleOptionQuestionId, new decimal[] { }, DateTime.Now, 1);

                using (var eventContext = new EventContext())
                {
                    interview.AnswerSingleOptionQuestion(actorId, childCascadedComboboxId, new decimal[] { 0 }, DateTime.Now, 1);

                    return new InvokeResults
                    {
                         WasChildAnswerSaved = eventContext.AnyEvent<SingleOptionQuestionAnswered>(x => x.SelectedValue == 1)
                    };
                }
            });

        It should_answer_on_question_with_selectedValue_equals_1 = () =>
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