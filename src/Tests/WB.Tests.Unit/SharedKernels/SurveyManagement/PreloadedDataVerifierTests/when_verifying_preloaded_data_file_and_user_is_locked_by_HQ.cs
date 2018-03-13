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
        public void Should_return_1_PL0027_error()
        {
            var QuestionnaireCsvFileName = "questionnaire.csv";
            var questionnaireId = Guid.Parse("11111111111111111111111111111111");
            var questionnaire = CreateQuestionnaireDocumentWithOneChapter();
            questionnaire.Title = "questionnaire";
            var preloadedDataByFile = CreatePreloadedDataByFile(new[] { ServiceColumns.InterviewId, "_responsible" },
                new[] { new[] { "1", "fd" } },
                QuestionnaireCsvFileName);

            var preloadedDataServiceMock = new Mock<IPreloadedDataService>();

            preloadedDataServiceMock.Setup(x => x.FindLevelInPreloadedData(QuestionnaireCsvFileName))
                .Returns(new HeaderStructureForLevel { LevelIdColumnName = ServiceColumns.InterviewId });
            preloadedDataServiceMock
                .Setup(x => x.GetColumnIndexByHeaderName(preloadedDataByFile, ServiceColumns.ResponsibleColumnName))
                .Returns(1);
            preloadedDataServiceMock
                .Setup(x => x.GetColumnIndexByHeaderName(preloadedDataByFile, ServiceColumns.AssignmentsCountColumnName))
                .Returns(-1);

            var userViewFactory = new Mock<IUserViewFactory>();
            var user = new UserView
            {
                PublicKey = Guid.NewGuid(),
                UserName = "fd",
                IsLockedByHQ = true
            };
            userViewFactory.Setup(x => x.GetUser(Moq.It.IsAny<UserViewInputModel>())).Returns(user);

            var importDataVerifier = CreatePreloadedDataVerifier(questionnaire, preloadedDataServiceMock.Object, userViewFactory.Object);


            //act
            importDataVerifier.VerifyPanelFiles(questionnaireId, 1, Create.Entity.PreloadedDataByFile(preloadedDataByFile), status);

            // Assert
            Assert.That(status.VerificationState.Errors, Has.Count.EqualTo(1));

            var panelImportVerificationError = status.VerificationState.Errors.First();
            Assert.That(panelImportVerificationError, Has.Property(nameof(PanelImportVerificationError.Code)).EqualTo("PL0027"));

            var interviewImportReference = panelImportVerificationError.References.First();
            Assert.That(interviewImportReference.Type, Is.EqualTo(PreloadedDataVerificationReferenceType.Cell));
            Assert.That(interviewImportReference.PositionX, Is.EqualTo(1));
            Assert.That(interviewImportReference.PositionY, Is.EqualTo(0));
        }
    }
}
