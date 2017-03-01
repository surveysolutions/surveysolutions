using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using It = Machine.Specifications.It;

namespace WB.Tests.Integration.InterviewTests.LinkedQuestions
{
    internal class when_answering_linked_single_option_question_which_links_to_roster_and_roster_has_multi_question_as_source : InterviewTestsContext
    {
        Establish context = () =>
        {
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFF1111111111");
            var questionnaireId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDD0000000000");

            var triggerQuestionId = Guid.Parse("11111111111111111111111111111111");
            var questionId        = Guid.Parse("22222222222222222222222222222222");
            linkedToRosterId      = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            rosterId              = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(id: questionnaireId, children: new IComposite[]
            {
                Create.MultyOptionsQuestion(id: triggerQuestionId, variable: "multi_trigger", options: new Answer[]
                {
                    new Answer() { AnswerCode = 1, AnswerText = "1" }, 
                    new Answer() { AnswerCode = 2, AnswerText = "2" }, 
                    new Answer() { AnswerCode = 3, AnswerText = "3" }, 
                }),
                Abc.Create.Entity.Roster(rosterId: rosterId, rosterSizeSourceType: RosterSizeSourceType.Question,
                    rosterSizeQuestionId: triggerQuestionId, rosterTitleQuestionId: triggerQuestionId, variable: "roster_var",
                    children: new IComposite[]
                    {
                        Abc.Create.Entity.TextQuestion(questionId: questionId, variable: "text")
                    }),
                Create.SingleQuestion(id: linkedToRosterId, variable: "single", linkedToRosterId: rosterId)
            });

            interview = SetupInterview(questionnaireDocument);

            interview.AnswerMultipleOptionsQuestion(userId, triggerQuestionId, RosterVector.Empty, DateTime.Now, new [] { 2 });
            interview.AnswerMultipleOptionsQuestion(userId, triggerQuestionId, RosterVector.Empty, DateTime.Now, new [] { 2, 3 });
            interview.AnswerMultipleOptionsQuestion(userId, triggerQuestionId, RosterVector.Empty, DateTime.Now, new [] { 2, 3, 1 });
            
            eventContext = new EventContext();
        };

        Because of = () =>
            interview.AnswerSingleOptionLinkedQuestion(userId, linkedToRosterId, RosterVector.Empty, DateTime.Now, new decimal[] { 1 });

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_SingleOptionLinkedQuestionAnswered_event = () =>
            eventContext.ShouldContainEvent<SingleOptionLinkedQuestionAnswered>();

        It should_contains_options_for_single_linked_question_in_original_order = () =>
        {
            var linkedToRosterQuestion = interview.GetLinkedSingleOptionQuestion(Create.Identity(linkedToRosterId));
            linkedToRosterQuestion.Options.ShouldEqual(new List<RosterVector>()
            {
                new RosterVector(new decimal[] { 1m }),
                new RosterVector(new decimal[] { 2m }),
                new RosterVector(new decimal[] { 3m }),
            });
        };


        static EventContext eventContext;
        static Interview interview;
        static Guid userId;
        static Guid linkedToRosterId;
        static Guid rosterId;
    }
}