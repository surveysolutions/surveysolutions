using System;
using System.Linq;
using FluentAssertions;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataServiceTests
{
    [TestFixture]
    internal class when_GetAvalibleIdListForParent_is_called_for_file_corresponding_to_roster_triggered_by_textlist_question : PreloadedDataServiceTestContext
    {
        [OneTimeSetUp] 
        public void Setup()
        {
            questionnaireDocument =
                CreateQuestionnaireDocumentWithOneChapter(
                    new TextListQuestion() { PublicKey = rosterSizeQuestionId, QuestionType = QuestionType.TextList, StataExportCaption = rosterSizeQuestionVariableName, MaxAnswerCount = 1},
                    new Group("Roster Group")
                    {
                        IsRoster = true,
                        RosterSizeSource = RosterSizeSourceType.Question,
                        PublicKey = rosterGroupId,
                        RosterSizeQuestionId = rosterSizeQuestionId
                    });

            importDataParsingService = CreatePreloadedDataService(questionnaireDocument);

            result =
                importDataParsingService.GetAvailableIdListForParent(
                    CreatePreloadedDataByFile(
                        new string[] {ServiceColumns.InterviewId, rosterSizeQuestionVariableName + "_0"},
                        new string[][] {new string[] {"1", "3"}},
                        questionnaireDocument.Title), new ValueVector<Guid> {rosterSizeQuestionId}, new[] {"1"},
                    Create.Entity.PreloadedDataByFile(new PreloadedDataByFile[0]));
        }
        
        [Test]
        public void  should_return_not_null_result() =>
            result.Should().NotBeNull();

        [Test]
        public void should_result_have_2_ids_1_and_2() =>
            result.SequenceEqual(new [] { 1 });

        private static ImportDataParsingService importDataParsingService;
        private static QuestionnaireDocument questionnaireDocument;
        private static int[] result;
        private static Guid rosterGroupId = Guid.NewGuid();
        private static Guid rosterSizeQuestionId = Guid.NewGuid();
        private static string rosterSizeQuestionVariableName = "var";
    }
}
