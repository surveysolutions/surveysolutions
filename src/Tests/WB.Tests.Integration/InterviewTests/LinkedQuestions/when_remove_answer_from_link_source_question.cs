using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Tests.Integration.InterviewTests.LinkedQuestions
{
    internal class when_remove_answer_from_link_source_question : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
            rosterId = Guid.Parse("21111111111111111111111111111111");
            sourceOfLinkQuestionId = Guid.Parse("22222222222222222222222222222222");
            linkedQuestionId = Guid.Parse("33222222222222222222222222222222");

            QuestionnaireDocument questionnaire = Create.QuestionnaireDocumentWithOneChapter(id: questionnaireId,
                children: new IComposite[]
                {
                    Create.Roster(id: rosterId, variable: "ros", obsoleteFixedTitles: new[] {"1", "2"},
                        children: new IComposite[]
                        {
                            Create.TextQuestion(id: sourceOfLinkQuestionId, variable:"txt")
                        }),
                    Create.SingleQuestion(id: linkedQuestionId, linkedToQuestionId: sourceOfLinkQuestionId, variable:"link")
                });

            interview = SetupInterview(questionnaire);
            interview.AnswerTextQuestion(userId, sourceOfLinkQuestionId, new decimal[] {0}, DateTime.Now, "a");
            interview.AnswerSingleOptionLinkedQuestion(userId, linkedQuestionId, new decimal[0], DateTime.Now, new decimal[] {0});
            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
           interview.RemoveAnswer(sourceOfLinkQuestionId, new decimal[] { 0 }, userId, DateTime.Now);

        private It should_raise_AnswerRemoved_event_for_first_row = () =>
            eventContext.GetSingleEvent<AnswersRemoved>()
                .Questions.ShouldContain(Create.Identity(sourceOfLinkQuestionId, Create.RosterVector(0)));

        It should_raise_AnswersRemoved_event_for_answered_linked_Question = () =>
            eventContext.ShouldContainEvent<AnswersRemoved>(@event
                => @event.Questions.Count(q => q.Id == linkedQuestionId && !q.RosterVector.Any())==1);

        It should_raise_LinkedOptionsChanged_event_with_empty_option_list_for_linked_question = () =>
            eventContext.ShouldContainEvent<LinkedOptionsChanged>(@event
                => @event.ChangedLinkedQuestions.Count(q => q.QuestionId.Id == linkedQuestionId && !q.Options.Any()) == 1);


        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid sourceOfLinkQuestionId;
        private static Guid linkedQuestionId;
        private static Guid rosterId;
    }
}