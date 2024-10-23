using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_getting_unanswered_interview_questions_having_hidden_and_supervisors_questions : StatefulInterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocument(questionnaireId,
                children: Create.Entity.Group(groupId: group.Id, children: new List<IComposite>()
                {
                    Create.Entity.Group(children: new[]
                    {
                        Create.Entity.TextQuestion(superQuestionId, scope:QuestionScope.Supervisor),
                        Create.Entity.TextQuestion(disabledQuestionId, scope:QuestionScope.Interviewer),
                        Create.Entity.TextQuestion(questionId, scope:QuestionScope.Interviewer),
                        Create.Entity.TextQuestion(hidden1Id, scope:QuestionScope.Hidden),
                        Create.Entity.TextQuestion(prefieldQuestionId, preFilled:true),
                    })
                })));

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            interview = Create.AggregateRoot.StatefulInterview(questionnaireId: questionnaireId, questionnaireRepository: questionnaireRepository);
            
            interview.Apply(Create.Event.AnswersDeclaredInvalid(new[] {Create.Entity.Identity(questionId), Create.Entity.Identity(disabledQuestionId), Create.Entity.Identity(prefieldQuestionId) }));
            interview.Apply(Create.Event.QuestionsDisabled(new[] { Create.Entity.Identity(disabledQuestionId) }));
            BecauseOf();
        }

        private void BecauseOf() =>
            unansweredQuestionsInInterview = interview.GetAllUnansweredQuestions(false);

        [NUnit.Framework.Test] public void shouldreturn_1_unanswered_questions () =>
            unansweredQuestionsInInterview.Count().Should().Be(2);

        [NUnit.Framework.Test] public void should_contain_only_unanswered_element () =>
            unansweredQuestionsInInterview.Should().BeEquivalentTo(new []{
                Create.Entity.Identity(prefieldQuestionId),
                Create.Entity.Identity(questionId),
            });

        private static StatefulInterview interview;
        private static readonly Guid hidden1Id = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly Guid prefieldQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static readonly Guid questionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static readonly Guid disabledQuestionId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static readonly Guid superQuestionId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static IEnumerable<Identity> unansweredQuestionsInInterview;
        private static Guid questionnaireId = Guid.Parse("99999999999999999999999999999999");
        static readonly Identity group = Create.Identity(Guid.Parse("11111111111111111111111111111111"));
    }
}
