using System;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Enumerator.Native.WebInterview.Models;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Applications.Headquarters.WebInterview.Review.Api.GetGroupDetailsTests
{
    public class when_all_answers_valid_and_answered : WebInterviewInterviewEntityFactorySpecification
    {
        protected override QuestionnaireDocument GetDocument()
        {
            return Create.Entity.QuestionnaireDocument(Guid.NewGuid(),

                Create.Entity.Group(SectionA.Id, "Section A", "SecA", children: new IComposite[]
                {
                    Create.Entity.TextQuestion(SecA_InterviewerQuestion.Id, text: "Interviewer Question", variable: "text_in"),
                    Create.Entity.TextQuestion(SecA_SupervisorQuestion.Id, text: "Supervisor Questions", variable: "text_sup", scope: QuestionScope.Supervisor)
                }));
        }

        [SetUp]
        public void Context()
        {
            this.AnswerTextQuestions( SecA_InterviewerQuestion, SecA_SupervisorQuestion); 
        }

        [Test]
        public void should_mark_group_as_completed_for_interviewer()
        {
            Assert.That(this.GetGroupDetails(SectionA, asReviewer: false).Status, Is.EqualTo(GroupStatus.Completed));
        }

        [Test]
        public void should_mark_group_as_valid_for_interviewer()
        {
            Assert.That(this.GetGroupDetails(SectionA, asReviewer: false).Validity.IsValid, Is.True);
        }

        [Test]
        public void should_mark_group_as_completed_for_reviewer()
        {
            Assert.That(this.GetGroupDetails(SectionA, asReviewer: true).Status, Is.EqualTo(GroupStatus.Completed));
        }

        [Test]
        public void should_mark_group_as_valid_for_reviewer()
        {
            Assert.That(this.GetGroupDetails(SectionA, asReviewer: true).Validity.IsValid, Is.True);
        }
    }
}
