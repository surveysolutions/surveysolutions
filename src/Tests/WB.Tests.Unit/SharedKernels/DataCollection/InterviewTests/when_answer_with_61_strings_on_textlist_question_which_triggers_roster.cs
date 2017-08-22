using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_answer_with_61_strings_on_textlist_question_which_triggers_short_roster : InterviewTestsContext
    {
        private Establish context = () =>
        {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

            rosterSizeQuestionId = Guid.Parse("33333333333333333333333333333333");

            var questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                Create.Entity.TextListQuestion(questionId: rosterSizeQuestionId),
                Create.Entity.FixedRoster(children: new List<IComposite>
                {
                    Create.Entity.FixedRoster(children: new List<IComposite>
                    {
                        Create.Entity.Roster(
                            rosterId: Guid.Parse("11111111111111111111111111111111"),
                            rosterSizeSourceType: RosterSizeSourceType.Question,
                            rosterSizeQuestionId: rosterSizeQuestionId)
                    })
                }));

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId,
                Create.Entity.PlainQuestionnaire(questionnaire, 1));

            interview = CreateInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);

            answers=new Tuple<decimal, string>[61];
            for (int i = 0; i < answers.Length; i++)
            {
                answers[i] = new Tuple<decimal, string>(i, i.ToString());
            }
        };

        Because of = () =>
            exception = Catch.Only<InterviewException>(() => interview.AnswerTextListQuestion(userId, rosterSizeQuestionId, new decimal[0], DateTime.Now, answers));

        It should_throw_InterviewException = () =>
            exception.ShouldNotBeNull();

        It should_throw_InterviewException_with_explanation = () =>
           exception.Message.ToLower().ToSeparateWords().ShouldContain("answer", "'61'", "question", "roster", "greater", "60");

        private static Interview interview;
        private static Guid userId;
        private static Guid rosterSizeQuestionId;
        private static Tuple<decimal, string>[] answers;
        private static InterviewException exception;
    }
}