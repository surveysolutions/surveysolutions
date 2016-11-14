using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireInfoFactoryTests
{
    internal class when_getting_question_view_for_linked_questions_from_different_levels : QuestionnaireInfoFactoryTestContext
    {
        Establish context = () =>
        {
            questionDetailsReaderMock = new Mock<IPlainKeyValueStorage<QuestionnaireDocument>>();
            questionnaireView = Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Roster(g1Id, title: "Roster 1", fixedRosterTitles: new [] { Create.FixedRosterTitle(1, "1") }, children: new IComposite[]
                {
                    Create.Roster(rosterId:g2Id, title:"Roster 1.1", fixedRosterTitles: new [] { Create.FixedRosterTitle(2, "2") }, children: new IComposite[]
                    {
                        Create.Roster(rosterId:g3Id, title:"Roster 1.1.1", fixedRosterTitles: new [] { Create.FixedRosterTitle(3, "3") }, children: new IComposite[]
                        {
                            Create.TextListQuestion(q1Id, title: "text list title"),
                            Create.SingleQuestion(q2Id, linkedToQuestionId: q1Id)
                        }),
                        Create.TextListQuestion(q3Id, title: "text list title"),
                        Create.SingleQuestion(q4Id, linkedToQuestionId: q3Id)
                    }),
                    Create.TextListQuestion(q5Id, title: "text list title"),
                    Create.SingleQuestion(q6Id, linkedToQuestionId: q5Id)
                }),
                Create.TextListQuestion(q7Id, title: "text list title"),
                Create.SingleQuestion(q8Id, linkedToQuestionId: q7Id)
            });

            questionDetailsReaderMock
                .Setup(x => x.GetById(questionnaireId))
                .Returns(questionnaireView);

            factory = CreateQuestionnaireInfoFactory(questionDetailsReaderMock.Object);
        };

        Because of = () =>
        {
            q2View = factory.GetQuestionEditView(questionnaireId, q2Id);
            q4View = factory.GetQuestionEditView(questionnaireId, q4Id);
            q6View = factory.GetQuestionEditView(questionnaireId, q6Id);
            q8View = factory.GetQuestionEditView(questionnaireId, q8Id);
        };

        It should_return_1_elemets_for_dropdown_on_level_0 = () =>
        {
            var listItems = q8View.SourceOfLinkedEntities.Where(x => x.Type == "textlist");
            listItems.Select(x => x.Id).ShouldContainOnly(q7Id.FormatGuid());
        };

        It should_return_2_elemets_for_dropdown_on_level_1 = () =>
        {
            var listItems = q6View.SourceOfLinkedEntities.Where(x => x.Type == "textlist");
            listItems.Select(x => x.Id).ShouldContainOnly(q7Id.FormatGuid(), q5Id.FormatGuid());
        };

        It should_return_3_elemets_for_dropdown_on_level_2 = () =>
        {
            var listItems = q4View.SourceOfLinkedEntities.Where(x => x.Type == "textlist");
            listItems.Select(x => x.Id).ShouldContainOnly(q7Id.FormatGuid(), q5Id.FormatGuid(), q3Id.FormatGuid());
        };

        It should_return_4_elemets_for_dropdown_on_level_3 = () =>
        {
            var listItems = q2View.SourceOfLinkedEntities.Where(x => x.Type == "textlist");
            listItems.Select(x => x.Id).ShouldContainOnly(q7Id.FormatGuid(), q5Id.FormatGuid(), q3Id.FormatGuid(), q1Id.FormatGuid());
        };

        private static QuestionnaireInfoFactory factory;
        private static QuestionnaireDocument questionnaireView;
        private static Mock<IPlainKeyValueStorage<QuestionnaireDocument>> questionDetailsReaderMock;
        private static string questionnaireId = "11111111111111111111111111111111";
        private static NewEditQuestionView q2View;
        private static NewEditQuestionView q4View;
        private static NewEditQuestionView q6View;
        private static NewEditQuestionView q8View;
    }
}