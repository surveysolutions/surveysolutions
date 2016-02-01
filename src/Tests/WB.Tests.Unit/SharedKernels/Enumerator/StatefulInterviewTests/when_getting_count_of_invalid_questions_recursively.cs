using System;
using System.Collections.Generic;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;

using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_getting_count_of_invalid_questions_recursively : StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            var targetRosterVector = new decimal[0] { };

            var questionnaire = Mock.Of<IQuestionnaire>(x => x.GetAllUnderlyingInterviewerQuestions(group.Id) == new List<Guid>
                                                            {
                                                                question1Id,
                                                                question2Id,
                                                                question3Id
                                                            }.ToReadOnlyCollection());
            IPlainQuestionnaireRepository questionnaireRepository = Create.QuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            interview = Create.StatefulInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);

            interview.Apply(Create.Event.TextQuestionAnswered(question1Id, targetRosterVector, "answer"));
            interview.Apply(Create.Event.AnswersDeclaredInvalid(question1Id, targetRosterVector));
            interview.Apply(Create.Event.TextQuestionAnswered(question2Id, targetRosterVector, "answer2"));
            interview.Apply(Create.Event.AnswersDeclaredInvalid(question2Id, targetRosterVector));
            interview.Apply(Create.Event.QuestionsDisabled(question2Id, targetRosterVector));
            interview.Apply(Create.Event.TextQuestionAnswered(question2Id, targetRosterVector, "answer3"));
        };

        Because of = () =>
            countOfEnabledInvalidAnswers = interview.CountInvalidInterviewerAnswersInGroupRecursively(group);

        It should_reduce_roster_vector_to_find_target_question_answer = () =>
            countOfEnabledInvalidAnswers.ShouldEqual(1);

        private static StatefulInterview interview;
        private static readonly Guid question1Id = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly Guid question2Id = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static readonly Guid question3Id = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static int countOfEnabledInvalidAnswers;
        private static Guid questionnaireId = Guid.Parse("99999999999999999999999999999999");
        static readonly Identity group = new Identity(Guid.Parse("11111111111111111111111111111111"), new decimal[0]);
    }
}