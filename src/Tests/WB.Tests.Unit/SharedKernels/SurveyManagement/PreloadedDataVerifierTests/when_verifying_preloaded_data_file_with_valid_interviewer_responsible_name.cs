using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using It = Machine.Specifications.It;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.PreloadedData;
using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataVerifierTests
{
    internal class when_verifying_preloaded_data_file_with_valid_interviewer_responsible_name : PreloadedDataVerifierTestContext
    {
        Establish context = () =>
        {
            questionnaireId = Guid.Parse("11111111111111111111111111111111");
            questionnaire = CreateQuestionnaireDocumentWithOneChapter();
            questionnaire.Title = "questionnaire";
            preloadedDataByFile = CreatePreloadedDataByFile(new[] { "Id", "_responsible" }, new string[][] { new string[] { "1", "fd" } },
                QuestionnaireCsvFileName);

            preloadedDataServiceMock = new Mock<IPreloadedDataService>();

            preloadedDataServiceMock.Setup(x => x.FindLevelInPreloadedData(QuestionnaireCsvFileName)).Returns(new HeaderStructureForLevel());
            preloadedDataServiceMock.Setup(x => x.GetColumnIndexByHeaderName(preloadedDataByFile, Moq.It.IsAny<string>())).Returns(1);

            var userViewFactory = new Mock<IUserViewFactory>();

            var user = new UserView()
            {
                PublicKey = Guid.NewGuid(),
                UserName = "fd",
                IsLockedByHQ = false,
                IsLockedBySupervisor = false,
                Roles = { UserRoles.Operator }
            };
            userViewFactory.Setup(x => x.Load(Moq.It.IsAny<UserViewInputModel>())).Returns(user);

            preloadedDataVerifier = CreatePreloadedDataVerifier(questionnaire, preloadedDataServiceMock.Object, userViewFactory: userViewFactory.Object);
        };

        Because of =
            () =>
                result =
                    preloadedDataVerifier.VerifyPanel(questionnaireId, 1, new[] { preloadedDataByFile });

        It should_result_has_0_error = () =>
            result.Errors.Count().ShouldEqual(0);

        
        private static PreloadedDataVerifier preloadedDataVerifier;
        private static VerificationStatus result;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionnaireId;
        private static PreloadedDataByFile preloadedDataByFile;

        private static Mock<IPreloadedDataService> preloadedDataServiceMock;
        private const string QuestionnaireCsvFileName = "questionnaire.csv";
    }
}
