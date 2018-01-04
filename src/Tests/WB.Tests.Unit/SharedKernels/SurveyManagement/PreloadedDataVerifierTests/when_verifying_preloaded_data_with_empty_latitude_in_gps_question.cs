﻿using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Verifier;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;
using WB.Core.GenericSubdomains.Portable.Implementation.ServiceVariables;
using WB.Tests.Abc;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.PreloadedDataVerifierTests
{
    internal class when_verifying_preloaded_data_with_empty_latitude_in_gps_question : PreloadedDataVerifierTestContext
    {
        Establish context = () =>
        {
            questionnaireId = Guid.Parse("11111111111111111111111111111111");
            gpsQuestionId = Guid.Parse("21111111111111111111111111111111");
            var gpsQuestion = Create.Entity.GpsCoordinateQuestion(gpsQuestionId, "gps");

            questionnaire = CreateQuestionnaireDocumentWithOneChapter(gpsQuestion);
            questionnaire.Title = "questionnaire";
            preloadedDataByFile = CreatePreloadedDataByFile(
                new[] { ServiceColumns.InterviewId, "gps__Latitude", "gps__Longitude" },
                new[] { new[] { "1", "", "1.5" } },
                "questionnaire.csv");

            var preloadedDataService = Create.Service.PreloadedDataService(questionnaire);

            importDataVerifier = CreatePreloadedDataVerifier(questionnaire, preloadedDataService);
        };

        Because of = () => 
            importDataVerifier.VerifyPanelFiles(questionnaireId, 1, Create.Entity.PreloadedData(preloadedDataByFile), status);

        It should_return_1_error_PL0030 = () =>
            status.VerificationState.Errors.Single().Code.ShouldEqual("PL0030");

        private static ImportDataVerifier importDataVerifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionnaireId;
        private static Guid gpsQuestionId;
        private static PreloadedDataByFile preloadedDataByFile;
    }
}