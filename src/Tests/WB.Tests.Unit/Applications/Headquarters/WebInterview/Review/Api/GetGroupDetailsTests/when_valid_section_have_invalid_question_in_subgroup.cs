using System;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Enumerator.Native.WebInterview.Models;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Applications.Headquarters.WebInterview.Review.Api.GetGroupDetailsTests
{
    public class when_valid_section_have_invalid_question_in_subgroup : WebInterviewInterviewEntityFactorySpecification
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

        protected override void Because()
        {
            AnswerTextQuestions(SecA_InterviewerQuestion, SecA_Roster_InterviewerQuestion);
            MarkQuestionAsInvalid(SecA_Roster_InterviewerQuestion);
        }

        [Test]
        public void should_have_section_validity_valid() => 
            Assert.That(GetGroupDetails(SectionA, asReviewer: false).Validity.IsValid, Is.EqualTo(true));

        [Test]
        public void should_have_section_status_Started() => 
            Assert.That(GetGroupDetails(SectionA, asReviewer: false).Status, Is.EqualTo(GroupStatus.Started));
    }
}
