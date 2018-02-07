using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using It = Machine.Specifications.It;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataVerifierTests
{
    internal class when_verifying_preloaded_data_file_with_valid_interviewer_responsible_name : PreloadedDataVerifierTestContext
    {
        private Establish context = () =>
        {
            questionnaireId = Guid.Parse("11111111111111111111111111111111");
            questionnaire = CreateQuestionnaireDocumentWithOneChapter();
            questionnaire.Title = "questionnaire";
            preloadedDataByFile = CreatePreloadedDataByFile(new[] { ServiceColumns.InterviewId, "_responsible" }, new string[][] { new string[] { "1", "fd" } },
                QuestionnaireCsvFileName);

            preloadedDataServiceMock = new Mock<IPreloadedDataService>();

            preloadedDataServiceMock.Setup(x => x.FindLevelInPreloadedData(QuestionnaireCsvFileName)).Returns(new HeaderStructureForLevel(){LevelIdColumnName = ServiceColumns.InterviewId });
            preloadedDataServiceMock.Setup(x => x.GetColumnIndexByHeaderName(preloadedDataByFile, ServiceColumns.ResponsibleColumnName)).Returns(1);
            preloadedDataServiceMock.Setup(x => x.GetColumnIndexByHeaderName(preloadedDataByFile, ServiceColumns.AssignmentsCountColumnName)).Returns(-1);

            var userViewFactory = new Mock<IUserViewFactory>();

            var user = new UserToVerify
            {
                UserName = "fd",
                IsLocked = false,
                IsInterviewer = true,
                IsSupervisor = false
            };
            userViewFactory.Setup(x => x.GetUsersByUserNames(Moq.It.IsAny<string[]>())).Returns(new[] {user});

            importDataVerifier = CreatePreloadedDataVerifier(questionnaire, preloadedDataServiceMock.Object, userViewFactory: userViewFactory.Object);
        };

        Because of =
            () => VerificationErrors = importDataVerifier.VerifyPanelFiles(Create.Entity.PreloadedDataByFile(preloadedDataByFile), preloadedDataServiceMock.Object).ToList();

        It should_result_has_0_error = () =>
            VerificationErrors.Count().ShouldEqual(0);

        
        private static ImportDataVerifier importDataVerifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionnaireId;
        private static PreloadedDataByFile preloadedDataByFile;

        private static Mock<IPreloadedDataService> preloadedDataServiceMock;
        private const string QuestionnaireCsvFileName = "questionnaire.csv";
    }
}
