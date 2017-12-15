using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.QuestionnaireTests
{
    public class when_getting_common_parent_for_linked_question_and_source
    {
        [Test]
        public void should_return_null_when_there_is_no_common_parent()
        {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.TextListQuestion(Id.g1),
                Create.Entity.Roster(Id.gA, rosterSizeSourceType: RosterSizeSourceType.Question, rosterSizeQuestionId: Id.g1, children: new IComposite[]
                {
                    Create.Entity.FixedRoster(Id.gB, children: new IComposite[]
                    {
                        Create.Entity.TextListQuestion(Id.g2),
                        Create.Entity.Roster(Id.gC, rosterSizeSourceType: RosterSizeSourceType.Question, rosterSizeQuestionId: Id.g2)
                    }),
                    Create.Entity.Group(Id.gD, children: new IComposite[]
                    {
                        Create.Entity.SingleOptionQuestion(Id.g3, linkedToRosterId: Id.gC)
                    })
                }),
                Create.Entity.Roster(Id.gE, rosterSizeSourceType: RosterSizeSourceType.Question, rosterSizeQuestionId: Id.g1)
            });

            var plainQuestionnaire = Create.Entity.PlainQuestionnaire(questionnaire);

            // Act
            var commonParent = plainQuestionnaire.GetCommonParentRosterForLinkedQuestionAndItSource(Id.g3);

            // Assert
            Assert.That(commonParent, Is.EqualTo(Id.gA));
        }
    }
}