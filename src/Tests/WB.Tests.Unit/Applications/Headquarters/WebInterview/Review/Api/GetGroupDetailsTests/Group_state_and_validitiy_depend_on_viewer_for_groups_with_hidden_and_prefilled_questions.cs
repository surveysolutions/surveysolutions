using System;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Enumerator.Native.WebInterview.Models;
using WB.Enumerator.Native.WebInterview.Services;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Applications.Headquarters.WebInterview.Review.Api.GetGroupDetailsTests
{
    [TestOf(typeof(WebInterviewInterviewEntityFactory))]
    public class Group_state_and_validitiy_depend_on_viewer_for_groups_with_hidden_and_prefilled_questions : WebInterviewInterviewEntityFactorySpecification
    {
        private static readonly Identity QuestionInterviewer = Id.Identity1;
        private static readonly Identity QuestionSupervisor = Id.Identity2;
        private static readonly Identity QuestionHidden = Id.Identity3;
        private static readonly Identity QuestionPrefilled = Id.Identity4;

        protected override QuestionnaireDocument GetDocument()
        {
            return Create.Entity.QuestionnaireDocument(Guid.NewGuid(),
                Create.Entity.Group(SectionA.Id, "Section A", "SecA", children: new IComposite[]
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
        
        [Test]
        public void when_group_has_unanswered_hidden_question_then_should_be_STARTED_for_reviewer()
        {
            this.AnswerTextQuestions(QuestionInterviewer, QuestionSupervisor, QuestionPrefilled);

            var group = this.GetGroupDetails(SectionA, asReviewer: true);

            Assert.That(group.Status, Is.EqualTo(GroupStatus.Started));
        }

        [Test]
        public void when_group_has_unanswered_hidden_question_then_should_be_COMPLETED_for_interviewer()
        {
            this.AnswerTextQuestions(QuestionInterviewer, QuestionPrefilled);

            var group = this.GetGroupDetails(SectionA, asReviewer: false);

            Assert.That(group.Status, Is.EqualTo(GroupStatus.Completed));
        }

        [Test]
        public void when_group_has_only_hidden_question_with_answer_then_group_should_has_STARTED_status_for_reviewer()
        {
            this.AnswerTextQuestions(QuestionHidden);

            var group = this.GetGroupDetails(SectionA, asReviewer: true);

            Assert.That(group.Status, Is.EqualTo(GroupStatus.Started));
        }

        [Test]
        public void when_group_has_only_hidden_question_with_answer_then_group_should_has_NOTSTARTED_status_for_interviewer()
        {
            this.AnswerTextQuestions(QuestionHidden);

            var group = this.GetGroupDetails(SectionA, asReviewer: false);

            Assert.That(group.Status, Is.EqualTo(GroupStatus.NotStarted));
        }

        [Test]
        public void when_group_has_invalid_hidden_question_then_group_should_has_VALID_status_for_interviewer()
        {
            this.AnswerTextQuestions(QuestionHidden);
            this.MarkQuestionAsInvalid(QuestionHidden);

            var group = this.GetGroupDetails(SectionA, asReviewer: false);

            Assert.That(group.Validity.IsValid, Is.EqualTo(true));
        }

        [Test]
        public void when_group_has_invalid_hidden_question_then_group_should_has_INVALID_status_for_reviewer()
        {
            this.AnswerTextQuestions(QuestionHidden);
            this.MarkQuestionAsInvalid(QuestionHidden);

            var group = this.GetGroupDetails(SectionA, asReviewer: true);

            Assert.That(group.Validity.IsValid, Is.EqualTo(false));
        }

        [Test]
        public void when_group_has_invalid_identifying_question_then_group_should_has_INVALID_status_for_reviewer()
        {
            this.AnswerTextQuestions(QuestionPrefilled);
            this.MarkQuestionAsInvalid(QuestionPrefilled);

            var group = this.GetGroupDetails(SectionA, asReviewer: true);

            Assert.That(group.Validity.IsValid, Is.EqualTo(false));
        }

        [Test]
        public void when_group_has_invalid_identifying_question_then_group_should_has_VALID_status_for_interviewer()
        {
            this.AnswerTextQuestions(QuestionPrefilled);
            this.MarkQuestionAsInvalid(QuestionPrefilled);

            var group = this.GetGroupDetails(SectionA, asReviewer: false);

            Assert.That(group.Validity.IsValid, Is.EqualTo(true));
        }
    }
}
