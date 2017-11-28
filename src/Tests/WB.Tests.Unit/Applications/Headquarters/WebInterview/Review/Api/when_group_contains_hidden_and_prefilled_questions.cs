using System;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Tests.Abc;
using WB.UI.Headquarters.API.WebInterview.Services;
using WB.UI.Headquarters.Models.WebInterview;

namespace WB.Tests.Unit.Applications.Headquarters.WebInterview.Review.Api
{
    [TestOf(typeof(WebInterviewInterviewEntityFactory))]
    public class when_group_contains_hidden_and_prefilled_questions : WebInterviewInterviewEntityFactorySpecification
    {
        private static readonly Identity QuestionInterviewer = Id.Identity1;
        private static readonly Identity QuestionSupervisor = Id.Identity2;
        private static readonly Identity QuestionHidden = Id.Identity3;
        private static readonly Identity QuestionPrefilled = Id.Identity4;

        protected override QuestionnaireDocument GetDocument()
        {
            return Create.Entity.QuestionnaireDocument(Guid.NewGuid(),
                Create.Entity.Group(SecA.Id, "Section A", "SecA", children: new IComposite[]
                {
                    Create.Entity.TextQuestion(QuestionInterviewer.Id,
                        text: "Interviewer Question", variable: "text_in"),
                    Create.Entity.TextQuestion(QuestionSupervisor.Id,
                        text: "Supervisor Questions", variable: "text_sup", scope: QuestionScope.Supervisor),
                    Create.Entity.TextQuestion(QuestionHidden.Id, text: "Hiddden Questions",
                        variable: "text_hidden", scope: QuestionScope.Hidden),
                    Create.Entity.TextQuestion(QuestionPrefilled.Id, text: "Prefilled Questions",
                        variable: "text_prefilled", preFilled: true),
                }));
        }

        [SetUp]
        public void Setup()
        {
            this.interview = Create.AggregateRoot.StatefulInterview(Guid.NewGuid(), questionnaire: this.document);
        }

        private InterviewGroupOrRosterInstance GetGroupFromInterviewEntityFactory(bool asReviewer)
        {
            if (asReviewer)
                this.AsSupervisor();
            else
                this.AsInterviewer();

            var entity = this.Subject.GetEntityDetails(SecA.ToString(), this.interview, this.questionnaire, asReviewer);
            return entity as InterviewGroupOrRosterInstance;
        }

        [Test]
        public void when_has_unanswered_hidden_question_should_be_started_for_reviewer()
        {
            this.AnswerTextQuestions(QuestionInterviewer, QuestionSupervisor, QuestionPrefilled);

            var group = GetGroupFromInterviewEntityFactory(asReviewer: true);

            Assert.That(group.Status, Is.EqualTo(GroupStatus.Started));
        }

        [Test]
        public void when_has_unanswered_hidden_question_should_be_started_for_interviewer()
        {
            this.AnswerTextQuestions(QuestionInterviewer, QuestionPrefilled);

            var group = GetGroupFromInterviewEntityFactory(asReviewer: false);

            Assert.That(group.Status, Is.EqualTo(GroupStatus.Completed));
        }

        [Test]
        public void when_only_hidden_question_has_answer_should_has_started_status_for_reviewer()
        {
            this.AnswerTextQuestions(QuestionHidden);

            var group = GetGroupFromInterviewEntityFactory(asReviewer: true);

            Assert.That(group.Status, Is.EqualTo(GroupStatus.Started));
        }

        [Test]
        public void when_only_hidden_question_has_answer_should_has_notstarted_status_for_interviewer()
        {
            this.AnswerTextQuestions(QuestionHidden);

            var group = GetGroupFromInterviewEntityFactory(asReviewer: false);

            Assert.That(group.Status, Is.EqualTo(GroupStatus.NotStarted));
        }

        [Test]
        public void when_has_invalid_hidden_question_should_be_valid_for_interviewer()
        {
            this.AnswerTextQuestions(QuestionInterviewer, QuestionSupervisor, QuestionHidden, QuestionPrefilled);
            this.MarkQuestionAsInvalid(QuestionHidden);

            var group = GetGroupFromInterviewEntityFactory(asReviewer: false);

            Assert.That(group.Status, Is.EqualTo(GroupStatus.Completed));

            Assert.That(group.Validity.IsValid, Is.EqualTo(true));
        }

        [Test]
        public void when_has_invalid_hidden_question_should_be_invalid_for_reviewer()
        {
            this.AnswerTextQuestions(QuestionInterviewer, QuestionSupervisor, QuestionHidden, QuestionPrefilled);
            this.MarkQuestionAsInvalid(QuestionHidden);

            var group = GetGroupFromInterviewEntityFactory(asReviewer: true);

            Assert.That(group.Status, Is.EqualTo(GroupStatus.Completed));

            Assert.That(group.Validity.IsValid, Is.EqualTo(false));
        }

        [Test]
        public void when_has_invalid_prefilled_question_should_be_invalid_for_reviewer()
        {
            this.AnswerTextQuestions(QuestionInterviewer, QuestionSupervisor, QuestionHidden, QuestionPrefilled);
            this.MarkQuestionAsInvalid(QuestionPrefilled);

            var reviewerView = GetGroupFromInterviewEntityFactory(asReviewer: true);
            Assert.That(reviewerView.Status, Is.EqualTo(GroupStatus.Completed));
            Assert.That(reviewerView.Validity.IsValid, Is.EqualTo(false));
        }
    }
}