using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Tests.Abc;
using WB.UI.Headquarters.API.PublicApi.Models;

namespace WB.Tests.Unit.Applications.Headquarters.PublicApiTests
{
    [TestFixture]
    internal class InterviewApiDetailsTests
    {
        private readonly Guid userId = Guid.NewGuid();

        [Test]
        public void text_list_answer_with_comma_in_value_should_be_pipe_delimited_in_api_response()
        {
            var textListQuestionId = Guid.NewGuid();
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextListQuestion(textListQuestionId));
            var interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);
            interview.AnswerTextListQuestion(userId, textListQuestionId, RosterVector.Empty, DateTime.Now, new[]
            {
                Tuple.Create(1m, "New York, NY"),
                Tuple.Create(2m, "Washington, DC"),
            });

            var apiDetails = new InterviewApiDetails(interview);

            var answer = apiDetails.Answers.Single(a => a.QuestionId.Id == textListQuestionId);
            Assert.That(answer.Answer, Is.EqualTo("New York, NY|Washington, DC"));
        }

        [Test]
        public void text_list_answer_with_pipe_in_value_should_escape_pipe_in_api_response()
        {
            var textListQuestionId = Guid.NewGuid();
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextListQuestion(textListQuestionId));
            var interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);
            interview.AnswerTextListQuestion(userId, textListQuestionId, RosterVector.Empty, DateTime.Now, new[]
            {
                Tuple.Create(1m, "a|b"),
                Tuple.Create(2m, "c"),
            });

            var apiDetails = new InterviewApiDetails(interview);

            var answer = apiDetails.Answers.Single(a => a.QuestionId.Id == textListQuestionId);
            Assert.That(answer.Answer, Is.EqualTo(@"a\|b|c"));
        }

        [Test]
        public void unanswered_text_list_question_should_return_null_in_api_response()
        {
            var textListQuestionId = Guid.NewGuid();
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextListQuestion(textListQuestionId));
            var interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);

            var apiDetails = new InterviewApiDetails(interview);

            var answer = apiDetails.Answers.Single(a => a.QuestionId.Id == textListQuestionId);
            Assert.That(answer.Answer, Is.Null);
        }

        [Test]
        public void unanswered_multi_linked_to_list_question_should_return_null_in_api_response()
        {
            var textListQuestionId = Guid.NewGuid();
            var linkedMultiQuestionId = Guid.NewGuid();
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextListQuestion(textListQuestionId),
                Create.Entity.MultyOptionsQuestion(linkedMultiQuestionId,
                    linkedToQuestionId: textListQuestionId,
                    options: new List<Main.Core.Entities.SubEntities.Answer>()));
            var interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);
            interview.AnswerTextListQuestion(userId, textListQuestionId, RosterVector.Empty, DateTime.Now, new[]
            {
                Tuple.Create(1m, "one"),
                Tuple.Create(2m, "two"),
            });

            var apiDetails = new InterviewApiDetails(interview);

            var answer = apiDetails.Answers.Single(a => a.QuestionId.Id == linkedMultiQuestionId);
            Assert.That(answer.Answer, Is.Null);
        }

        [Test]
        public void answered_multi_linked_to_list_question_should_return_pipe_delimited_texts_in_api_response()
        {
            var textListQuestionId = Guid.NewGuid();
            var linkedMultiQuestionId = Guid.NewGuid();
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextListQuestion(textListQuestionId),
                Create.Entity.MultyOptionsQuestion(linkedMultiQuestionId,
                    linkedToQuestionId: textListQuestionId,
                    options: new List<Main.Core.Entities.SubEntities.Answer>()));
            var interview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);
            interview.AnswerTextListQuestion(userId, textListQuestionId, RosterVector.Empty, DateTime.Now, new[]
            {
                Tuple.Create(1m, "one"),
                Tuple.Create(2m, "two"),
                Tuple.Create(3m, "three"),
            });
            interview.AnswerMultipleOptionsQuestion(userId, linkedMultiQuestionId, RosterVector.Empty, DateTime.Now, new int[] { 1, 3 });

            var apiDetails = new InterviewApiDetails(interview);

            var answer = apiDetails.Answers.Single(a => a.QuestionId.Id == linkedMultiQuestionId);
            Assert.That(answer.Answer, Is.EqualTo("one|three"));
        }
    }
}
