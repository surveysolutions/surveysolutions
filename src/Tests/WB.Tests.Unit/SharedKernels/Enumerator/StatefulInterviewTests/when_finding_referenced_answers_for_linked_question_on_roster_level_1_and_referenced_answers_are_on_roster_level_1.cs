using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Repositories;

using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    [Ignore("KP-8159")]
    internal class when_finding_referenced_answers_for_linked_question_on_roster_level_1_and_referenced_answers_are_on_roster_level_1 : StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            IQuestionnaireStorage questionnaireRepository =
                Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireId,
                    Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(new IComposite[]
                    {
                        Create.Entity.FixedRoster(
                            fixedTitles:
                                new[]
                                {
                                    new FixedRosterTitle(1, "first fixed roster"),
                                    new FixedRosterTitle(2, "second fixed roster")
                                },
                            children: new[]
                            {
                                Create.Entity.FixedRoster(
                                    fixedTitles:
                                        new[]
                                        {
                                            new FixedRosterTitle(1, "first NESTED fixed roster"),
                                            new FixedRosterTitle(2, "second NESTED fixed roster")
                                        },
                                    children: new[]
                                    {
                                        Create.Entity.NumericIntegerQuestion(sourceOfLinkedQuestionId)
                                    })
                            }),
                        Create.Entity.MultyOptionsQuestion(linkedToQuestionIdentity.Id, linkedToQuestionId: sourceOfLinkedQuestionId)
                    })));

            interview = Create.AggregateRoot.StatefulInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);
        };

        Because of = () =>
        {
            interview.AnswerNumericIntegerQuestion(interviewerId, sourceOfLinkedQuestionId, Create.Entity.RosterVector(1, 1), DateTime.UtcNow, 1);
            interview.AnswerNumericIntegerQuestion(interviewerId, sourceOfLinkedQuestionId, Create.Entity.RosterVector(1, 2), DateTime.UtcNow, 2);

            interview.AnswerNumericIntegerQuestion(interviewerId, sourceOfLinkedQuestionId, Create.Entity.RosterVector(2, 1), DateTime.UtcNow, 3);
            interview.AnswerNumericIntegerQuestion(interviewerId, sourceOfLinkedQuestionId, Create.Entity.RosterVector(2, 2), DateTime.UtcNow, 4);
        };

        It should_return_2_answers_from_2_roster_instances = () => interview.GetLinkedMultiOptionQuestion(linkedToQuestionIdentity)
            .Options
            .Select(x => interview.GetAnswerAsString(Identity.Create(sourceOfLinkedQuestionId, x)))
            .ShouldContainOnly("1", "2", "3", "4");

        private static StatefulInterview interview;
        private static Guid interviewerId = Guid.Parse("55555555555555555555555555555555");
        private static Guid questionnaireId = Guid.Parse("44444444444444444444444444444444");
        private static Identity linkedToQuestionIdentity = Identity.Create(Guid.Parse("33333333333333333333333333333333"), Create.Entity.RosterVector(0));
        private static Guid sourceOfLinkedQuestionId = Guid.Parse("22222222222222222222222222222222");
    }
}