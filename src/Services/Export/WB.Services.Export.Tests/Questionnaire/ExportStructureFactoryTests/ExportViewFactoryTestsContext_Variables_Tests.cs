using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Questionnaire.Services;

namespace WB.Services.Export.Tests.Questionnaire.ExportStructureFactoryTests
{
    [TestFixture]
    internal class ExportViewFactoryTestsContext_Variables_Tests : ExportViewFactoryTestsContext
    {
        [Test]
        public void when_create_export_structure_from_questionnaire_containing_variable()
        {
            var questionnaireDocument = CreateQuestionnaireDocumentWithOneChapter(
                Create.NumericIntegerQuestion(numericTriggerQuestionId),
                Create.Variable(variableId),
                Create.Roster(roster1Id, children: new IQuestionnaireEntity[]
                {
                    Create.Variable(variableInRosterId),
                    Create.NumericIntegerQuestion(numericQuestionInRosterId)
                })
            );

            var exportViewFactory = CreateExportViewFactory();


            var questionnaireExportStructure = exportViewFactory.CreateQuestionnaireExportStructure(questionnaireDocument);


            questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid>()].LevelLabels.Should().BeNull();
            questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid>()].HeaderItems[variableId].Should().NotBeNull();
            questionnaireExportStructure.HeaderToLevelMap[new ValueVector<Guid> { roster1Id }].HeaderItems[variableInRosterId].Should().NotBeNull();
        }

        private static readonly Guid numericTriggerQuestionId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid numericQuestionInRosterId = Guid.Parse("33333333333333333333333333333333");
        private static readonly Guid roster1Id = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly Guid variableId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static readonly Guid variableInRosterId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
    }
}
