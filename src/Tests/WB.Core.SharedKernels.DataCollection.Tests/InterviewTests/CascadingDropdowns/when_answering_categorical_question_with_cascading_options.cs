using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
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
    [Subject(typeof(Interview))]
    [Ignore("Cascading")]
    internal class when_answering_categorical_question_with_cascading_options : InterviewTestsContext
    {
        private Establish context = () =>
        {
            parentSingleOptionQuestionId = Guid.Parse("11111111111111111111111111111111");
            childCascadedComboboxId = Guid.Parse("22222222222222222222222222222222");
            grandChildCascadedComboboxId = Guid.Parse("33333333333333333333333333333333");

            var questionnaireId = Guid.NewGuid();
            actorId = Guid.NewGuid();

            var nonAnsweredCombo = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            comboShouldNotBeRemoved = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            Questionnaire questionnaire = Create.Questionnaire(actorId,
                CreateQuestionnaireDocumentWithOneChapter(
                    new SingleQuestion
                    {
                        PublicKey = parentSingleOptionQuestionId,
                        QuestionType = QuestionType.SingleOption,
                        Answers = new List<Answer>
                        {
                            new Answer { AnswerText = "one", AnswerValue = "1", PublicKey = Guid.NewGuid() },
                            new Answer { AnswerText = "two", AnswerValue = "2", PublicKey = Guid.NewGuid() }
                        }
                    },
                    new SingleQuestion
                    {
                        PublicKey = childCascadedComboboxId,
                        QuestionType = QuestionType.SingleOption,
                        StataExportCaption = "q1",
                        Mandatory = true,
                        CascadeFromQuestionId = parentSingleOptionQuestionId,
                        Answers = new List<Answer>
                        {
                            new Answer { AnswerText = "child 1", AnswerValue = "1", PublicKey = Guid.NewGuid(), ParentValue = "1"},
                            new Answer { AnswerText = "child 2", AnswerValue = "2", PublicKey = Guid.NewGuid(), ParentValue = "2" }
                        }
                    },
                    new SingleQuestion
                    {
                        PublicKey = grandChildCascadedComboboxId,
                        QuestionType = QuestionType.SingleOption,
                        CascadeFromQuestionId = childCascadedComboboxId,
                        StataExportCaption = "q2",
                        Answers = new List<Answer>
                        {
                            new Answer { AnswerText = "grand child", AnswerValue = "1", PublicKey = Guid.NewGuid(), ParentValue = "1"}
                        }
                    },
                    new SingleQuestion
                    {
                        PublicKey = nonAnsweredCombo,
                        QuestionType = QuestionType.SingleOption,
                        StataExportCaption = "q3",
                        Answers = new List<Answer> {
                            new Answer { AnswerText = "other cascade", AnswerValue = "1", PublicKey = Guid.NewGuid() }
                        }
                    },
                    new SingleQuestion
                    {
                        PublicKey = comboShouldNotBeRemoved,
                        QuestionType = QuestionType.SingleOption,
                        StataExportCaption = "q4",
                        CascadeFromQuestionId = nonAnsweredCombo,
                        Answers = new List<Answer> {
                            new Answer { AnswerText = "blue", AnswerValue = "1", PublicKey = Guid.NewGuid() }
                        }
                    }));

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire.GetQuestionnaire());

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);

            interview = CreateInterview(questionnaireId: questionnaireId);

            interview.AnswerSingleOptionQuestion(actorId, parentSingleOptionQuestionId, new decimal[] { }, DateTime.Now, 1);
            interview.AnswerSingleOptionQuestion(actorId, childCascadedComboboxId, new decimal[] { }, DateTime.Now, 1);
            
            eventContext = new EventContext();
        };

        Because of = () => interview.AnswerSingleOptionQuestion(Guid.NewGuid(), parentSingleOptionQuestionId, new decimal[] { }, DateTime.Now, 2);

        It should_enable_child_question = () =>
            eventContext.ShouldContainEvent<QuestionsEnabled>(x => x.Questions.Any(q => q.Id == childCascadedComboboxId));

        It should_invalidate_child_question = () =>
            eventContext.ShouldContainEvent<AnswersDeclaredInvalid>(x => x.Questions.Any(q => q.Id == childCascadedComboboxId));

        It should_disable_grandchild_question = () =>
            eventContext.ShouldContainEvent<QuestionsDisabled>(x => x.Questions.Any(q => q.Id == grandChildCascadedComboboxId));

        It should_not_enable_grandchild_question = () =>
            eventContext.ShouldNotContainEvent<QuestionsEnabled>(x => x.Questions.Any(q => q.Id == grandChildCascadedComboboxId));

        It should_not_remove_answer_from_self = () =>
            eventContext.ShouldNotContainEvent<AnswersRemoved>(x => x.Questions.Any(q => q.Id == parentSingleOptionQuestionId));

        It should_not_remove_answer_from_not_related_question = () => 
            eventContext.ShouldNotContainEvent<AnswersRemoved>(x => x.Questions.Any(q => q.Id == comboShouldNotBeRemoved));

        It should_remove_dependent_answers = () =>
            eventContext.ShouldContainEvent<AnswersRemoved>(x => x.Questions.Any(q => q.Id == childCascadedComboboxId));

        It should_remove_dependent_answers_on_second_level_of_cascades_if_it_is_answered = () =>
            eventContext.ShouldNotContainEvent<AnswersRemoved>(x => x.Questions.Any(q => q.Id == grandChildCascadedComboboxId));

        static Interview interview;
        static EventContext eventContext;
        static Guid parentSingleOptionQuestionId;
        static Guid childCascadedComboboxId;
        static Guid grandChildCascadedComboboxId;
        static Guid comboShouldNotBeRemoved;
        static Guid actorId;

        Cleanup stuff = () => eventContext.Dispose();
    }
}

