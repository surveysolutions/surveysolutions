using System;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using NUnit.Framework;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Applications.Headquarters.WebInterview.Review.Api
{
    public class when_valid_section_have_invalid_question_in_subgroup : WebInterviewInterviewEntityFactorySpecification
    {
        protected override QuestionnaireDocument GetDocument()
        {
            return Create.Entity.QuestionnaireDocument(Guid.NewGuid(),

                Create.Entity.Group(SecA.Id, "Section A", "SecA", children: new IComposite[]
                {
                    Create.Entity.TextQuestion(SecA_In.Id, text: "Interviewer Question", variable: "text_in"),
                    
                    Create.Entity.FixedRoster(SecA_Roster.Id, title: "roster", children: new IComposite[]
                    {
                        Create.Entity.TextQuestion(SecA_Roster_In.Id, text: "interviewer q in roster",
                            variable: "text_in_r"),
                       
                    }, fixedTitles: new[] {Create.Entity.FixedTitle(1, "Test")}),
                }));
        }

        protected override void Because()
        {
            AnswerTextQuestions(SecA_In, SecA_Roster_In);
            MarkQuestionAsInvalid(SecA_Roster_In);
        }

        [Test]
        public void should_have_section_validity_valid() => Assert.That(RootGroupDetails.Validity.IsValid, Is.EqualTo(true));
    }
}