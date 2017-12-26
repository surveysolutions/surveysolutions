using Main.Core.Documents;
using Main.Core.Entities.Composite;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Tests.Abc;
using WB.UI.Headquarters.Models.WebInterview;

namespace WB.Tests.Unit.Applications.Headquarters.WebInterview.Review.Api
{
    public class when_get_details_for_linked_question : WebInterviewInterviewEntityFactorySpecification
    {
        private static readonly Identity LinkedSource = Id.Identity1;
        private static readonly Identity LinkedQuestion = Id.Identity2;
        private InterviewLinkedSingleQuestion singleLinkedQuestion;

        protected override QuestionnaireDocument GetDocument()
        {
            return Create.Entity.QuestionnaireDocumentWithOneChapter(Id.gA,
               Create.Entity.SingleQuestion(LinkedQuestion.Id, linkedToRosterId: LinkedSource.Id),
               Create.Entity.FixedRoster(LinkedSource.Id, 
                    children: new IComposite[] { Create.Entity.TextQuestion() },
                    fixedTitles: new []
                    {
                        Create.Entity.FixedTitle(1, "Test"),
                        Create.Entity.FixedTitle(2, "Test2")
                    })
            );
        }

        protected override void Because()
        {
            // act
            this.singleLinkedQuestion = Subject.GetEntityDetails(LinkedQuestion.ToString(), CurrentInterview, questionnaire, true)
                as InterviewLinkedSingleQuestion;
        }

        [Test]
        public void should_get_InterviewLinkedSingleQuestion_details()
        {
            Assert.That(singleLinkedQuestion, Is.Not.Null);
        }

        [Test]
        public void should_get_id_of_single_linked_question()
        {
            Assert.That(singleLinkedQuestion.Id, Is.EqualTo(LinkedQuestion.ToString()));
        }

        [Test]
        public void should_get_options_from_rosterVector()
        {
            void AssertEqual(LinkedOption option1, LinkedOption option2)
            {
                Assert.That(option1.Title, Is.EqualTo(option2.Title));
                Assert.That(option1.Value, Is.EqualTo(option2.Value));
                Assert.That(option1.RosterVector, Is.EqualTo(option2.RosterVector));
            }

            AssertEqual(singleLinkedQuestion.Options[0],
                new LinkedOption() {Title = "Test", Value = "_1", RosterVector = Create.Entity.RosterVector(1)});

            AssertEqual(singleLinkedQuestion.Options[1],
                new LinkedOption() {Title = "Test2", Value = "_2", RosterVector = Create.Entity.RosterVector(2)});
        }
    }
}
