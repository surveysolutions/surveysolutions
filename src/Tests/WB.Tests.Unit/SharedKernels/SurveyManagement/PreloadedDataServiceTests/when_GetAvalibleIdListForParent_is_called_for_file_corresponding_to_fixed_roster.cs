using System;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataServiceTests
{
    internal class when_GetAvalibleIdListForParent_is_called_for_file_corresponding_to_fixed_roster : PreloadedDataServiceTestContext
    {
        [Test] 
        [Ignore("KP-11068")]
        public void should_result_have_2_ids_1_and_2 ()
        {
            var rosterGroupId = Id.g1;
            var questionnaireDocument =
                CreateQuestionnaireDocumentWithOneChapter(Create.Entity.FixedRoster(rosterId: rosterGroupId,
                        obsoleteFixedTitles: new[] { "1", "2" }, title: "Roster Group"));

            var importDataParsingService = CreatePreloadedDataService(questionnaireDocument);

            // Act
            var result =
                importDataParsingService.GetAvailableIdListForParent(
                    CreatePreloadedDataByFile(new string[] {ServiceColumns.InterviewId},
                        new string[][] {new string[] {"1"}},
                        questionnaireDocument.Title), new ValueVector<Guid> {rosterGroupId}, new[] {"1"},
                    Create.Entity.PreloadedDataByFile(new PreloadedDataByFile[0]));


            // Assert
            Assert.That(result, Is.EqualTo(new [] { 1, 2 }));
        }

    }
}
