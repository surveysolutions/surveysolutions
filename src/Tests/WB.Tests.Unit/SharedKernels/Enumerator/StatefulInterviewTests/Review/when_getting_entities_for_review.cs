using System.Linq;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests.Review
{
    internal class when_getting_entities_for_review : StatefulInterviewTestsContext
    {
        [Test]
        public void should_return_questions_with_all_scopes()
        {
            var chapterId = Id.gA;
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(chapterId: chapterId, 
                children: new IComposite[] {
                    Create.Entity.NumericIntegerQuestion(Id.g1, scope: QuestionScope.Interviewer, variable: "q1"),
                    Create.Entity.NumericIntegerQuestion(Id.g2, scope: QuestionScope.Supervisor, variable: "q2"),
                    Create.Entity.NumericIntegerQuestion(Id.g3, scope: QuestionScope.Hidden, variable: "q3"),
                    Create.Entity.NumericIntegerQuestion(Id.g4, isPrefilled: true, variable: "q4"),
                    Create.Entity.Group(Id.g5),
                    Create.Entity.Variable(Id.g6, variableName: "var1")
                }
            );

            var interview = Setup.StatefulInterview(questionnaire);

            // Act
            var entities = interview.GetUnderlyingEntitiesForReview(Create.Identity(chapterId)).ToList();

            // Assert
            Assert.That(entities, Is.Not.Empty);
            Assert.That(entities, Is.EquivalentTo(new []
            {
                Create.Identity(Id.g1),
                Create.Identity(Id.g2),
                Create.Identity(Id.g3),
                Create.Identity(Id.g4),
                Create.Identity(Id.g5)
            }));
        }
    }
}