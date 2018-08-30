using System;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Enumerator.Native.WebInterview.Services;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Applications.Headquarters.WebInterview.Review.Api
{
    [TestOf(typeof(WebInterviewInterviewEntityFactory))]
    public class InterviewSimpleStatusTests : WebInterviewInterviewEntityFactorySpecification
    {
        protected override QuestionnaireDocument GetDocument()
        {
            return Create.Entity.QuestionnaireDocument(Guid.NewGuid(),

                Create.Entity.Group(SectionA.Id, "Section A", "SecA", children: new IComposite[]
                {
                    Create.Entity.TextQuestion(SecA_InterviewerQuestion.Id, text: "Interviewer Question", variable: "text_in"),

                    Create.Entity.FixedRoster(SecA_Roster.Id, title: "roster", children: new IComposite[]
                    {
                        Create.Entity.TextQuestion(SecA_Roster_InterviewerQuestion.Id, text: "interviewer q in roster",
                            variable: "text_in_r"),

                    }, fixedTitles: new[] {Create.Entity.FixedTitle(1, "Test")}),
                }));
        }

        protected GroupStatus GetInterviewSimpleStatus() => Subject.GetInterviewSimpleStatus(this.CurrentInterview, this.IsReviewMode);

        [Test]
        public void when_all_questions_answered_should_return_completed_state()
        {
            AnswerTextQuestions(SecA_InterviewerQuestion, SecA_Roster_InterviewerQuestion);

            Assert.That(GetInterviewSimpleStatus(), Is.EqualTo(GroupStatus.Completed));
        }

        [Test]
        public void when_subgroups_has_errors_should_return_interview_invalid_status()
        {
            AnswerTextQuestions(SecA_InterviewerQuestion, SecA_Roster_InterviewerQuestion);
            MarkQuestionAsInvalid(SecA_Roster_InterviewerQuestion);

            Assert.That(GetInterviewSimpleStatus(), Is.EqualTo(GroupStatus.StartedInvalid));
        }

        [Test]
        public void when_any_group_without_answers_should_return_not_started_status()
        {
            AnswerTextQuestions(SecA_InterviewerQuestion);

            Assert.That(GetInterviewSimpleStatus(), Is.EqualTo(GroupStatus.NotStarted));
        }

        [Test]
        public void when_interviews_ha_has_no_answers_should_return_not_started_status()
        {
            Assert.That(GetInterviewSimpleStatus(), Is.EqualTo(GroupStatus.NotStarted));
        }
    }
}
