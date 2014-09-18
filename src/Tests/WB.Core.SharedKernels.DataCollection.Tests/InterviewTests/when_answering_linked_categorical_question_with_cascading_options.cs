using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests
{
    [Subject(typeof(Interview))]
    internal class when_answering_linked_categorical_question_with_cascading_options : InterviewTestsContext
    {
        private Establish context = () =>
        {
            parentSingleOptionQuestionId = Guid.Parse("9E96D4AB-DF91-4FC9-9585-23FA270B25D7");
            childCascadedComboboxId = Guid.Parse("C6CC807A-3E81-406C-A110-1044AE3FD89B");
            grandChildCascadedComboboxId = Guid.Parse("4C603B8A-3237-4915-96FA-8D1568C679E2");

            var questionnaireId = Guid.NewGuid();

            var nonAnsweredCombo = Guid.NewGuid();
            comboShouldNotBeRemoved = Guid.NewGuid();
            Questionnaire questionnaire = Create.Questionnaire(Guid.NewGuid(),
                CreateQuestionnaireDocumentWithOneChapter(new MultyOptionsQuestion
                {
                    PublicKey = parentSingleOptionQuestionId,
                    QuestionType = QuestionType.SingleOption,
                    Answers = new List<Answer>
                    {
                        new Answer { AnswerText = "one", AnswerValue = "1", PublicKey = Guid.NewGuid() }
                    }
                },
                    new SingleQuestion
                    {
                        PublicKey = childCascadedComboboxId,
                        QuestionType = QuestionType.SingleOption,
                        IsCascadingCombobox = true,
                        CascadeFromQuestionId = parentSingleOptionQuestionId,
                        Answers = new List<Answer>
                        {
                            new Answer { AnswerText = "grind", AnswerValue = "2", PublicKey = Guid.NewGuid() }
                        }
                    }, new SingleQuestion
                    {
                        PublicKey = grandChildCascadedComboboxId,
                        QuestionType = QuestionType.SingleOption,
                        IsCascadingCombobox = true,
                        CascadeFromQuestionId = childCascadedComboboxId,
                        Answers = new List<Answer>
                        {
                            new Answer { AnswerText = "crab", AnswerValue = "3", PublicKey = Guid.NewGuid() }
                        }
                    },
                    new SingleQuestion
                    {
                        PublicKey = nonAnsweredCombo,
                        QuestionType = QuestionType.SingleOption,
                        Answers = new List<Answer> {
                            new Answer { AnswerText = "one", AnswerValue = "1", PublicKey = Guid.NewGuid() }
                        }
                    },
                    new SingleQuestion
                    {
                        PublicKey = comboShouldNotBeRemoved,
                        QuestionType = QuestionType.SingleOption,
                        IsCascadingCombobox = true,
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
            eventContext = new EventContext();
        };

        Because of = () => interview.AnswerSingleOptionQuestion(Guid.NewGuid(), parentSingleOptionQuestionId, new decimal[] { }, DateTime.Now, 1);

        It should_remove_answer_from_2_questions = () => 
            eventContext.ShouldNotContainEvent<AnswersRemoved>(x => x.Questions.Any(q => q.Id == comboShouldNotBeRemoved));

        It should_remove_dependent_answers = () =>
            eventContext.ShouldContainEvent<AnswersRemoved>(x => x.Questions.Any(q => q.Id == childCascadedComboboxId));

        It should_remove_dependent_answers_on_second_level_of_cascades = () =>
            eventContext.ShouldContainEvent<AnswersRemoved>(x => x.Questions.Any(q => q.Id == grandChildCascadedComboboxId));

        private static Interview interview;
        private static EventContext eventContext;
        private static Guid parentSingleOptionQuestionId;
        private static Guid childCascadedComboboxId;
        private static Guid grandChildCascadedComboboxId;
        private static Guid comboShouldNotBeRemoved;
    }
}

