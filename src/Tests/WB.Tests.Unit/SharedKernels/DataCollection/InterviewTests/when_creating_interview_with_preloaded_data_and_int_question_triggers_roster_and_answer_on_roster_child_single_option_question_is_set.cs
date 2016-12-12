using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Preloading;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_creating_interview_with_preloaded_data_and_int_question_triggers_roster_and_answer_on_roster_child_single_option_question_is_set : InterviewTestsContext
    {
        Establish context = () =>
        {
            questionnaireId = Guid.Parse("22220000000000000000000000000000");
            userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            supervisorId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            prefilledIntQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            rosterGroupId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            prefilledIntQuestionAnswer = 1;

            prefilledSingleOptionQuestionId = Guid.Parse("22222222222222222222222222222222");
            prefilledSingleOptionQuestionAnswer = 3;
            preloadedDataDto = new PreloadedDataDto(
                new[]
                {
                    new PreloadedLevelDto(new decimal[0],
                        new Dictionary<Guid, AbstractAnswer> { { prefilledIntQuestionId, NumericIntegerAnswer.FromInt(prefilledIntQuestionAnswer) } }),
                    new PreloadedLevelDto(new decimal[] {0},
                        new Dictionary<Guid, AbstractAnswer> {{prefilledSingleOptionQuestionId, CategoricalFixedSingleOptionAnswer.FromDecimal(prefilledSingleOptionQuestionAnswer) }})
                });

            answersTime = new DateTime(2013, 09, 01);

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.NumericIntegerQuestion(id: prefilledIntQuestionId),

                Create.Entity.Roster(rosterId: rosterGroupId, rosterSizeQuestionId: prefilledIntQuestionId, children: new IComposite[]
                {
                    Create.Entity.SingleOptionQuestion(questionId: prefilledSingleOptionQuestionId)
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

        It should_raise_valid_SingleOptionQuestionAnswered_event = () =>
            eventContext.ShouldContainEvent<SingleOptionQuestionAnswered>(@event
                => @event.SelectedValue == prefilledSingleOptionQuestionAnswer && @event.QuestionId == prefilledSingleOptionQuestionId);

        private static EventContext eventContext;
        private static Guid userId;
        private static Guid questionnaireId;
        private static PreloadedDataDto preloadedDataDto;
        private static DateTime answersTime;
        private static Guid supervisorId;
        private static Guid rosterGroupId;
        private static Guid prefilledIntQuestionId;
        private static int prefilledIntQuestionAnswer;
        private static Guid prefilledSingleOptionQuestionId;
        private static decimal prefilledSingleOptionQuestionAnswer;
        private static Interview interview;
    }
}