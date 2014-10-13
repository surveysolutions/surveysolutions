using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests.CascadingDropdowns
{
    internal class when_answering_cascading_single_question_from_roster_with_child_parent : InterviewTestsContext
    {
        Establish context = () =>
        {
            parentSingleOptionQuestionId = Guid.Parse("00000000000000000000000000000000");
            childCascadedComboboxId = Guid.Parse("11111111111111111111111111111111");
            var questionnaireId = Guid.Parse("22222222222222222222222222222222");
            actorId = Guid.Parse("33333333333333333333333333333333");
            var topRosterId = Guid.Parse("44444444444444444444444444444444");

            var questionnaire = Create.Questionnaire(actorId,
                CreateQuestionnaireDocumentWithOneChapter(
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
                        RosterFixedTitles = new []{"a", "b"},
                        Children = new List<IComposite>
                        {
                            new SingleQuestion
                            {
                                PublicKey = childCascadedComboboxId,
                                QuestionType = QuestionType.SingleOption,
                                CascadeFromQuestionId = parentSingleOptionQuestionId,
                                Answers = new List<Answer>
                                {
                                    new Answer { AnswerText = "child 1 for parent option 1", AnswerValue = "1", PublicKey = Guid.NewGuid(), ParentValue = "1" },
                                    new Answer { AnswerText = "child 1 for parent option 2", AnswerValue = "2", PublicKey = Guid.NewGuid(), ParentValue = "2" }
                                }
                            }
                        }
                    }));

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire.GetQuestionnaire());

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);

            interview = CreateInterview(questionnaireId: questionnaireId);

            interview.AnswerSingleOptionQuestion(actorId, parentSingleOptionQuestionId, new decimal[] { }, DateTime.Now, 1);

            eventContext = new EventContext();
        };

        Because of = () =>
            interview.AnswerSingleOptionQuestion(actorId, childCascadedComboboxId, new decimal[] { 0 }, DateTime.Now, 1);

        It should_answer_on_question_with_selectedValue_equals_1 = () =>
            eventContext.ShouldContainEvent<SingleOptionQuestionAnswered>(x => x.SelectedValue == 1);

        static Interview interview;
        static EventContext eventContext;
        static Guid parentSingleOptionQuestionId;
        static Guid childCascadedComboboxId;
        static Guid actorId;
    }
}