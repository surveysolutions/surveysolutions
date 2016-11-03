using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Preloading;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_creating_interview_with_preloaded_data : InterviewTestsContext
    {
        Establish context = () =>
        {
            questionnaireId = Guid.Parse("22220000000000000000000000000000");
            userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            supervisorId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            prefilledQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var fixedRosterGroup = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCD");
            prefilledQuestionAnswer = "answer";
            preloadedDataDto = new PreloadedDataDto(new []
            {
                new PreloadedLevelDto(new decimal[0], new Dictionary<Guid, AbstractAnswer>
                {
                    { prefilledQuestionId, TextAnswer.FromString(prefilledQuestionAnswer) }
                }),
            });
            answersTime = new DateTime(2013, 09, 01);

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.TextQuestion(questionId: prefilledQuestionId),
                Create.Entity.FixedRoster(rosterId: fixedRosterGroup, fixedTitles: new string[]{}),
            }));

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            eventContext = new EventContext();

            interview = Create.AggregateRoot.Interview(questionnaireRepository: questionnaireRepository);
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

        It should_raise_valid_TextQuestionAnswered_event = () =>
            eventContext.ShouldContainEvent<TextQuestionAnswered>(@event
                => @event.Answer == prefilledQuestionAnswer && @event.QuestionId == prefilledQuestionId);


        private static EventContext eventContext;
        private static Guid userId;
        private static Guid questionnaireId;
        private static PreloadedDataDto preloadedDataDto;
        private static DateTime answersTime;
        private static Guid supervisorId;
        private static Guid prefilledQuestionId;
        private static string prefilledQuestionAnswer;
        private static Interview interview;
    }
}
