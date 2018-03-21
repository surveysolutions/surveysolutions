using System;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answering_propagating_question_with_2_and_previous_answer_was_4_and_there_were_answers_given_to_propagated_questions_with_indexes_1_and_2 : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            propagatingQuestionId = Guid.Parse("11111111111111111111111111111111");
            var propagatedGroupId = Guid.Parse("00000000000000003333333333333333");
            propagatedQuestionId = Guid.Parse("22222222222222222222222222222222");

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.NumericIntegerQuestion(id: propagatingQuestionId),

                Create.Entity.Roster(rosterId: propagatedGroupId, rosterSizeQuestionId: propagatingQuestionId, children: new IComposite[]
                {
                    Create.Entity.TextQuestion(questionId: propagatedQuestionId),
                }),
            }));

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);

            interview.AnswerNumericIntegerQuestion(userId, propagatingQuestionId, new decimal[] { }, DateTime.Now, 4);
            interview.AnswerTextQuestion(userId, propagatedQuestionId, new decimal[] { 1 }, DateTime.Now, "Answer for index 1");
            interview.AnswerTextQuestion(userId, propagatedQuestionId, new decimal[] { 2 }, DateTime.Now, "Answer for index 2");

            eventContext = new EventContext();
            BecauseOf();
        }

        public void BecauseOf() =>
            interview.AnswerNumericIntegerQuestion(userId, propagatingQuestionId, new decimal[] { }, DateTime.Now, 2);

        [NUnit.Framework.Test] public void should_not_raise_AnswersRemoved_event_with_QuestionId_equal_to_propagated_question_and_PropagationVector_equal_to_0 () =>
            eventContext.ShouldNotContainEvent<AnswersRemoved>(@event => @event.Questions.Any(question
                => question.Id == propagatedQuestionId
                && question.RosterVector.Identical(new decimal[] { 0 })));

        [NUnit.Framework.Test] public void should_not_raise_AnswersRemoved_event_with_QuestionId_equal_to_propagated_question_and_PropagationVector_equal_to_1 () =>
            eventContext.ShouldNotContainEvent<AnswersRemoved>(@event => @event.Questions.Any(question
                => question.Id == propagatedQuestionId
                && question.RosterVector.Identical(new decimal[] { 1 })));

        [NUnit.Framework.Test] public void should_raise_AnswersRemoved_event_with_QuestionId_equal_to_propagated_question_and_PropagationVector_equal_to_2 () =>
            eventContext.ShouldContainEvent<AnswersRemoved>(@event => @event.Questions.Any(question
                => question.Id == propagatedQuestionId
                && question.RosterVector.Identical(new decimal[] { 2 })));

        [NUnit.Framework.Test] public void should_not_raise_AnswersRemoved_event_with_QuestionId_equal_to_propagated_question_and_PropagationVector_equal_to_3 () =>
            eventContext.ShouldNotContainEvent<AnswersRemoved>(@event => @event.Questions.Any(question
                => question.Id == propagatedQuestionId
                && question.RosterVector.Identical(new decimal[] { 3 })));

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            eventContext.Dispose();
            eventContext = null;
        }

        private static EventContext eventContext;
        private static Guid propagatedQuestionId;
        private static Interview interview;
        private static Guid userId;
        private static Guid propagatingQuestionId;
    }
}
