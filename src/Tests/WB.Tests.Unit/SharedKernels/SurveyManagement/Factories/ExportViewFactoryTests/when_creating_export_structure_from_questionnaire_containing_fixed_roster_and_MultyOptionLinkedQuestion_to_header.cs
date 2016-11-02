using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.ExportViewFactoryTests
{
    internal class when_creating_export_structure_from_questionnaire_containing_fixed_roster_and_MultyOptionLinkedQuestion_to_header : ExportViewFactoryTestsContext
    {
        Establish context = () =>
        {
            rosterGroupId = Guid.Parse("00F000AAA111EE2DD2EE111AAA000FFF");
            linkedQuestionId = Guid.Parse("BBF000AAA111EE2DD2EE111AAA000FFF");
            referencedQuestionId = Guid.Parse("CCF000AAA111EE2DD2EE111AAA000FFF");

            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                Create.Entity.FixedRoster(rosterId: rosterGroupId,
                    title: "roster group", fixedTitles: new[] {"1", "2"}, children: new IComposite[]
                    {
                        new NumericQuestion() {PublicKey = referencedQuestionId, QuestionType = QuestionType.Numeric},
                        new MultyOptionsQuestion()
                        {
                            LinkedToQuestionId = referencedQuestionId,
                            PublicKey = linkedQuestionId,
                            QuestionType = QuestionType.MultyOption
                        }
                    }));


            var questionnaireMockStorage = new Mock<IQuestionnaireStorage>();
            questionnaireMockStorage.Setup(x => x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), Moq.It.IsAny<string>())).Returns(new PlainQuestionnaire(questionnaire, 1, null));
            questionnaireMockStorage.Setup(x => x.GetQuestionnaireDocument(Moq.It.IsAny<QuestionnaireIdentity>())).Returns(questionnaire);
            
            exportViewFactory = CreateExportViewFactory(questionnaireMockStorage.Object, rostrerStructureService: new RosterStructureService());
        };

        private Because of = () =>
            questionnaireExportStructure = exportViewFactory.CreateQuestionnaireExportStructure(new QuestionnaireIdentity(questionnaire.PublicKey, 1));

        It should_create_header_with_2_column = () =>
            questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid> { rosterGroupId }].HeaderItems[linkedQuestionId].ColumnNames.Length.ShouldEqual(2);

        It should_create_header_with_2_header_labeles = () =>
            questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid> { rosterGroupId }].LevelLabels.Length.ShouldEqual(2);

        It should_create_header_with_first_header_label_title_equal_1 = () =>
            questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid> { rosterGroupId }].LevelLabels[0].Title.ShouldEqual("1");

        It should_create_header_with_first_header_label_caption_equal_1 = () =>
            questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid> { rosterGroupId }].LevelLabels[0].Caption.ShouldEqual("0");

        It should_create_header_with_second_header_label_title_equal_1 = () =>
            questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid> { rosterGroupId }].LevelLabels[1].Title.ShouldEqual("2");

        It should_create_header_with_second_header_label_caption_equal_1 = () =>
            questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid> { rosterGroupId }].LevelLabels[1].Caption.ShouldEqual("1");

        private static QuestionnaireExportStructure questionnaireExportStructure;
        private static ExportViewFactory exportViewFactory;
        private static Guid linkedQuestionId;
        private static Guid referencedQuestionId;
        private static QuestionnaireDocument questionnaire;
        private static Guid rosterGroupId;
    }
}
