using System;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using NHibernate.Util;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;
using TextAnswer = WB.Core.SharedKernels.Enumerator.Entities.Interview.TextAnswer;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_answering_on_nested_roster_question_which_is_linked_to_question_from_roster : StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            var linkedSingleQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var linkedMultiQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

            linkedSingleQuestionIdentity = Create.Entity.Identity(linkedSingleQuestionId, new RosterVector(new[] {0m}));
            linkedMultiQuestionIdentity = Create.Entity.Identity(linkedMultiQuestionId, new RosterVector(new[] { 0m }));
            sourceOfLinkedQuestionIdentity = Create.Entity.Identity(sourceOfLinkedQuestionId, new RosterVector(new[] {0m, 1m}));

            var rosterId = Guid.Parse("22222222222222222222222222222222");
            var nestedRosterId = Guid.Parse("33333333333333333333333333333333");

            Guid rosterSizeQuestionId = Guid.Parse("44444444444444444444444444444444");
            Guid nestedRosterSizeQuestionId = Guid.Parse("55555555555555555555555555555555");

            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.Group(children: new IComposite[]
                {
                    Create.Entity.NumericIntegerQuestion(id: rosterSizeQuestionId),
                    Create.Entity.Roster(rosterId: rosterId, rosterSizeQuestionId: rosterSizeQuestionId,
                        children: new IComposite[]
                        {
                            Create.Entity.MultipleOptionsQuestion(questionId: nestedRosterSizeQuestionId, answers: new [] {1m, 2m}),
                            Create.Entity.Roster(rosterId: nestedRosterId,
                                rosterSizeQuestionId: nestedRosterSizeQuestionId,
                                children: new IComposite[]
                                {
                                    Create.Entity.TextQuestion(questionId: sourceOfLinkedQuestionId)
                                }),
                            Create.Entity.SingleQuestion(id: linkedSingleQuestionId, linkedToQuestionId: sourceOfLinkedQuestionId),
                            Create.Entity.MultyOptionsQuestion(id: linkedMultiQuestionId, linkedToQuestionId: sourceOfLinkedQuestionId)
                        })
                })
            });
            var plainQuestionnaire = new PlainQuestionnaire(questionnaireDocument, 0);

            interview = Create.AggregateRoot.StatefulInterview(questionnaire: plainQuestionnaire);
            interview.AnswerNumericIntegerQuestion(interviewerId, rosterSizeQuestionId, RosterVector.Empty, DateTime.UtcNow, 1);
            interview.AnswerMultipleOptionsQuestion(interviewerId, nestedRosterSizeQuestionId, new RosterVector(new[] {0m}), DateTime.UtcNow, new[] {1m});
        };

        Because of = () => interview.AnswerTextQuestion(interviewerId, sourceOfLinkedQuestionIdentity.Id, 
            sourceOfLinkedQuestionIdentity.RosterVector, DateTime.UtcNow, expectedLinkedOptionText);

        It should_linked_single_question_has_1_option = () => ((TextAnswer)interview.FindAnswersOfReferencedQuestionForLinkedQuestion(sourceOfLinkedQuestionId,
                    linkedSingleQuestionIdentity).First()).Answer.ShouldEqual(expectedLinkedOptionText);

        It should_linked_multi_question_has_1_option = () => ((TextAnswer)interview.FindAnswersOfReferencedQuestionForLinkedQuestion(sourceOfLinkedQuestionId,
                    linkedMultiQuestionIdentity).First()).Answer.ShouldEqual(expectedLinkedOptionText);

        static StatefulInterview interview;
        static Identity linkedSingleQuestionIdentity;
        static Identity linkedMultiQuestionIdentity;
        static Identity sourceOfLinkedQuestionIdentity;
        static readonly Guid sourceOfLinkedQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        static readonly Guid interviewerId = Guid.Parse("11111111111111111111111111111111");
        static readonly string expectedLinkedOptionText = "text answer";
    }
}