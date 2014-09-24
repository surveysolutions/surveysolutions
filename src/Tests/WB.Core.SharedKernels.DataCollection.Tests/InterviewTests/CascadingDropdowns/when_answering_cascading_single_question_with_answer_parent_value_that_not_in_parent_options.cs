using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests.CascadingDropdowns
{
    [Subject(typeof(Interview))]
    internal class when_answering_cascading_single_question_with_answer_parent_value_that_not_in_parent_options : InterviewTestsContext
    {
        Establish context = () =>
        {
            parentSingleOptionQuestionId = Guid.Parse("9E96D4AB-DF91-4FC9-9585-23FA270B25D7");
            childCascadedComboboxId = Guid.Parse("C6CC807A-3E81-406C-A110-1044AE3FD89B");

            var questionnaireId = Guid.NewGuid();
            actorId = Guid.NewGuid();

            var questionnaire = Create.Questionnaire(actorId,
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
                        CascadeFromQuestionId = parentSingleOptionQuestionId,
                        Answers = new List<Answer>
                        {
                            new Answer { AnswerText = "child 1", AnswerValue = "1", PublicKey = Guid.NewGuid(), ParentValue = "3" }
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
            exception = Catch.Exception(() =>
                interview.AnswerSingleOptionQuestion(actorId, childCascadedComboboxId, new decimal[] { }, DateTime.Now, 1)
            );

        It should_throw_InterviewException = () =>
            exception.ShouldBeOfExactType<InterviewException>();

        It should_throw_exception_with_message_containting__not__valid__expression__ = () =>
            new[] { "answer", "parent value", "not found" }.ShouldEachConformTo(
                keyword => exception.Message.ToLower().Contains(keyword));

        static Exception exception;
        static Interview interview;
        static EventContext eventContext;
        static Guid parentSingleOptionQuestionId;
        static Guid childCascadedComboboxId;
        static Guid actorId;
    }
}