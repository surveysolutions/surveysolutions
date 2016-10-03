using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.ExportViewFactoryTests
{
    internal class when_creating_export_structure_from_questionnaire_containing_roter_triggered_by_TextListQuestion : ExportViewFactoryTestsContext
    {
        Establish context = () =>
        {
            rosterSizeQuestionId = Guid.Parse("AAF000AAA111EE2DD2EE111AAA000FFF");
            var rosterGroupId = Guid.Parse("00F000AAA111EE2DD2EE111AAA000FFF");
            questionInsideRosterId = Guid.Parse("CCF000AAA111EE2DD2EE111AAA000FFF");

            questionnaireDocument = CreateQuestionnaireDocumentWithOneChapter(
                new TextListQuestion("text list roster size question")
                {
                    PublicKey = rosterSizeQuestionId,
                    QuestionType = QuestionType.TextList,
                    MaxAnswerCount = maxAnswerCount
                },
                new Group("roster group")
                {
                    PublicKey = rosterGroupId,
                    IsRoster = true,
                    RosterSizeSource = RosterSizeSourceType.Question,
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    Children = new List<IComposite>
                    {
                        new NumericQuestion() { PublicKey = questionInsideRosterId, QuestionType = QuestionType.Numeric }
                    }
                });

            var questionnaireMockStorage = new Mock<IQuestionnaireStorage>();
            questionnaireMockStorage.Setup(x => x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), Moq.It.IsAny<string>())).Returns(new PlainQuestionnaire(questionnaireDocument, 1, null));
            questionnaireMockStorage.Setup(x => x.GetQuestionnaireDocument(Moq.It.IsAny<QuestionnaireIdentity>())).Returns(questionnaireDocument);
            exportViewFactory = CreateExportViewFactory(questionnaireMockStorage.Object);
        };

        Because of = () =>
            questionnaireExportStructure = exportViewFactory.CreateQuestionnaireExportStructure(new QuestionnaireIdentity(questionnaireDocument.PublicKey, 1));

        It should_create_header_with_1_column = () =>
            questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid> { rosterSizeQuestionId }].HeaderItems[questionInsideRosterId].ColumnNames.Length.ShouldEqual(1);

        It should_create_header_with_5_columns_at_first_level = () =>
          questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid>()].HeaderItems[rosterSizeQuestionId].ColumnNames.Length.ShouldEqual(5);

        It should_create_header_with_nullable_level_labels = () =>
            questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid>()].LevelLabels.ShouldBeNull();

        private static QuestionnaireExportStructure questionnaireExportStructure;
        private static ExportViewFactory exportViewFactory;
        private static Guid questionInsideRosterId;
        private static QuestionnaireDocument questionnaireDocument;
        private static Guid rosterSizeQuestionId;
        private static int maxAnswerCount=5;
    }
}
