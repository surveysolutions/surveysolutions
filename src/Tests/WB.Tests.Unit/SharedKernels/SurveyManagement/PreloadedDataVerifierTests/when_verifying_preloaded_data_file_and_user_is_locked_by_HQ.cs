﻿using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataVerifierTests
{
    internal class when_verifying_preloaded_data_file_and_user_is_locked_by_HQ : PreloadedDataVerifierTestContext
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
                IsLockedByHQ = true
            };

            userViewFactory.Setup(x => x.GetUser(Moq.It.IsAny<UserViewInputModel>())).Returns(user);

            importDataVerifier = CreatePreloadedDataVerifier(questionnaire, preloadedDataServiceMock.Object, userViewFactory.Object);
        };

        Because of =
            () =>
                result =
                    importDataVerifier.VerifyPanelFiles(questionnaireId, 1, new[] { preloadedDataByFile });

        It should_result_has_1_error = () =>
            result.Errors.Count().ShouldEqual(1);

        It should_return_single_PL0006_error = () =>
            result.Errors.First().Code.ShouldEqual("PL0027");

        It should_return_reference_with_Cell_type = () =>
            result.Errors.First().References.First().Type.ShouldEqual(PreloadedDataVerificationReferenceType.Cell);

        It should_error_PositionX_be_equal_to_0 = () =>
          result.Errors.First().References.First().PositionX.ShouldEqual(1);

        It should_error_PositionY_be_equal_to_1 = () =>
          result.Errors.First().References.First().PositionY.ShouldEqual(0);

        private static ImportDataVerifier importDataVerifier;
        private static ImportDataVerificationState result;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionnaireId;
        private static PreloadedDataByFile preloadedDataByFile;

        private static Mock<IPreloadedDataService> preloadedDataServiceMock;
        private const string QuestionnaireCsvFileName = "questionnaire.csv";
    }
}
