using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Tests.Abc;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataVerifierTests
{
    internal class when_verifying_preloaded_data_file_and_user_is_locked_by_HQ : PreloadedDataVerifierTestContext
    {
        [Test]
        public void Should_return_1_error()
        {
            var QuestionnaireCsvFileName = "questionnaire.csv";
            var questionnaireId = Guid.Parse("11111111111111111111111111111111");
            var questionnaire = CreateQuestionnaireDocumentWithOneChapter();
            questionnaire.Title = "questionnaire";
            var preloadedDataByFile = CreatePreloadedDataByFile(new[] {ServiceColumns.InterviewId, "_responsible"},
                new string[][] {new string[] {"1", "fd"}},
                QuestionnaireCsvFileName);

            var preloadedDataServiceMock = new Mock<IPreloadedDataService>();

            preloadedDataServiceMock.Setup(x => x.FindLevelInPreloadedData(QuestionnaireCsvFileName))
                .Returns(new HeaderStructureForLevel(){LevelIdColumnName = ServiceColumns.InterviewId });
            preloadedDataServiceMock
                .Setup(x => x.GetColumnIndexByHeaderName(preloadedDataByFile, Moq.It.IsAny<string>())).Returns(1);
            var userViewFactory = new Mock<IUserViewFactory>();

            var user = new UserView()
            {
                PublicKey = Guid.NewGuid(),
                UserName = "fd",
                IsLockedByHQ = true
            };

            userViewFactory.Setup(x => x.GetUser(Moq.It.IsAny<UserViewInputModel>())).Returns(user);

            var importDataVerifier =
                CreatePreloadedDataVerifier(questionnaire, preloadedDataServiceMock.Object, userViewFactory.Object);


            //act
            importDataVerifier.VerifyPanelFiles(questionnaireId, 1, Create.Entity.PreloadedData(preloadedDataByFile), status);

            Assert.AreEqual(status.VerificationState.Errors.Count(), 1);

            Assert.AreEqual(status.VerificationState.Errors.First().Code,"PL0027");

            Assert.AreEqual(
                status.VerificationState.Errors.First().References.First().Type,
                    PreloadedDataVerificationReferenceType.Cell);

            Assert.AreEqual(
                status.VerificationState.Errors.First().References.First().PositionX,1);

            Assert.AreEqual(
                status.VerificationState.Errors.First().References.First().PositionY, 0);
        
    }
}
}
