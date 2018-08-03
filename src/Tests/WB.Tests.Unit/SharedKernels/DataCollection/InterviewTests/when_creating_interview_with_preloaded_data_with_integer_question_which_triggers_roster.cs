using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Preloading;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_creating_interview_with_preloaded_data_with_integer_question_which_triggers_roster : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaireId = Guid.Parse("22220000000000000000000000000000");
            userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            supervisorId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            prefilledQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            rosterGroupId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCD");
            prefilledQuestionAnswer = 2;
            preloadedDataDto = new PreloadedDataDto(new[]
            {
                new PreloadedLevelDto(new decimal[0], new Dictionary<Guid, AbstractAnswer>
                {
                    {prefilledQuestionId, NumericIntegerAnswer.FromInt(prefilledQuestionAnswer)}
                }),
            });
            answersTime = new DateTime(2013, 09, 01);

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.NumericIntegerQuestion(id: prefilledQuestionId),

                Create.Entity.Roster(rosterGroupId, rosterSizeQuestionId: prefilledQuestionId),
            }));

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            eventContext = new EventContext();

            interview = Create.AggregateRoot.Interview(questionnaireRepository: questionnaireRepository);
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

        [NUnit.Framework.Test] public void should_raise_NumericIntegerQuestionAnswered_event () =>
            eventContext.ShouldContainEvent<NumericIntegerQuestionAnswered>(@event
                => @event.Answer == prefilledQuestionAnswer && @event.QuestionId == prefilledQuestionId);

        [NUnit.Framework.Test] public void should_raise_RosterInstancesAdded_event () =>
            eventContext.ShouldContainEvent<RosterInstancesAdded>(@event
                => @event.Instances.All(i => i.GroupId == rosterGroupId));


        private static EventContext eventContext;
        private static Guid userId;
        private static Guid questionnaireId;
        private static PreloadedDataDto preloadedDataDto;
        private static DateTime answersTime;
        private static Guid supervisorId;
        private static Guid prefilledQuestionId;
        private static Guid rosterGroupId;
        private static int prefilledQuestionAnswer;
        private static Interview interview;
    }
}
