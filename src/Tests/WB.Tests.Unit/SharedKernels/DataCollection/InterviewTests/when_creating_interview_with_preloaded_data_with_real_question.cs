using System;
using System.Collections.Generic;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Preloading;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_creating_interview_with_preloaded_data_with_real_question : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaireId = Guid.Parse("22220000000000000000000000000000");
            userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            supervisorId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            prefilledQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var fixedRosterGroup = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCD");
            prefilledQuestionAnswer = 2;
            preloadedDataDto = new PreloadedDataDto(new[]
            {
                new PreloadedLevelDto(new decimal[0], new Dictionary<Guid, AbstractAnswer>
                {
                    { prefilledQuestionId, NumericRealAnswer.FromDouble(prefilledQuestionAnswer) }
                }),
            });
            answersTime = new DateTime(2013, 09, 01);

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.NumericQuestion(questionId: prefilledQuestionId),
                Create.Entity.FixedRoster(rosterId: fixedRosterGroup, obsoleteFixedTitles: new string[]{}),
            }));

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            eventContext = new EventContext();

            interview = Create.AggregateRoot.Interview(questionnaireRepository: questionnaireRepository);
            BecauseOf();
        }

        public void BecauseOf() =>
            interview.CreateInterview(Create.Command.CreateInterview(interview.EventSourceId, userId, questionnaireId, 1, preloadedDataDto.Answers, answersTime, supervisorId, null, null, null));

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            eventContext.Dispose();
            eventContext = null;
        }

        [NUnit.Framework.Test] public void should_raise_InterviewCreated_event () =>
            eventContext.ShouldContainEvent<InterviewCreated>();

        [NUnit.Framework.Test] public void should_raise_valid_TextQuestionAnswered_event () =>
            eventContext.ShouldContainEvent<NumericRealQuestionAnswered>(@event
                => @event.Answer == (decimal)prefilledQuestionAnswer && @event.QuestionId == prefilledQuestionId);


        private static EventContext eventContext;
        private static Guid userId;
        private static Guid questionnaireId;
        private static PreloadedDataDto preloadedDataDto;
        private static DateTime answersTime;
        private static Guid supervisorId;
        private static Guid prefilledQuestionId;
        private static int prefilledQuestionAnswer;
        private static Interview interview;
    }
}
