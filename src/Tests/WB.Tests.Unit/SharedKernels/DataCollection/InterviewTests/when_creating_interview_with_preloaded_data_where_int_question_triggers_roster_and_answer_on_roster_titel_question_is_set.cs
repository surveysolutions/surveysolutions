﻿using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Preloading;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_creating_interview_with_preloaded_data_where_int_question_triggers_roster_and_answer_on_roster_titel_question_is_set : InterviewTestsContext
    {
        Establish context = () =>
        {
            questionnaireId = Guid.Parse("22220000000000000000000000000000");
            userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            supervisorId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            prefilledIntQuestion = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            rosterGroupId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            prefilledIntQuestionAnswer = 1;

            rosterTitleQuestionId = Guid.Parse("22222222222222222222222222222222");
            rosterTitleQuestionAnswer = "a";
            preloadedDataDto = new PreloadedDataDto(new[]
            {
                new PreloadedLevelDto(new decimal[0],
                    new Dictionary<Guid, AbstractAnswer> { { prefilledIntQuestion, NumericIntegerAnswer.FromInt(prefilledIntQuestionAnswer) } }),
                new PreloadedLevelDto(new decimal[] { 0 },
                    new Dictionary<Guid, AbstractAnswer> { { rosterTitleQuestionId, TextAnswer.FromString(rosterTitleQuestionAnswer) } })
            });

            answersTime = new DateTime(2013, 09, 01);

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.NumericIntegerQuestion(id: prefilledIntQuestion),

                Create.Entity.Roster(rosterId: rosterGroupId, rosterSizeQuestionId: prefilledIntQuestion, rosterTitleQuestionId: rosterTitleQuestionId, children: new IComposite[]
                {
                    Create.Entity.TextQuestion(questionId: rosterTitleQuestionId)
                }),
            }));

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            interview = Create.AggregateRoot.Interview(questionnaireRepository: questionnaireRepository);

            eventContext = new EventContext();
        };

        Because of = () =>
            interview.CreateInterviewWithPreloadedData(new CreateInterviewWithPreloadedData(interview.EventSourceId, userId, questionnaireId, 1, preloadedDataDto, answersTime, supervisorId, null));

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_InterviewFromPreloadedDataCreated_event = () =>
            eventContext.ShouldContainEvent<InterviewFromPreloadedDataCreated>();

        It should_raise_valid_NumericIntegerQuestionAnswered_event = () =>
            eventContext.ShouldContainEvent<NumericIntegerQuestionAnswered>(@event
                => @event.Answer == prefilledIntQuestionAnswer && @event.QuestionId == prefilledIntQuestion);

        It should_raise_RosterInstancesTitleChanged_event = () =>
           eventContext.ShouldContainEvent<RosterInstancesTitleChanged>(@event
               => @event.ChangedInstances[0].Title == rosterTitleQuestionAnswer && @event.ChangedInstances[0].RosterInstance.GroupId == rosterGroupId);


        private static EventContext eventContext;
        private static Guid userId;
        private static Guid questionnaireId;
        private static PreloadedDataDto preloadedDataDto;
        private static DateTime answersTime;
        private static Guid supervisorId;
        private static Guid rosterGroupId;
        private static Guid prefilledIntQuestion;
        private static int prefilledIntQuestionAnswer;
        private static Guid rosterTitleQuestionId;
        private static string rosterTitleQuestionAnswer;
        private static Interview interview;
    }
}
