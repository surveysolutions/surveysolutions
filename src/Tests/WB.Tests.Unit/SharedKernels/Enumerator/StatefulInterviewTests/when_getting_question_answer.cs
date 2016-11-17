using System;
using Machine.Specifications;
using Moq;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;

using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_getting_answer_on_text_question_from_fixed_roster : StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            questionnaireId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            textQuestionIdentity = Identity.Create(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), Create.Entity.RosterVector(0));

            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(children: new[]
            {
                Create.Entity.FixedRoster(fixedRosterTitles: new []{ Create.Entity.FixedRosterTitle(0, "fixter fixed title")}, children: new []
                {
                    Create.Entity.TextQuestion(textQuestionIdentity.Id)
                })
            }));

            IQuestionnaireStorage questionnaireRepository = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireId, questionnaire);

            interview = Create.AggregateRoot.StatefulInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);
        };

        Because of = () => interview.AnswerTextQuestion(Guid.NewGuid(), textQuestionIdentity.Id, textQuestionIdentity.RosterVector,
                    DateTime.UtcNow, answerOnTextQuestionInFidexRoster);

        It should_reduce_roster_vector_to_find_target_question_answer =
            () => interview.GetTextQuestion(textQuestionIdentity)
                    .GetAnswer()
                    .Value.ShouldEqual(answerOnTextQuestionInFidexRoster);

        private static StatefulInterview interview;
        private static Identity textQuestionIdentity;
        private static Guid questionnaireId;
        private static string answerOnTextQuestionInFidexRoster = "answer";
    }
}

