using System;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    [NUnit.Framework.Ignore("KP-6150")]
    internal class when_remove_answer_from_link_source_question_of_multi_option_question : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
            rosterId = Guid.Parse("21111111111111111111111111111111");
            sourceOfLinkQuestionId = Guid.Parse("22222222222222222222222222222222");
            linkedQuestionId = Guid.Parse("33222222222222222222222222222222");

            QuestionnaireDocument questionnaire = Create.Entity.QuestionnaireDocument(id: questionnaireId,
                children: new IComposite[]
                {
                    Create.Entity.Roster(rosterId: rosterId, variable: "ros", fixedTitles: new[] {"1", "2"},
                        children: new IComposite[]
                        {
                            Create.Entity.TextQuestion(questionId: sourceOfLinkQuestionId)
                        }),
                    Create.Entity.MultyOptionsQuestion(id: linkedQuestionId, linkedToQuestionId: sourceOfLinkQuestionId)
                });

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, Create.Entity.PlainQuestionnaire(questionnaire, 1));

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);
            interview.AnswerTextQuestion(userId, sourceOfLinkQuestionId, new decimal[] { 0 },
                DateTime.Now, "a");
            interview.AnswerTextQuestion(userId, sourceOfLinkQuestionId, new decimal[] { 1 },
             DateTime.Now, "b");
            interview.AnswerMultipleOptionsLinkedQuestion(userId, linkedQuestionId, RosterVector.Empty, DateTime.Now,
                new RosterVector[] {new decimal[] {0}, new decimal[] {1}});
            eventContext = new EventContext();
            BecauseOf();
        }

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            eventContext.Dispose();
            eventContext = null;
        }

        public void BecauseOf() =>
           interview.RemoveAnswer(sourceOfLinkQuestionId, new decimal[] { 0 }, userId, DateTime.Now);

        [NUnit.Framework.Test] public void should_raise_AnswerRemoved_event_for_first_row () =>
            eventContext.ShouldContainEvent<AnswerRemoved>(@event
                => @event.QuestionId == sourceOfLinkQuestionId && @event.RosterVector.SequenceEqual(new decimal[] { 0 }));

        [NUnit.Framework.Test] public void should_not_raise_AnswersRemoved_event_for_answered_linked_Question () =>
            eventContext.ShouldNotContainEvent<AnswersRemoved>(@event
                => @event.Questions.Count(q => q.Id == linkedQuestionId && !q.RosterVector.Any()) == 1);

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid sourceOfLinkQuestionId;
        private static Guid linkedQuestionId;
        private static Guid rosterId;
    }
}
