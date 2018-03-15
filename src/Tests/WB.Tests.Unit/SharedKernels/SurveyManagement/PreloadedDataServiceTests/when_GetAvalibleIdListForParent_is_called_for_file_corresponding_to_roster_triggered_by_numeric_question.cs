using System;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataServiceTests
{
    internal class when_GetAvalibleIdListForParent_is_called_for_file_corresponding_to_roster_triggered_by_numeric_question : PreloadedDataServiceTestContext
    {
        [Test]
        public void should_return_array_with_0_1_2()
        {
            var rosterSizeQuestionId = Id.g1;
            var rosterGroupId = Id.g2;
            const string rosterSizeQuestionVariableName = "var";


            var questionnaireDocument =
                CreateQuestionnaireDocumentWithOneChapter(
                    new NumericQuestion()
                    {
                        PublicKey = rosterSizeQuestionId, QuestionType = QuestionType.Numeric,
                        StataExportCaption = rosterSizeQuestionVariableName, IsInteger = true
                    },
                    new Group("Roster Group")
                    {
                        IsRoster = true,
                        RosterSizeSource = RosterSizeSourceType.Question,
                        PublicKey = rosterGroupId,
                        RosterSizeQuestionId = rosterSizeQuestionId
                    });

            var importDataParsingService = CreatePreloadedDataService(questionnaireDocument);

            // Act
            var result =
                importDataParsingService.GetAvailableIdListForParent(
                    CreatePreloadedDataByFile(new string[] { ServiceColumns.InterviewId, rosterSizeQuestionVariableName }, new string[][] { new string[] { "1", "3" } },
                        questionnaireDocument.Title), new ValueVector<Guid> { rosterSizeQuestionId }, new[] { "1" }, Create.Entity.PreloadedDataByFile(new PreloadedDataByFile[0]));

            // Assert
            Assert.That(result, Is.EqualTo(new[] { 0, 1, 2 }));
        }

    }
}
