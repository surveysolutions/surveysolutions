using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using It = Machine.Specifications.It;

namespace WB.Tests.Integration.InterviewTests.LinkedQuestions
{
    internal class when_answering_single_option_linked_on_roster_title_question : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDD0000000000");

            
            var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(id: questionnaireId, children: new IComposite[]
            {
                Create.SingleQuestion(id: linkedToQuestionId, linkedToQuestionId: titleQuestionId, variable: "link_single"),
                Create.NumericIntegerQuestion(id: triggerQuestionId, variable: "num_trigger"),
                Create.Roster(id: rosterId, rosterSizeSourceType: RosterSizeSourceType.Question,
                    rosterSizeQuestionId: triggerQuestionId, rosterTitleQuestionId: titleQuestionId, variable: "ros1",
                    children: new IComposite[]
                    {
                        Create.NumericRealQuestion(id: titleQuestionId, variable: "link_source")
                    }),
                Create.TextQuestion(id: disabledQuestionsId, variable: "txt_disabled", enablementCondition:"IsAnswered(link_single)")
            });

            interview = SetupInterview(questionnaireDocument);
            interview.AnswerNumericIntegerQuestion(userId, triggerQuestionId, RosterVector.Empty, DateTime.Now, 1);
            interview.AnswerNumericRealQuestion(userId, titleQuestionId, Create.RosterVector(0), DateTime.Now, 18.5m);
            eventContext = new EventContext();
        };

        Because of = () =>
            interview.AnswerSingleOptionLinkedQuestion(userId: userId, questionId: linkedToQuestionId,
                 answerTime: DateTime.Now, rosterVector: new decimal[0], selectedRosterVector: new [] { 0m });

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_SingleOptionLinkedQuestionAnswered_event = () =>
            eventContext.ShouldContainEvent<SingleOptionLinkedQuestionAnswered>();

        It should_raise_QuestionsEnabled_event_for_question_conditionally_dependant_on_lined_question = () =>
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