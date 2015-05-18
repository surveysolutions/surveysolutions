using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.Services.Preloading;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.PreloadedData;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataVerifierTests
{
    internal class when_verifying_preloaded_data_file_has_roster_row : PreloadedDataVerifierTestContext
    {
        Establish context = () =>
        {
            questionnaireId = Guid.Parse("11111111111111111111111111111111");
            var rosterId = Guid.NewGuid();
            questionnaire =
                CreateQuestionnaireDocumentWithOneChapter(new Group(rosterTitle)
                {
                    IsRoster = true,
                    PublicKey = rosterId,
                    RosterSizeSource = RosterSizeSourceType.FixedTitles,
                    RosterFixedTitles = new[] { "a" }
                });
            questionnaire.Title = questionnaireTitle;
            preloadedDataByFileTopLevel = CreatePreloadedDataByFile(new[] { "Id"}, new string[][] { new string[] { "1"} },
                questionnaireTitle + ".csv");
            preloadedDataByFileRosterLevel = CreatePreloadedDataByFile(new[] { "Id", "ParentId1" }, new string[][] { new string[] { "5", "1" } },
                rosterTitle + ".csv");

            files = new[] { preloadedDataByFileTopLevel, preloadedDataByFileRosterLevel };
            preloadedDataServiceMock = new Mock<IPreloadedDataService>();
            preloadedDataServiceMock.Setup(x => x.GetIdColumnIndex(preloadedDataByFileRosterLevel)).Returns(0);
            preloadedDataServiceMock.Setup(x => x.GetParentIdColumnIndexes(preloadedDataByFileRosterLevel)).Returns(new[] { 1 });
            
            preloadedDataServiceMock.Setup(x => x.FindLevelInPreloadedData(preloadedDataByFileTopLevel.FileName)).Returns(new HeaderStructureForLevel());
            preloadedDataServiceMock.Setup(x => x.FindLevelInPreloadedData(preloadedDataByFileRosterLevel.FileName))
                .Returns(new HeaderStructureForLevel() { LevelScopeVector = new ValueVector<Guid>(new[] { rosterId }) });
            preloadedDataServiceMock.Setup(x => x.GetParentDataFile(preloadedDataByFileRosterLevel.FileName, files))
                .Returns(preloadedDataByFileTopLevel);

            preloadedDataServiceMock.Setup(x => x.GetAvailableIdListForParent(preloadedDataByFileTopLevel, Moq.It.IsAny<ValueVector<Guid>>(), new []{"1"}))
                .Returns(new decimal[] { 0 });

            preloadedDataServiceMock.Setup(x => x.GetColumnIndexByHeaderName(preloadedDataByFileTopLevel, Moq.It.IsAny<string>())).Returns(-1);
            preloadedDataVerifier = CreatePreloadedDataVerifier(questionnaire, null, preloadedDataServiceMock.Object);
        };

        Because of =
            () =>
                result =
                    preloadedDataVerifier.VerifyPanel(questionnaireId, 1, files);

        It should_result_has_1_error = () =>
            result.Errors.Count().ShouldEqual(1);

        It should_return_single_PL0009_error = () =>
            result.Errors.First().Code.ShouldEqual("PL0009");

        It should_return_reference_with_Cell_type = () =>
            result.Errors.First().References.First().Type.ShouldEqual(PreloadedDataVerificationReferenceType.Cell);

        It should_error_PositionX_be_equal_to_0 = () =>
          result.Errors.First().References.First().PositionX.ShouldEqual(0);

        It should_error_PositionY_be_equal_to_0 = () =>
          result.Errors.First().References.First().PositionY.ShouldEqual(0);

        It should_error_has_content_id_of_inconsistent_record = () =>
            result.Errors.First().References.First().Content.ShouldEqual("5");

        private static PreloadedDataVerifier preloadedDataVerifier;
        private static VerificationStatus result;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionnaireId;
        private static PreloadedDataByFile preloadedDataByFileTopLevel;
        private static PreloadedDataByFile preloadedDataByFileRosterLevel;
        private static string questionnaireTitle = "questionnaire";
        private static string rosterTitle = "roster";

        private static Mock<IPreloadedDataService> preloadedDataServiceMock;
        private static PreloadedDataByFile[] files;
    }
}
