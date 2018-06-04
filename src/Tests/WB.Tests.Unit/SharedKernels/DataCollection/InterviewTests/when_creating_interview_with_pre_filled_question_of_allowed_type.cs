using System;
using System.Collections.Generic;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_creating_interview_with_pre_filled_question_of_allowed_type : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaireId = Guid.Parse("22220000000000000000000000000000");
            userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            supervisorId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            prefilledQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            answersToFeaturedQuestions = new List<InterviewAnswer>();
            prefilledQuestionAnswer = "answer";
            answersToFeaturedQuestions.Add(Create.Entity.InterviewAnswer(Create.Identity(prefilledQuestionId), TextAnswer.FromString(prefilledQuestionAnswer)));
            answersTime = new DateTime(2013, 09, 01);

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.TextQuestion(questionId: prefilledQuestionId),
            }));

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            eventContext = new EventContext();

            command = Create.Command.CreateInterview(questionnaireId, 1, supervisorId,
                answersToFeaturedQuestions, userId: userId);
            interview = Create.AggregateRoot.Interview(questionnaireRepository: questionnaireRepository);
            BecauseOf();
        }

        public void BecauseOf() =>
            interview.CreateInterview(command);

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            eventContext.Dispose();
            eventContext = null;
        }

        [NUnit.Framework.Test] public void should_raise_InterviewCreated_event () =>
            eventContext.ShouldContainEvent<InterviewCreated>();

        [NUnit.Framework.Test] public void should_raise_valid_TextQuestionAnswered_event () =>
            eventContext.ShouldContainEvent<TextQuestionAnswered>(@event
                => @event.Answer == prefilledQuestionAnswer && @event.QuestionId == prefilledQuestionId);
        

        private static EventContext eventContext;
        private static Guid userId;
        private static Guid questionnaireId;
        private static List<InterviewAnswer> answersToFeaturedQuestions;
        private static DateTime answersTime;
        private static Guid supervisorId;
        private static Guid prefilledQuestionId;
        private static string prefilledQuestionAnswer;
        private static Interview interview;
        private static CreateInterview command;
    }
}
