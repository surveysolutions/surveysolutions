using System;
using System.Collections.Generic;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Preloading;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_creating_interview_with_preloaded_data_and_int_question_triggers_roster_and_answer_on_roster_child_text_question_is_set : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaireId = Guid.Parse("22220000000000000000000000000000");
            userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            supervisorId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            prefilledIntQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            rosterGroupId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            prefilledIntQuestionAnswer = 1;

            prefilledTextQuestionId = Guid.Parse("22222222222222222222222222222222");
            prefilledTextQuestionAnswer = "a";
            preloadedDataDto = new PreloadedDataDto(
                new[]
                {
                    new PreloadedLevelDto(new decimal[0],
                        new Dictionary<Guid, AbstractAnswer> { { prefilledIntQuestionId, NumericIntegerAnswer.FromInt(prefilledIntQuestionAnswer) } }),
                    new PreloadedLevelDto(new decimal[] {0},
                        new Dictionary<Guid, AbstractAnswer> {{prefilledTextQuestionId, TextAnswer.FromString(prefilledTextQuestionAnswer) }})
                });

            answersTime = new DateTime(2013, 09, 01);

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.NumericIntegerQuestion(id: prefilledIntQuestionId),

                Create.Entity.Roster(rosterId: rosterGroupId, rosterSizeQuestionId: prefilledIntQuestionId, children: new IComposite[]
                {
                    Create.Entity.TextQuestion(questionId: prefilledTextQuestionId)
                }),
            }));

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            interview = Create.AggregateRoot.Interview(questionnaireRepository: questionnaireRepository);

            eventContext = new EventContext();
            BecauseOf();
        }

        public void BecauseOf() =>
                interview.CreateInterview(Create.Command.CreateInterview(interview.EventSourceId, userId, questionnaireId, 1, preloadedDataDto.Answers, supervisorId, null, null, null));

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            eventContext.Dispose();
            eventContext = null;
        }

        [NUnit.Framework.Test] public void should_raise_InterviewCreated_event () =>
                eventContext.ShouldContainEvent<InterviewCreated>();

        [NUnit.Framework.Test] public void should_raise_valid_TextQuestionAnswered_event () =>
            eventContext.ShouldContainEvent<TextQuestionAnswered>(@event
                => @event.Answer == prefilledTextQuestionAnswer && @event.QuestionId == prefilledTextQuestionId);

        private static EventContext eventContext;
        private static Guid userId;
        private static Guid questionnaireId;
        private static PreloadedDataDto preloadedDataDto;
        private static DateTime answersTime;
        private static Guid supervisorId;
        private static Guid rosterGroupId;
        private static Guid prefilledIntQuestionId;
        private static int prefilledIntQuestionAnswer;
        private static Guid prefilledTextQuestionId;
        private static string prefilledTextQuestionAnswer;
        private static Interview interview;
    }
}
