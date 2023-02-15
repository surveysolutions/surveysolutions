using System;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Tests.Integration.InterviewTests.LinkedQuestions
{
    internal class when_answering_linked_single_option_question_which_links_to_roster_and_roster_has_multi_question_as_source : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFF1111111111");
            var questionnaireId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDD0000000000");

            var triggerQuestionId = Guid.Parse("11111111111111111111111111111111");
            var questionId        = Guid.Parse("22222222222222222222222222222222");
            linkedToRosterId      = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            rosterId              = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(id: questionnaireId, children: new IComposite[]
            {
                Abc.Create.Entity.MultyOptionsQuestion(id: triggerQuestionId, variable: "multi_trigger", options: new Answer[]
                {
                    Abc.Create.Entity.Option(1, "1"),
                    Abc.Create.Entity.Option(2, "2"),
                    Abc.Create.Entity.Option(3, "3")
                }),
                Abc.Create.Entity.MultiRoster(rosterId: rosterId, rosterSizeQuestionId: triggerQuestionId, variable: "roster_var", children: new IComposite[]
                {
                    Abc.Create.Entity.TextQuestion(questionId: questionId, variable: "text")
                }),
                Abc.Create.Entity.SingleQuestion(id: linkedToRosterId, variable: "single", linkedToRosterId: rosterId)
            });
            appDomainContext = AppDomainContext.Create();

            interview = SetupInterview(appDomainContext.AssemblyLoadContext, questionnaireDocument);

            interview.AnswerMultipleOptionsQuestion(userId, triggerQuestionId, RosterVector.Empty, DateTime.Now, new [] { 2 });
            interview.AnswerMultipleOptionsQuestion(userId, triggerQuestionId, RosterVector.Empty, DateTime.Now, new [] { 2, 3 });
            interview.AnswerMultipleOptionsQuestion(userId, triggerQuestionId, RosterVector.Empty, DateTime.Now, new [] { 2, 3, 1 });
            
            eventContext = new EventContext();

            BecauseOf();
        }

        public void BecauseOf() =>
            interview.AnswerSingleOptionLinkedQuestion(userId, linkedToRosterId, RosterVector.Empty, DateTime.Now, new decimal[] { 1 });

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            eventContext.Dispose();
            eventContext = null;
            appDomainContext.Dispose();
        }

        [NUnit.Framework.Test] public void should_raise_SingleOptionLinkedQuestionAnswered_event () =>
            eventContext.ShouldContainEvent<SingleOptionLinkedQuestionAnswered>();

        [NUnit.Framework.Test] public void should_contains_options_for_single_linked_question_in_original_order () 
        {
            var linkedToRosterQuestion = interview.GetLinkedSingleOptionQuestion(Abc.Create.Identity(linkedToRosterId));
            linkedToRosterQuestion.Options.Should().BeEquivalentTo(
                new[]{ Abc.Create.RosterVector(1), 
                    Abc.Create.RosterVector(2), 
                    Abc.Create.RosterVector(3) });
        }

        static AppDomainContext appDomainContext;
        static EventContext eventContext;
        static Interview interview;
        static Guid userId;
        static Guid linkedToRosterId;
        static Guid rosterId;
    }
}
