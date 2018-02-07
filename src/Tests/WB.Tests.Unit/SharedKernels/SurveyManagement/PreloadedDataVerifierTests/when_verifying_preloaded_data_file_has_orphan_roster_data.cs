using System;
using System.Linq;
using Main.Core.Documents;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataVerifierTests
{
    internal class when_verifying_preloaded_data_file_has_orphan_roster_data : PreloadedDataVerifierTestContext
    {
        [Test]
        public void Should_return_1_error()
        {
            ImportDataVerifier importDataVerifier;
            QuestionnaireDocument questionnaire;
            Guid questionnaireId;
            PreloadedDataByFile preloadedDataByFileTopLevel;
            PreloadedDataByFile preloadedDataByFileRosterLevel;
            string questionnaireTitle = "questionnaire";
            string rosterTitle = "roster";

            Mock<IPreloadedDataService> preloadedDataServiceMock;
            PreloadedDataByFile[] files;

            questionnaireId = Guid.Parse("11111111111111111111111111111111");
            var rosterId = Guid.NewGuid();
            questionnaire =
                CreateQuestionnaireDocumentWithOneChapter(Create.Entity.FixedRoster(rosterId: rosterId,
                    obsoleteFixedTitles: new[] {"a"}, title: rosterTitle));
            questionnaire.Title = questionnaireTitle;
            preloadedDataByFileTopLevel = CreatePreloadedDataByFile(new[] {ServiceColumns.InterviewId}, new string[0][],
                questionnaireTitle + ".csv");
            preloadedDataByFileRosterLevel = CreatePreloadedDataByFile(
                new[] {$"{rosterTitle}__id", ServiceColumns.InterviewId}, new string[][] {new string[] {"0", "1"}},
                rosterTitle + ".csv");
            files = Create.Entity.PreloadedDataByFile(preloadedDataByFileTopLevel, preloadedDataByFileRosterLevel);
            preloadedDataServiceMock = new Mock<IPreloadedDataService>();

            preloadedDataServiceMock.Setup(x => x.GetIdColumnIndex(preloadedDataByFileRosterLevel)).Returns(0);
            preloadedDataServiceMock.Setup(x => x.GetParentIdColumnIndexes(preloadedDataByFileRosterLevel))
                .Returns(new[] {1});
            preloadedDataServiceMock
                .Setup(x => x.GetColumnIndexByHeaderName(preloadedDataByFileRosterLevel, Moq.It.IsAny<string>()))
                .Returns(-1);

            preloadedDataServiceMock.Setup(x => x.FindLevelInPreloadedData(preloadedDataByFileTopLevel.FileName))
                .Returns(new HeaderStructureForLevel() {LevelIdColumnName = ServiceColumns.InterviewId});
            preloadedDataServiceMock.Setup(x => x.FindLevelInPreloadedData(preloadedDataByFileRosterLevel.FileName))
                .Returns(new HeaderStructureForLevel()
                {
                    LevelIdColumnName = $"{rosterTitle}__id",
                    LevelScopeVector = new ValueVector<Guid>(new[] {rosterId})
                });
            preloadedDataServiceMock.Setup(x => x.GetParentDataFile(preloadedDataByFileRosterLevel.FileName, files))
                .Returns(preloadedDataByFileTopLevel);

            preloadedDataServiceMock.Setup(x => x.GetAvailableIdListForParent(preloadedDataByFileTopLevel,
                    Moq.It.IsAny<ValueVector<Guid>>(), new[] {"1"}, files))
                .Returns((int[]) null);
            preloadedDataServiceMock
                .Setup(x => x.GetColumnIndexByHeaderName(preloadedDataByFileTopLevel, Moq.It.IsAny<string>()))
                .Returns(0);
            preloadedDataServiceMock.Setup(x => x.GetAllParentColumnNamesForLevel(
                    Moq.It.IsAny<ValueVector<Guid>>()))
                .Returns(new string[] {ServiceColumns.InterviewId});
            
            importDataVerifier = CreatePreloadedDataVerifier(questionnaire, preloadedDataServiceMock.Object);

            VerificationErrors = importDataVerifier.VerifyPanelFiles(files, preloadedDataServiceMock.Object).ToList();

            Assert.AreEqual(VerificationErrors.Count(), 1);

            Assert.AreEqual(VerificationErrors.First().Code, "PL0008");

            Assert.AreEqual(VerificationErrors.First().References.First().Type,
                PreloadedDataVerificationReferenceType.Cell);

            Assert.AreEqual(VerificationErrors.First().References.First().PositionX, 1);

            Assert.AreEqual(
                VerificationErrors.First().References.First().PositionY, 0);

            Assert.AreEqual(VerificationErrors.First().References.First().Content, "1");

        }
    }
}