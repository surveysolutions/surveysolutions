using System;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.PreloadedData;
using WB.Core.SharedKernels.DataCollection.ValueObjects;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataServiceTests
{
    internal class when_GetAvalibleIdListForParent_is_called_for_file_corresponding_to_roster_triggered_by_yesno_question : PreloadedDataServiceTestContext
    {
        [Test]
        public void should_not_throw_invalid_cast_exception()
        {
            //arrange
            Guid ynQuestionId = Guid.Parse("11111111111111111111111111111111");
            var ynVariable = "yn1";

            var questionnaireDocument =
                CreateQuestionnaireDocumentWithOneChapter(
                    Create.Entity.YesNoQuestion(ynQuestionId, variable: ynVariable, answers: new []{1}),
                    Create.Entity.Roster(rosterSizeSourceType: RosterSizeSourceType.Question, rosterSizeQuestionId: ynQuestionId));
            
            //act
            var preloadedDataService = CreatePreloadedDataService(questionnaireDocument);

            // assert
            Assert.DoesNotThrow(() => preloadedDataService.GetAvailableIdListForParent(
                CreatePreloadedDataByFile(new[] {"Id", ynVariable + "__1"}, new[] {new[] {"1", "1"}},
                    questionnaireDocument.Title), new ValueVector<Guid> {ynQuestionId}, new[] {"1"},
                new PreloadedDataByFile[0]));
        }
    }
}
