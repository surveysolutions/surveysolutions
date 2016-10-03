using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.ExportViewFactoryTests
{
    internal class when_creating_export_structure_from_questionnaire_containing_2_autopropagated_groups_with_one_trigger_max_2_rows_and__ : ExportViewFactoryTestsContext
    {
        Establish context = () =>
        {
            questionnaireDocument = CreateQuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericIntegerQuestion(numericTriggerQuestionId),
                Create.Entity.Roster(roster1Id, rosterSizeQuestionId: numericTriggerQuestionId, rosterSizeSourceType: RosterSizeSourceType.FixedTitles, children: new []
                {
                    Create.Entity.NumericIntegerQuestion(linkedToQuestionId)
                }),
                Create.Entity.Roster(roster2Id, rosterSizeQuestionId: numericTriggerQuestionId, rosterSizeSourceType: RosterSizeSourceType.FixedTitles, children: new[]
                {
                    Create.Entity.MultyOptionsQuestion(linkedQuestionId, linkedToQuestionId: linkedToQuestionId)
                }));

            var questionnaireMockStorage = new Mock<IQuestionnaireStorage>();
            questionnaireMockStorage.Setup(x => x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), Moq.It.IsAny<string>())).Returns(new PlainQuestionnaire(questionnaireDocument, 1, null));
            questionnaireMockStorage.Setup(x => x.GetQuestionnaireDocument(Moq.It.IsAny<QuestionnaireIdentity>())).Returns(questionnaireDocument);
            exportViewFactory = CreateExportViewFactory(questionnaireMockStorage.Object);

        };

        Because of = () =>
            questionnaireExportStructure = exportViewFactory.CreateQuestionnaireExportStructure(questionnaireDocument.PublicKey, 1);

        It should_create_header_with_60_column = () =>
            questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid> { roster2Id }].HeaderItems[linkedQuestionId].ColumnNames.Length.ShouldEqual(60);

        It should_create_header_with_nullable_level_labels = () =>
            questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid>()].LevelLabels.ShouldBeNull();

        private static QuestionnaireExportStructure questionnaireExportStructure;
        private static ExportViewFactory exportViewFactory;
        private static QuestionnaireDocument questionnaireDocument;
        private static readonly Guid numericTriggerQuestionId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid linkedQuestionId = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid linkedToQuestionId = Guid.Parse("33333333333333333333333333333333");
        private static readonly Guid roster1Id = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly Guid roster2Id = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
    }
}
