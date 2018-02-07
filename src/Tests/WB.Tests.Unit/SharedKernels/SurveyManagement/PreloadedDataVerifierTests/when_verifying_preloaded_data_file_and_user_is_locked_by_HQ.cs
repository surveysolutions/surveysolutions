using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataVerifierTests
{
    internal class when_verifying_preloaded_data_file_and_user_is_locked_by_HQ : PreloadedDataVerifierTestContext
    {
        [Test]
        public void Should_return_1_error()
        {
            var QuestionnaireCsvFileName = "questionnaire.csv";
            var questionnaire = CreateQuestionnaireDocumentWithOneChapter();
            questionnaire.Title = "questionnaire";
            var preloadedDataByFile = CreatePreloadedDataByFile(new[] {ServiceColumns.InterviewId, "_responsible"},
                new string[][] {new string[] {"1", "fd"}},
                QuestionnaireCsvFileName);

            var preloadedDataServiceMock = new Mock<IPreloadedDataService>();

            preloadedDataServiceMock.Setup(x => x.FindLevelInPreloadedData(QuestionnaireCsvFileName))
                .Returns(new HeaderStructureForLevel(){LevelIdColumnName = ServiceColumns.InterviewId });
            preloadedDataServiceMock.Setup(x => x.GetColumnIndexByHeaderName(preloadedDataByFile, ServiceColumns.AssignmentsCountColumnName)).Returns(-1);
            preloadedDataServiceMock.Setup(x => x.GetColumnIndexByHeaderName(preloadedDataByFile, ServiceColumns.ResponsibleColumnName)).Returns(1);
            var userViewFactory = new Mock<IUserViewFactory>();

            var user = new UserToVerify
            {
                UserName = "fd",
                IsLocked = true
            };

            userViewFactory.Setup(x => x.GetUsersByUserNames(Moq.It.IsAny<string[]>())).Returns(new[] {user});

            var importDataVerifier =
                CreatePreloadedDataVerifier(questionnaire, preloadedDataServiceMock.Object, userViewFactory.Object);


            //act
            VerificationErrors = importDataVerifier.VerifyPanelFiles(Create.Entity.PreloadedDataByFile(preloadedDataByFile), preloadedDataServiceMock.Object).ToList();

            Assert.AreEqual(VerificationErrors.Count(), 1);

            Assert.AreEqual(VerificationErrors.First().Code,"PL0027");

            Assert.AreEqual(
                VerificationErrors.First().References.First().Type,
                    PreloadedDataVerificationReferenceType.Cell);

            Assert.AreEqual(
                VerificationErrors.First().References.First().PositionX,1);

            Assert.AreEqual(
                VerificationErrors.First().References.First().PositionY, 0);
        
    }
}
}
