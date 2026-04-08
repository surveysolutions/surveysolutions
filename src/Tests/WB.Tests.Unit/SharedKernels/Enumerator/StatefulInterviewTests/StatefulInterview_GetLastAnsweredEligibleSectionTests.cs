using System;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    [TestOf(typeof(StatefulInterview))]
    public class StatefulInterview_GetLastAnsweredEligibleSectionTests
    {
        [Test]
        public void when_no_questions_answered_should_return_null()
        {
            var sectionId = Guid.Parse("11111111111111111111111111111111");
            var questionId = Guid.Parse("22222222222222222222222222222222");

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(chapterId: sectionId,
                children: Create.Entity.TextQuestion(questionId));

            var interview = SetUp.StatefulInterview(questionnaire);

            var result = interview.GetLastAnsweredEligibleSection();

            result.Should().BeNull();
        }

        [Test]
        public void when_interviewer_question_answered_should_return_its_section()
        {
            var sectionId = Guid.Parse("11111111111111111111111111111111");
            var questionId = Guid.Parse("22222222222222222222222222222222");
            var userId = Guid.Parse("33333333333333333333333333333333");

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(chapterId: sectionId,
                children: Create.Entity.TextQuestion(questionId));

            var interview = SetUp.StatefulInterview(questionnaire);
            interview.AnswerTextQuestion(userId, questionId, RosterVector.Empty, DateTime.UtcNow, "answer");

            var result = interview.GetLastAnsweredEligibleSection();

            result.Should().NotBeNull();
            result.Id.Should().Be(sectionId);
        }

        [Test]
        public void when_only_hidden_question_answered_should_return_null()
        {
            var sectionId = Guid.Parse("11111111111111111111111111111111");
            var hiddenQuestionId = Guid.Parse("22222222222222222222222222222222");
            var userId = Guid.Parse("33333333333333333333333333333333");

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(chapterId: sectionId,
                children: Create.Entity.TextQuestion(hiddenQuestionId, scope: QuestionScope.Hidden));

            var interview = SetUp.StatefulInterview(questionnaire);
            // Hidden questions cannot typically be answered by users, but we test the filter
            // by applying the event directly
            interview.Apply(Create.Event.TextQuestionAnswered(hiddenQuestionId, RosterVector.Empty, "answer"));

            var result = interview.GetLastAnsweredEligibleSection();

            result.Should().BeNull();
        }

        [Test]
        public void when_only_supervisor_question_answered_should_return_null()
        {
            var sectionId = Guid.Parse("11111111111111111111111111111111");
            var supervisorQuestionId = Guid.Parse("22222222222222222222222222222222");

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(chapterId: sectionId,
                children: Create.Entity.TextQuestion(supervisorQuestionId, scope: QuestionScope.Supervisor));

            var interview = SetUp.StatefulInterview(questionnaire);
            interview.Apply(Create.Event.TextQuestionAnswered(supervisorQuestionId, RosterVector.Empty, "answer"));

            var result = interview.GetLastAnsweredEligibleSection();

            result.Should().BeNull();
        }

        [Test]
        public void when_only_prefilled_question_answered_should_return_null()
        {
            var sectionId = Guid.Parse("11111111111111111111111111111111");
            var prefilledQuestionId = Guid.Parse("22222222222222222222222222222222");
            var userId = Guid.Parse("33333333333333333333333333333333");

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(chapterId: sectionId,
                children: Create.Entity.TextQuestion(prefilledQuestionId, preFilled: true));

            var interview = SetUp.StatefulInterview(questionnaire);
            interview.AnswerTextQuestion(userId, prefilledQuestionId, RosterVector.Empty, DateTime.UtcNow, "answer");

            var result = interview.GetLastAnsweredEligibleSection();

            result.Should().BeNull();
        }

        [Test]
        public void when_questions_answered_in_multiple_sections_should_return_section_of_last_answered()
        {
            var section1Id = Guid.Parse("11111111111111111111111111111111");
            var section2Id = Guid.Parse("22222222222222222222222222222222");
            var question1Id = Guid.Parse("33333333333333333333333333333333");
            var question2Id = Guid.Parse("44444444444444444444444444444444");
            var userId = Guid.Parse("55555555555555555555555555555555");

            var questionnaire = Create.Entity.QuestionnaireDocument(null, null,
                Create.Entity.Group(groupId: section1Id, children: new[] { Create.Entity.TextQuestion(question1Id) }),
                Create.Entity.Group(groupId: section2Id, children: new[] { Create.Entity.TextQuestion(question2Id) })
            );

            var interview = SetUp.StatefulInterview(questionnaire);
            interview.AnswerTextQuestion(userId, question1Id, RosterVector.Empty, DateTime.UtcNow, "answer1");
            interview.AnswerTextQuestion(userId, question2Id, RosterVector.Empty, DateTime.UtcNow, "answer2");

            var result = interview.GetLastAnsweredEligibleSection();

            result.Should().NotBeNull();
            result.Id.Should().Be(section2Id);
        }

        [Test]
        public void when_eligible_question_answered_among_mixed_scopes_should_return_eligible_section()
        {
            var sectionId = Guid.Parse("11111111111111111111111111111111");
            var interviewerQuestionId = Guid.Parse("22222222222222222222222222222222");
            var supervisorQuestionId = Guid.Parse("33333333333333333333333333333333");
            var hiddenQuestionId = Guid.Parse("44444444444444444444444444444444");
            var userId = Guid.Parse("55555555555555555555555555555555");

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(chapterId: sectionId,
                Create.Entity.TextQuestion(interviewerQuestionId, scope: QuestionScope.Interviewer),
                Create.Entity.TextQuestion(supervisorQuestionId, scope: QuestionScope.Supervisor),
                Create.Entity.TextQuestion(hiddenQuestionId, scope: QuestionScope.Hidden)
            );

            var interview = SetUp.StatefulInterview(questionnaire);
            interview.AnswerTextQuestion(userId, interviewerQuestionId, RosterVector.Empty, DateTime.UtcNow, "answer");
            interview.Apply(Create.Event.TextQuestionAnswered(supervisorQuestionId, RosterVector.Empty, "sup_answer"));
            interview.Apply(Create.Event.TextQuestionAnswered(hiddenQuestionId, RosterVector.Empty, "hid_answer"));

            var result = interview.GetLastAnsweredEligibleSection();

            result.Should().NotBeNull();
            result.Id.Should().Be(sectionId);
        }
    }
}
