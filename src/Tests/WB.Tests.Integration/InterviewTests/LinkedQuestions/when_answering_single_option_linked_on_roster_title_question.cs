using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Tests.Integration.InterviewTests.LinkedQuestions
{
    internal class when_answering_single_option_linked_on_roster_title_question : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDD0000000000");

            var triggerQuestionId = Guid.NewGuid();
            var titleQuestionId = Guid.NewGuid();
            var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(id: questionnaireId, children: new IComposite[]
            {
                Create.SingleQuestion(id: linkedToQuestionId, linkedToQuestionId: linkedToQuestionId,
                    variable: "link_single"),
                Integration.Create.NumericIntegerQuestion(id: triggerQuestionId, variable: "num_trigger"),
                Create.Roster(id: rosterId, rosterSizeSourceType: RosterSizeSourceType.Question,
                    rosterSizeQuestionId: triggerQuestionId, rosterTitleQuestionId: titleQuestionId, variable: "ros1",
                    children: new IComposite[]
                    {
                        Create.NumericRealQuestion(id: titleQuestionId, variable: "link_source")
                    }),
                Create.TextQuestion(id: disabledQuestionsId, variable: "txt_disabled", enablementCondition:"IsAnswered(link_single)")
            });

            interview = SetupInterview(questionnaireDocument: questionnaireDocument);
            interview.Apply(Create.Event.RosterInstancesAdded(rosterId));
            interview.Apply(new LinkedOptionsChanged(new[]
            {
                new ChangedLinkedOptions(new Identity(linkedToQuestionId, new decimal[0]), new RosterVector[]
                {
                    answer
                })
            }));
            eventContext = new EventContext();
        };

        Because of = () => interview.AnswerSingleOptionLinkedQuestion(userId: userId, questionId: linkedToQuestionId,
                 answerTime: DateTime.Now, rosterVector: new decimal[0], selectedRosterVector: answer);

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
        private static Guid userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFF1111111111");
        private static Guid linkedToQuestionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid rosterId = Guid.Parse("22222222222222222222222222222222");
        private static decimal[] answer = { 0 };
        private static Guid disabledQuestionsId = Guid.NewGuid();
    }
}