﻿using System;
using System.Collections.Generic;
using System.ServiceModel;
using Machine.Specifications;
using Machine.Specifications.Utility;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.UI.Designer.WebServices;
using WB.UI.Designer.WebServices.Questionnaire;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Applications.Designer.PublicServiceTests
{
    internal class when_downloading_questionnaire_and_verifier_returns_errors : PublicServiceTestContext
    {
        Establish context = () =>
        {
            var supportedQuestionnaireVersion = new QuestionnaireVersion(0, 0, 1);

            var questionnaireId = Guid.Parse("11111111111111111111111111111111");

            request = CreateDownloadQuestionnaireRequest(questionnaireId, supportedQuestionnaireVersion);

            var templateInfo = CreateTemplateInfo(supportedQuestionnaireVersion);

            var exportService = new Mock<IQuestionnaireExportService>();
            exportService.Setup(x => x.GetQuestionnaireTemplateInfo(Moq.It.IsAny<QuestionnaireDocument>())).Returns(templateInfo);

            var questionnaireViewFactory = CreateQuestionnaireViewFactory(questionnaireId);            

            var questionnaireVerifier = new Mock<IQuestionnaireVerifier>();
            questionnaireVerifier.Setup(x => x.Verify(Moq.It.IsAny<QuestionnaireDocument>())).Returns(new List<QuestionnaireVerificationError>() { new QuestionnaireVerificationError("test", "t1", new QuestionnaireVerificationReference[0]) });

            service = CreatePublicService(exportService: exportService.Object, questionnaireVerifier: questionnaireVerifier.Object, questionnaireViewFactory: questionnaireViewFactory);
        };

        Because of = () => 
             exception = Catch.Exception(() => service.DownloadQuestionnaire(request));

        It should_throw_exception_of_type_InconsistentVersionException = () =>
            exception.ShouldBeOfExactType<FaultException>();

        It should_throw_exception_that_contains_such_words = () =>
            (new[] { "requested questionnaire", "has errors" }).Each(x => (exception as FaultException).Message.ToLower().ShouldContain(x));

        private static DownloadQuestionnaireRequest request;
        private static IPublicService service;
        private static Exception exception;
    }
}
