﻿using System;
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
    internal class when_creating_new_interview_with_cascading_options_question : InterviewTestsContext
    {
        Establish context = () =>
        {
            parentSingleOptionQuestionId = Guid.Parse("11111111111111111111111111111111");
            childCascadedComboboxId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            grandChildCascadedComboboxId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            actorId = Guid.Parse("366404A9-374E-4DCC-9B56-1F15103DE880");
            questionnaireId = Guid.Parse("3B7145CD-A235-44D0-917C-7B34A1017AEC");

            questionnaire = Create.Questionnaire(actorId,
                CreateQuestionnaireDocumentWithOneChapter(new MultyOptionsQuestion
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
                            new Answer { AnswerText = "child 1", AnswerValue = "1", PublicKey = Guid.NewGuid(), ParentValue = "1" },
                            new Answer { AnswerText = "child 2", AnswerValue = "2", PublicKey = Guid.NewGuid(), ParentValue = "2" }
                        }
                    },
                    new SingleQuestion
                    {
                        PublicKey = grandChildCascadedComboboxId,
                        QuestionType = QuestionType.SingleOption,
                        CascadeFromQuestionId = childCascadedComboboxId,
                        Answers = new List<Answer>
                        {
                            new Answer { AnswerText = "grand child", AnswerValue = "1", PublicKey = Guid.NewGuid(), ParentValue = "1" }
                        }
                    }
                    ));

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire.GetQuestionnaire());

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);

            eventContext = new EventContext();
        };

        Because of = () => new Interview(Guid.NewGuid(), actorId, questionnaireId, new Dictionary<Guid, object>(), DateTime.Now);

        It should_disable_cascading_questions = () => 
            eventContext.ShouldContainEvent<QuestionsDisabled>(x => x.Questions.Any(q => q.Id == childCascadedComboboxId));

        It should_disable_secod_level_of_questions_in_cascade = () =>
            eventContext.ShouldContainEvent<QuestionsDisabled>(x => x.Questions.Any(q => q.Id == grandChildCascadedComboboxId));

        static Guid parentSingleOptionQuestionId;
        static Guid childCascadedComboboxId;
        static Guid grandChildCascadedComboboxId;
        static Guid actorId;
        static Questionnaire questionnaire;
        static EventContext eventContext;
        static Guid questionnaireId;
    }
}

