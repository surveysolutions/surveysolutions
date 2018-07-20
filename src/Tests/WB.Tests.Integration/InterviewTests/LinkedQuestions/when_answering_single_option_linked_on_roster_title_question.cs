using System;
using System.Linq;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Tests.Integration.InterviewTests.LinkedQuestions
{
    internal class when_answering_single_option_linked_on_roster_title_question : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var questionnaireId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDD0000000000");
            
            var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(id: questionnaireId, children: new IComposite[]
            {
                Abc.Create.Entity.SingleQuestion(id: linkedToQuestionId, linkedToQuestionId: titleQuestionId, variable: "link_single"),
                Abc.Create.Entity.NumericIntegerQuestion(id: triggerQuestionId, variable: "num_trigger"),
                Abc.Create.Entity.NumericRoster(rosterId: rosterId, rosterSizeQuestionId: triggerQuestionId, rosterTitleQuestionId: titleQuestionId, variable: "ros1", children: new IComposite[]
                {
                    Abc.Create.Entity.NumericRealQuestion(id: titleQuestionId, variable: "link_source")
                }),
                Abc.Create.Entity.TextQuestion(questionId: disabledQuestionsId, variable: "txt_disabled", enablementCondition: "IsAnswered(link_single)")
            });

            interview = SetupInterview(questionnaireDocument);
            interview.AnswerNumericIntegerQuestion(userId, triggerQuestionId, RosterVector.Empty, DateTime.Now, 1);
            interview.AnswerNumericRealQuestion(userId, titleQuestionId, Abc.Create.RosterVector(0), DateTime.Now, 18.5);
            eventContext = new EventContext();

            BecauseOf();
        }

        public void BecauseOf() =>
            interview.AnswerSingleOptionLinkedQuestion(userId: userId, questionId: linkedToQuestionId,
                originDate: DateTimeOffset.Now, rosterVector: RosterVector.Empty, selectedRosterVector: Abc.Create.RosterVector(0));

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            eventContext.Dispose();
            eventContext = null;
        }

        [NUnit.Framework.Test] public void should_raise_SingleOptionLinkedQuestionAnswered_event () =>
            eventContext.ShouldContainEvent<SingleOptionLinkedQuestionAnswered>();

        [NUnit.Framework.Test] public void should_raise_QuestionsEnabled_event_for_question_conditionally_dependant_on_lined_question () =>
             eventContext.ShouldContainEvent<QuestionsEnabled>(q=>q.Questions.Any(x=>x.Id== disabledQuestionsId));

        private static EventContext eventContext;
        private static Interview interview;
        private static readonly Guid userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFF1111111111");
        private static readonly Guid linkedToQuestionId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid rosterId = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid disabledQuestionsId = Guid.Parse("33333333333333333333333333333333");
        private static readonly Guid triggerQuestionId = Guid.Parse("44444444444444444444444444444444");
        private static readonly Guid titleQuestionId = Guid.Parse("55555555555555555555555555555555");
    }
}
