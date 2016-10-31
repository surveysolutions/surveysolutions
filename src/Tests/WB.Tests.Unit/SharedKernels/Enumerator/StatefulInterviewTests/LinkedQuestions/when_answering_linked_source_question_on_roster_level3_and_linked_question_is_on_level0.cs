using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;
using TextAnswer = WB.Core.SharedKernels.Enumerator.Entities.Interview.TextAnswer;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests.LinkedQuestions
{
    internal class when_answering_linked_source_question_on_roster_level3_and_linked_question_is_on_level0 : StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.NumericIntegerQuestion(id: rosterSizeQuestionId),
                Create.Entity.Roster(rosterId: roster1Id, rosterSizeQuestionId: rosterSizeQuestionId, children: new IComposite[]
                {
                    Create.Entity.MultipleOptionsQuestion(questionId: nestedRosterSizeQuestionId, answers: new [] {1m, 2m}),
                    Create.Entity.Roster(rosterId: roster2Id, rosterSizeQuestionId: nestedRosterSizeQuestionId, children: new IComposite[]
                    {
                        Create.Entity.FixedRoster(roster3Id, fixedRosterTitles: new [] { Create.Entity.FixedRosterTitle(100, "Title 1"), Create.Entity.FixedRosterTitle(200, "Title 2") }, children: new IComposite[]
                        {
                            Create.Entity.TextQuestion(questionId: sourceOfLinkedQuestionId)
                        })
                    })
                }),
                Create.Entity.SingleQuestion(id: linkedSingleQuestionId, linkedToQuestionId: sourceOfLinkedQuestionId),
                Create.Entity.MultyOptionsQuestion(id: linkedMultiQuestionId, linkedToQuestionId: sourceOfLinkedQuestionId)
            });
            var plainQuestionnaire = new PlainQuestionnaire(questionnaireDocument, 0);

            interview = Create.AggregateRoot.StatefulInterview(questionnaire: plainQuestionnaire);
            interview.AnswerNumericIntegerQuestion(interviewerId, rosterSizeQuestionId, RosterVector.Empty, DateTime.UtcNow, 2);
            interview.AnswerMultipleOptionsQuestion(interviewerId, nestedRosterSizeQuestionId, Create.Entity.RosterVector(0), DateTime.UtcNow, new[] { 1m, 2m });
            interview.AnswerMultipleOptionsQuestion(interviewerId, nestedRosterSizeQuestionId, Create.Entity.RosterVector(1), DateTime.UtcNow, new[] { 1m, 2m });

            interview.AnswerTextQuestion(interviewerId, sourceOfLinkedQuestionId, Create.Entity.RosterVector(0, 1, 100), DateTime.UtcNow, "answer 0.1.100");
            interview.AnswerTextQuestion(interviewerId, sourceOfLinkedQuestionId, Create.Entity.RosterVector(0, 1, 200), DateTime.UtcNow, "answer 0.1.200");
            interview.AnswerTextQuestion(interviewerId, sourceOfLinkedQuestionId, Create.Entity.RosterVector(0, 2, 100), DateTime.UtcNow, "answer 0.2.100");
            interview.AnswerTextQuestion(interviewerId, sourceOfLinkedQuestionId, Create.Entity.RosterVector(0, 2, 200), DateTime.UtcNow, "answer 0.2.200");
            interview.AnswerTextQuestion(interviewerId, sourceOfLinkedQuestionId, Create.Entity.RosterVector(1, 1, 100), DateTime.UtcNow, "answer 1.3.100");
            interview.AnswerTextQuestion(interviewerId, sourceOfLinkedQuestionId, Create.Entity.RosterVector(1, 1, 200), DateTime.UtcNow, "answer 1.3.200");
            //interview.AnswerTextQuestion(interviewerId, sourceOfLinkedQuestionId, Create.Entity.RosterVector(1, 2, 200), DateTime.UtcNow, "answer 1.4.200");
        };

        Because of = () =>
            interview.AnswerTextQuestion(interviewerId, sourceOfLinkedQuestionId, Create.Entity.RosterVector(1, 2, 100), DateTime.UtcNow, "answer 1.4.100");

        It should_linked_single_question_has_7_options = () =>
        {
            var answersToBeOptions = interview
                .FindAnswersOfReferencedQuestionForLinkedQuestion(sourceOfLinkedQuestionId, Create.Entity.Identity(linkedSingleQuestionId, RosterVector.Empty))
                .ToList();

            answersToBeOptions.Count.ShouldEqual(7);
            answersToBeOptions.OfType<TextAnswer>().Select(x => x.Answer).ShouldContainOnly(
                "answer 0.1.100", "answer 0.1.200", "answer 0.2.100", "answer 0.2.200", "answer 1.3.100", "answer 1.3.200", "answer 1.4.100");
        };

        It should_linked_multi_question_has_7_options = () => {
            var answersToBeOptions = interview
                .FindAnswersOfReferencedQuestionForLinkedQuestion(sourceOfLinkedQuestionId, Create.Entity.Identity(linkedMultiQuestionId, RosterVector.Empty))
                .ToList();

            answersToBeOptions.Count.ShouldEqual(7);
            answersToBeOptions.OfType<TextAnswer>().Select(x => x.Answer).ShouldContainOnly(
                "answer 0.1.100", "answer 0.1.200", "answer 0.2.100", "answer 0.2.200", "answer 1.3.100", "answer 1.3.200", "answer 1.4.100");
        };

        static StatefulInterview interview;

        static readonly Guid roster1Id = Guid.Parse("11111111111111111111111111111111");
        static readonly Guid roster2Id = Guid.Parse("22222222222222222222222222222222");
        static readonly Guid roster3Id = Guid.Parse("33333333333333333333333333333333");

        static readonly Guid rosterSizeQuestionId = Guid.Parse("44444444444444444444444444444444");
        static readonly Guid nestedRosterSizeQuestionId = Guid.Parse("55555555555555555555555555555555");

        static readonly Guid linkedSingleQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        static readonly Guid linkedMultiQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

        static readonly Guid sourceOfLinkedQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        static readonly Guid interviewerId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
    }
}