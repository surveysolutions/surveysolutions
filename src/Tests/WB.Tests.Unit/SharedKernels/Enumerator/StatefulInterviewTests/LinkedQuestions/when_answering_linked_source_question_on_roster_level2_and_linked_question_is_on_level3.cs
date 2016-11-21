using System;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests.LinkedQuestions
{
    internal class when_answering_linked_source_question_on_roster_level2_and_linked_question_is_on_level3 : StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.NumericIntegerQuestion(id: rosterSizeQuestionId),
                Create.Entity.Roster(rosterId: roster1Id, rosterSizeQuestionId: rosterSizeQuestionId, children: new IComposite[]
                {
                    Create.Entity.MultipleOptionsQuestion(questionId: nestedRosterSizeQuestionId, textAnswers: new []{ Create.Entity.Option("1", "Multi 1"), Create.Entity.Option("2", "Multi 2") }  ),
                    Create.Entity.Roster(rosterId: roster2Id, rosterSizeQuestionId: nestedRosterSizeQuestionId, children: new IComposite[]
                    {
                        Create.Entity.TextQuestion(questionId: sourceOfLinkedQuestionId),
                        Create.Entity.NumericIntegerQuestion(roster3TriggerId),
                        Create.Entity.Roster(roster3Id,
                            rosterSizeSourceType: RosterSizeSourceType.Question,
                            rosterSizeQuestionId: roster3TriggerId,
                            children: new IComposite[]
                            {
                                Create.Entity.SingleQuestion(id: linkedSingleQuestionId, linkedToQuestionId: sourceOfLinkedQuestionId)
                            })
                    })
                })
            });
            var plainQuestionnaire = new PlainQuestionnaire(questionnaireDocument, 0);

            interview = Create.AggregateRoot.StatefulInterview(questionnaire: plainQuestionnaire);
            interview.AnswerNumericIntegerQuestion(interviewerId, rosterSizeQuestionId, RosterVector.Empty, DateTime.UtcNow, 2);
            interview.AnswerMultipleOptionsQuestion(interviewerId, nestedRosterSizeQuestionId, Create.Entity.RosterVector(0), DateTime.UtcNow, new[] { 1, 2 });
            interview.AnswerMultipleOptionsQuestion(interviewerId, nestedRosterSizeQuestionId, Create.Entity.RosterVector(1), DateTime.UtcNow, new[] { 1, 2 });
            interview.AnswerNumericIntegerQuestion(interviewerId, roster3TriggerId, Create.Entity.RosterVector(0, 1), DateTime.UtcNow, 2);
        };

        Because of = () =>
        {
            interview.AnswerTextQuestion(interviewerId, sourceOfLinkedQuestionId, Create.Entity.RosterVector(0, 1), DateTime.UtcNow, "answer 0.1");
            interview.AnswerTextQuestion(interviewerId, sourceOfLinkedQuestionId, Create.Entity.RosterVector(0, 2), DateTime.UtcNow, "answer 0.2");
        };

        It should_put__2__options_in_linked_single_question_from_roster__0_1_0__ = () =>
        {
            var identity = Create.Entity.Identity(linkedSingleQuestionId, Create.Entity.RosterVector(0, 1, 0));

            interview.GetLinkedSingleOptionQuestion(identity).Options.Count.ShouldEqual(2);

            interview.GetLinkedOptionTitle(identity, Create.Entity.RosterVector(0, 1)).ShouldEqual("answer 0.1");
            interview.GetLinkedOptionTitle(identity, Create.Entity.RosterVector(0, 2)).ShouldEqual("answer 0.2");
        };

        It should_put__2__options_in_linked_single_question_from_roster__0_1_1__ = () =>
        {
            var identity = Create.Entity.Identity(linkedSingleQuestionId, Create.Entity.RosterVector(0, 1, 1));

            interview.GetLinkedSingleOptionQuestion(identity).Options.Count.ShouldEqual(2);
            interview.GetLinkedOptionTitle(identity, Create.Entity.RosterVector(0, 1)).ShouldEqual("answer 0.1");
            interview.GetLinkedOptionTitle(identity, Create.Entity.RosterVector(0, 2)).ShouldEqual("answer 0.2");
        };

        static StatefulInterview interview;

        static readonly Guid roster1Id = Guid.Parse("11111111111111111111111111111111");
        static readonly Guid roster2Id = Guid.Parse("22222222222222222222222222222222");
        static readonly Guid roster3Id = Guid.Parse("33333333333333333333333333333333");

        static readonly Guid rosterSizeQuestionId = Guid.Parse("44444444444444444444444444444444");
        static readonly Guid nestedRosterSizeQuestionId = Guid.Parse("55555555555555555555555555555555");

        static readonly Guid linkedSingleQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

        static readonly Guid sourceOfLinkedQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        static readonly Guid interviewerId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
        static readonly Guid roster3TriggerId = Guid.Parse("3333333333333333333333333333333A");
    }
}