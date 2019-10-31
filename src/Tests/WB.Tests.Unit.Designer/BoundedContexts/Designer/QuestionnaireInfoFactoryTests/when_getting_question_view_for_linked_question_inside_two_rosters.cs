using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;


namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireInfoFactoryTests
{
    internal class when_getting_question_view_for_linked_question_inside_two_rosters : QuestionnaireInfoFactoryTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionDetailsReaderMock = new Mock<IDesignerQuestionnaireStorage>();
            questionnaireView = Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.TextListQuestion(q1Id, title: "text list title - l1"),
                Create.Roster(g1Id, title: "Roster 1", rosterType: RosterSizeSourceType.Question, rosterSizeQuestionId: q1Id, children: new IComposite[]
                {
                    Create.TextQuestion(q2Id, text: "text title - t1"),
                    Create.TextListQuestion(q3Id, title: "text list title - l2"),
                    Create.Roster(g2Id, title: "Roster 2", rosterType: RosterSizeSourceType.Question, rosterSizeQuestionId: q3Id, children: new IComposite[]
                    {
                        Create.Subsection(sectionId: g3Id, title: "Sub section", children: new IComposite[]
                        {
                            Create.TextQuestion(q4Id, text: "text title - t2"),
                            Create.SingleOptionQuestion(q5Id, title: "single linked - sl1"),
                        }),
                    }),
                }),
            });

            questionDetailsReaderMock
                .Setup(x => x.Get(questionnaireId))
                .Returns(questionnaireView);

            factory = CreateQuestionnaireInfoFactory(questionDetailsReaderMock.Object);
            BecauseOf();
        }

        private void BecauseOf() 
        {
            q5View = factory.GetQuestionEditView(questionnaireId, q5Id);
        }

        [NUnit.Framework.Test] public void should_return_4_elemets_for_dropdown () 
        {
            var listItems = q5View.SourceOfLinkedEntities.Where(x => x.Type == "textlist" || x.Type == "text");
            listItems.Select(x => x.Id).Should().BeEquivalentTo(new [] { q1Id.FormatGuid(), q2Id.FormatGuid(), q3Id.FormatGuid(), q4Id.FormatGuid() });
        }

        private static QuestionnaireInfoFactory factory;
        private static QuestionnaireDocument questionnaireView;
        private static Mock<IDesignerQuestionnaireStorage> questionDetailsReaderMock;
        private static NewEditQuestionView q5View;
    }
}
