using System;
using FluentAssertions;
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
using WB.Tests.Abc;


namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.ExportViewFactoryTests
{
    internal class when_creating_export_structure_from_questionnaire_containing_fixed_roster_and_MultyOptionLinkedQuestion_to_header : ExportViewFactoryTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            rosterGroupId = Guid.Parse("00F000AAA111EE2DD2EE111AAA000FFF");
            linkedQuestionId = Guid.Parse("BBF000AAA111EE2DD2EE111AAA000FFF");
            referencedQuestionId = Guid.Parse("CCF000AAA111EE2DD2EE111AAA000FFF");

            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                Create.Entity.FixedRoster(rosterId: rosterGroupId,
                    title: "roster group", obsoleteFixedTitles: new[] {"1", "2"}, children: new IComposite[]
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
            questionnaireMockStorage.Setup(x => x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), Moq.It.IsAny<string>())).Returns(Create.Entity.PlainQuestionnaire(questionnaire, 1, null));
            questionnaireMockStorage.Setup(x => x.GetQuestionnaireDocument(Moq.It.IsAny<QuestionnaireIdentity>())).Returns(questionnaire);
            
            exportViewFactory = CreateExportViewFactory(questionnaireMockStorage.Object, rosterStructureService: new RosterStructureService());
            BecauseOf();
        }

        private void BecauseOf() =>
            questionnaireExportStructure = exportViewFactory.CreateQuestionnaireExportStructure(new QuestionnaireIdentity(questionnaire.PublicKey, 1));

        [NUnit.Framework.Test] public void should_create_header_with_2_column () =>
            questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid> { rosterGroupId }].HeaderItems[linkedQuestionId].ColumnHeaders.Count.Should().Be(2);

        [NUnit.Framework.Test] public void should_create_header_with_2_header_labeles () =>
            questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid> { rosterGroupId }].LevelLabels.Length.Should().Be(2);

        [NUnit.Framework.Test] public void should_create_header_with_first_header_label_title_equal_1 () =>
            questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid> { rosterGroupId }].LevelLabels[0].Title.Should().Be("1");

        [NUnit.Framework.Test] public void should_create_header_with_first_header_label_caption_equal_1 () =>
            questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid> { rosterGroupId }].LevelLabels[0].Caption.Should().Be("0");

        [NUnit.Framework.Test] public void should_create_header_with_second_header_label_title_equal_1 () =>
            questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid> { rosterGroupId }].LevelLabels[1].Title.Should().Be("2");

        [NUnit.Framework.Test] public void should_create_header_with_second_header_label_caption_equal_1 () =>
            questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid> { rosterGroupId }].LevelLabels[1].Caption.Should().Be("1");

        private static QuestionnaireExportStructure questionnaireExportStructure;
        private static ExportViewFactory exportViewFactory;
        private static Guid linkedQuestionId;
        private static Guid referencedQuestionId;
        private static QuestionnaireDocument questionnaire;
        private static Guid rosterGroupId;
    }
}
