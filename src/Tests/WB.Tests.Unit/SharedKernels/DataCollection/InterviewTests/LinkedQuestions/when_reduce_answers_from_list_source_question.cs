using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests.LinkedQuestions
{
    internal class when_reduce_answers_from_list_source_question : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
            rosterId = Guid.Parse("21111111111111111111111111111111");
            rosterTriggerQuestionId = Guid.Parse("22222222222222222222222222222222");

            rosterQuestionId = Guid.Parse("92222222222222222222222222222222");
            linkedQuestionId = Guid.Parse("33222222222222222222222222222222");

            QuestionnaireDocument questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(id: questionnaireId,
                children: new IComposite[]
                {
                    Create.Entity.MultipleOptionsQuestion(rosterTriggerQuestionId, answers: new [] { 2, 22, 222 }),

                    Create.Entity.Roster(rosterId: rosterId, variable: "ros", rosterSizeSourceType: RosterSizeSourceType.Question, 
                        rosterSizeQuestionId: rosterTriggerQuestionId,
                        children: new IComposite[]
                        {
                            Create.Entity.TextQuestion(questionId: rosterQuestionId, variable:"txt", enablementCondition:"@rowindex==1")
                        }),

                    Create.Entity.SingleQuestion(id: linkedQuestionId, linkedToRosterId: rosterId, variable:"link")
                });

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId,
                new PlainQuestionnaire(questionnaire, 1));

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);

            interview.AnswerMultipleOptionsQuestion(userId, rosterTriggerQuestionId, new decimal[] { }, DateTime.Now, new[] { 2 });
            interview.AnswerMultipleOptionsQuestion(userId, rosterTriggerQuestionId, new decimal[] {}, DateTime.Now, new[] { 2, 22 });
            interview.AnswerMultipleOptionsQuestion(userId, rosterTriggerQuestionId, new decimal[] { }, DateTime.Now, new[] { 2, 22, 222 });

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
           interview.AnswerMultipleOptionsQuestion(userId, rosterTriggerQuestionId, new decimal[] {}, DateTime.Now, new[] { 22, 222 });

        It should_raise_LinkedOptionsChanged_event_with_empty_option_list_for_linked_question = () =>
            eventContext.ShouldContainEvent<LinkedOptionsChanged>(@event
                => @event.ChangedLinkedQuestions.Count(q => q.QuestionId.Id == linkedQuestionId && q.Options.Length == 2) == 1);


        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid rosterTriggerQuestionId;
        private static Guid rosterQuestionId;
        private static Guid linkedQuestionId;
        private static Guid rosterId;
    }
}