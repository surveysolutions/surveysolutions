using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataServiceTests
{
    internal class when_GetAvalibleIdListForParent_is_called_for_file_corresponding_to_roster_triggered_by_multyoption_question :
        PreloadedDataServiceTestContext
    {
        [Test]
        [Ignore("KP-11068")]
        public void should_return_single_id()
        {
            Guid rosterGroupId = Guid.NewGuid();
            Guid rosterSizeQuestionId = Guid.NewGuid();
            string rosterSizeQuestionVariableName = "var";

            var questionnaireDocument =
                CreateQuestionnaireDocumentWithOneChapter(
                    new MultyOptionsQuestion()
                    {
                        PublicKey = rosterSizeQuestionId,
                        QuestionType = QuestionType.MultyOption,
                        StataExportCaption = rosterSizeQuestionVariableName,
                        Answers = new List<Answer> { new Answer() { AnswerValue = "3", AnswerText = "three" } }
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
                    CreatePreloadedDataByFile(
                        new string[] { ServiceColumns.InterviewId, rosterSizeQuestionVariableName + "_0", rosterSizeQuestionVariableName + "_1" },
                        new string[][] { new string[] { "1", "3", "" } },
                        questionnaireDocument.Title),
                    new ValueVector<Guid> { rosterSizeQuestionId }, 
                    new[] { "1" }, 
                    Create.Entity.PreloadedDataByFile(new PreloadedDataByFile[0]));

            // Assert
            Assert.That(result, Is.EqualTo(new[] { 1 }));
        }
    }
}
